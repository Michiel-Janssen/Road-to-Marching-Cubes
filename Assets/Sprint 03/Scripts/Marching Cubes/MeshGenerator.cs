using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    [ExecuteInEditMode]
    public class MeshGenerator : MonoBehaviour
    {
        [Range(0, 4)]
        public int LOD;

        [Header("General Settings")]
        public DensityGenerator densityGenerator;

        public Vector3Int numChunks = Vector3Int.one;
        public const float maxViewDst = 30;
        public Transform viewer;

        public bool fixedMapSize;

        [Space()]


        public static Vector2 viewerPosition;

        [Space()]
        public bool autoUpdateInEditor = true;
        public bool autoUpdateInGame = true;
        public ComputeShader marchingShader;
        public Material mat;
        public bool generateColliders;

        [Header("Voxel Settings")]
        public float isoLevel;
        public float boundsSize = 1;
        public Vector3 offset = Vector3.zero;

        [Header("Gizmos")]
        public bool showBoundsGizmo = true;
        public Color boundsGizmoCol = Color.white;

        List<Chunk> chunks;
        Dictionary<Vector3Int, Chunk> existingChunks;
        Queue<Chunk> recycleableChunks;

        struct Triangle
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
            
            public Vector3 this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return a;
                        case 1:
                            return b;
                        default:
                            return c;
                    }
                }
            }

            public static int SizeOf => sizeof(float) * 3 * 3;
        }

        GameObject chunkHolder;
        const string chunkHolderName = "Chunks Holder";

        ComputeBuffer _trianglesBuffer;
        ComputeBuffer _trianglesCountBuffer;
        ComputeBuffer _weightsBuffer;

        bool settingsUpdated;

        private void Awake()
        {
            if (Application.isPlaying && !fixedMapSize)
            {
                InitChunkStructs();
                DestroyOldChunks();
            }
        }

        private void Update()
        {
            if (Application.isPlaying && !fixedMapSize)
            {
                Run();
            }

            if (settingsUpdated)
            {
                Debug.Log("Updating mesh");
                RequestMeshUpdate();
                settingsUpdated = false;
            }
        }

        public void Run()
        {
            CreateBuffers();

            if (fixedMapSize)
            {
                InitChunks();
                UpdateAllChunks();
            }
            else
            {
                if (Application.isPlaying)
                {
                    InitVisibleChunks();
                }
            }

            if (!Application.isPlaying)
            {
                ReleaseBuffers();
            }
        }

        void InitVisibleChunks()
        {
            if(chunks == null)
            {
                return;
            }

            CreateChunkHolder();

            Vector3 p = viewer.position;
            Vector3 ps = p / boundsSize;
            Vector3Int viewerCoord = new Vector3Int(Mathf.RoundToInt(ps.x), Mathf.RoundToInt(ps.y), Mathf.RoundToInt(ps.z));

            int maxChunksInView = Mathf.CeilToInt(maxViewDst / boundsSize);
            float sqrViewDistance = maxViewDst * maxViewDst;

            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                Chunk chunk = chunks[i];
                Vector3 centre = CentreFromCoord(chunk.coord);
                Vector3 viewerOffset = p - centre;
                Vector3 o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) - Vector3.one * boundsSize / 2;
                float sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;
                if(sqrDst > sqrViewDistance)
                {
                    existingChunks.Remove(chunk.coord);
                    recycleableChunks.Enqueue(chunk);
                    chunks.RemoveAt(i);
                }
            }

            for (int x = -maxChunksInView; x <= maxChunksInView; x++)
            {
                for (int y = -maxChunksInView; y <= maxChunksInView; y++)
                {
                    for (int z = -maxChunksInView; z <= maxChunksInView; z++)
                    {
                        Vector3Int coord = new Vector3Int(x, y, z) + viewerCoord;

                        if (existingChunks.ContainsKey(coord))
                        {
                            continue;
                        }

                        Vector3 centre = CentreFromCoord(coord);
                        Vector3 viewerOffset = p - centre;
                        Vector3 o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) - Vector3.one * boundsSize / 2;
                        float sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;

                        // Chunk is within view distance and should be created (if it doesn't already exist)
                        if (sqrDst <= sqrViewDistance)
                        {

                            Bounds bounds = new Bounds(CentreFromCoord(coord), Vector3.one * boundsSize);
                            if (IsVisibleFrom(bounds, Camera.main))
                            {
                                if (recycleableChunks.Count > 0)
                                {
                                    Chunk chunk = recycleableChunks.Dequeue();
                                    chunk.coord = coord;
                                    existingChunks.Add(coord, chunk);
                                    chunks.Add(chunk);
                                    UpdateChunkMesh(chunk);
                                }
                                else
                                {
                                    Chunk chunk = CreateChunk(coord);
                                    chunk.coord = coord;
                                    chunk.SetUp(mat, generateColliders);
                                    existingChunks.Add(coord, chunk);
                                    chunks.Add(chunk);
                                    UpdateChunkMesh(chunk);
                                }
                            }
                        }

                    }
                }
            }
        }

        public bool IsVisibleFrom(Bounds bounds, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }

        void UpdateAllChunks()
        {
            foreach(Chunk chunk in chunks)
            {
                Debug.Log($"Updating this chunk = {chunk}");
                UpdateChunkMesh(chunk);
            }
        }

        public void RequestMeshUpdate()
        {
            Debug.Log("Requesting mesh update...");
            if ((Application.isPlaying && autoUpdateInGame) || (!Application.isPlaying && autoUpdateInEditor))
            {
                Run();
            }
        }

        public void UpdateChunkMesh(Chunk chunk)
        {
            int kernelIndex = marchingShader.FindKernel("March");

            int numVoxelsPerAxis = GridMetrics.PointsPerChunk(GridMetrics.LastLod) - 1;
            //TODO: Check for size threadgroups
            //int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)GridMetrics.ThreadGroups(GridMetrics.LastLod));
            int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)8);
            float pointSpacing = boundsSize / (GridMetrics.PointsPerChunk(GridMetrics.LastLod) - 1);

            Vector3Int coord = chunk.coord;
            Vector3 centre = CentreFromCoord(coord);

            Vector3 worldBounds = new Vector3(numChunks.x, numChunks.y, numChunks.z) * boundsSize;

            densityGenerator.Generate(_weightsBuffer, GridMetrics.PointsPerChunk(GridMetrics.LastLod), boundsSize, worldBounds, centre, offset, pointSpacing);

            Debug.Log("Density generated for chunk at coordinates: " + coord);
            //float lodScaleFactor = ((float)GridMetrics.PointsPerChunk(GridMetrics.LastLod) + 1) / (float)GridMetrics.PointsPerChunk(LOD);

            _trianglesBuffer.SetCounterValue(0);

            //TODO SWITCH
            //marchingShader.SetBuffer(0, "weight", _weightsBuffer);
            marchingShader.SetBuffer(0, "points", _weightsBuffer);
            marchingShader.SetBuffer(0, "triangles", _trianglesBuffer);
/*            marchingShader.SetFloat("_LodScaleFactor", lodScaleFactor);
            marchingShader.SetInt("_LODSize", GridMetrics.PointsPerChunk(LOD));*/
            //TODO SWITCH
            //marchingShader.SetInt("pointsPerChunk", GridMetrics.PointsPerChunk(GridMetrics.LastLod));
            marchingShader.SetInt("numPointsPerAxis", GridMetrics.PointsPerChunk(GridMetrics.LastLod));
            //marchingShader.SetInt("_Scale", GridMetrics.Scale);
            marchingShader.SetFloat("isoLevel", isoLevel);

            Debug.Log("Dispatching marching compute shader");
            marchingShader.Dispatch(kernelIndex, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

            ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
            int[] triCountArray = { 0 };
            _trianglesCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];
            Debug.Log("Number of triangles generated: " + numTris);

            Triangle[] tris = new Triangle[numTris];
            _trianglesBuffer.GetData(tris, 0, 0, numTris);
            Debug.Log("Triangles buffer data length: " + tris.Length);

            Mesh mesh = chunk.mesh;
            mesh.Clear();

            var vertices = new Vector3[numTris * 3];
            var meshTriangles = new int[numTris * 3];

            for(int i = 0; i < numTris; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][j];
                }
            }
            for (int i = 0; i < Mathf.Min(10, vertices.Length); i++)
            {
                Debug.Log("Vertex " + i + ": " + vertices[i]);
            }

            mesh.vertices = vertices;
            mesh.triangles = meshTriangles;

            mesh.RecalculateNormals();
        }

/*        private int GetNumberOfTriangles()
        {
            ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
            int[] triCountArray = { 0 };
            _trianglesCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];
            return numTris;
        }*/

        Vector3 CentreFromCoord(Vector3Int coord)
        {
            if(fixedMapSize)
            {
                Vector3 totalBounds = (Vector3) numChunks * boundsSize;
                return -totalBounds / 2 + (Vector3) coord * boundsSize + Vector3.one * boundsSize / 2;
            }

            return new Vector3(coord.x, coord.y, coord.z) * boundsSize;
        }

        void CreateBuffers()
        {
            int numPoints = GridMetrics.PointsPerChunk(GridMetrics.LastLod) * GridMetrics.PointsPerChunk(GridMetrics.LastLod) * GridMetrics.PointsPerChunk(GridMetrics.LastLod);
            Debug.Log($"Amount of numPoints = {numPoints}");
            int numVoxelsPerAxis = GridMetrics.PointsPerChunk(GridMetrics.LastLod) - 1;
            int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
            int maxTriangleCount = numVoxels * 5;

            if (!Application.isPlaying || (_weightsBuffer == null || numPoints != _weightsBuffer.count))
            {
                if (Application.isPlaying)
                {
                    ReleaseBuffers();
                }
                Debug.Log($"Size of the triangle {Triangle.SizeOf}");
                _trianglesBuffer = new ComputeBuffer(maxTriangleCount, Triangle.SizeOf, ComputeBufferType.Append);
                _weightsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
                _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

                Debug.Log("Created the buffers");
            }
        }

        void InitChunks()
        {
            CreateChunkHolder();
            chunks = new List<Chunk>();
            List<Chunk> oldChunks = new List<Chunk>(FindObjectsOfType<Chunk>());

            for (int x = 0; x < numChunks.x; x++)
            {
                for (int y = 0; y < numChunks.y; y++)
                {
                    for (int z = 0; z < numChunks.z; z++)
                    {
                        Vector3Int coord = new Vector3Int(x, y, z);
                        bool chunksAlreadyExists = false;

                        for (int i = 0; i < oldChunks.Count; i++)
                        {
                            if (oldChunks[i].coord == coord)
                            {
                                chunks.Add(oldChunks[i]);
                                oldChunks.RemoveAt(i);
                                chunksAlreadyExists = true;
                                break;
                            }
                        }

                        if(!chunksAlreadyExists)
                        {
                            var newChunk = CreateChunk(coord);
                            chunks.Add(newChunk);
                        }

                        chunks[chunks.Count - 1].SetUp(mat, generateColliders);
                    }
                }
            }

            for (int i = 0; i < oldChunks.Count; i++)
            {
                oldChunks[i].DestroyOrDisable();
            }
        }

        Chunk CreateChunk(Vector3Int coord)
        {
            GameObject chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})");
            Debug.Log($"Chunk at {CentreFromCoord(coord)} created");
            chunk.transform.SetParent(chunkHolder.transform);
            Chunk newChunk = chunk.AddComponent<Chunk>();
            newChunk.coord = coord;
            return newChunk;
        }

        void CreateChunkHolder()
        {
            if(chunkHolder == null)
            {

                if(GameObject.Find(chunkHolderName))
                {
                    chunkHolder = GameObject.Find(chunkHolderName);
                }
                else
                {
                    chunkHolder = new GameObject(chunkHolderName);
                }
            }
        }

        void ReleaseBuffers()
        {
            if (_trianglesBuffer != null)
            {
                _trianglesBuffer.Release();
                _weightsBuffer.Release();
                _trianglesCountBuffer.Release();
                Debug.Log("Released the buffers");
            }
        }

        void InitChunkStructs()
        {
            recycleableChunks = new Queue<Chunk>();
            chunks = new List<Chunk>();
            existingChunks = new Dictionary<Vector3Int, Chunk>();

            Debug.Log("Initializing structs");
        }

        void DestroyOldChunks()
        {
            var oldChunks = FindObjectsOfType<Chunk>();
            foreach (Chunk chunk in oldChunks)
            {
                Destroy(chunk.gameObject);
            }

            Debug.Log("Destroyed old chunks");
        }

        void OnValidate()
        {
            settingsUpdated = true;
            Debug.Log($"On validate {settingsUpdated}");
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
            {
                ReleaseBuffers();
            }
        }

        void OnDrawGizmos()
        {
            if (showBoundsGizmo)
            {
                Gizmos.color = boundsGizmoCol;

                List<Chunk> chunks = (this.chunks == null) ? new List<Chunk>(FindObjectsOfType<Chunk>()) : this.chunks;
                foreach (var chunk in chunks)
                {
                    Bounds bounds = new Bounds(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
                    Gizmos.color = boundsGizmoCol;
                    Gizmos.DrawWireCube(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
                }
            }
        }
    }
}
