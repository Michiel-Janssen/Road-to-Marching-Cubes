using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public ComputeShader MarchingShader;
    public NoiseGenerator NoiseGenerator;
    public MeshFilter MeshFilter;
    public MeshCollider MeshCollider;

    [Space(10)]
    [SerializeField] private bool drawNoiseGizmos;

    Mesh _mesh;

    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public static int SizeOf => sizeof(float) * 3 * 3;
    }

    ComputeBuffer _trianglesBuffer;
    ComputeBuffer _trianglesCountBuffer;
    ComputeBuffer _weightsBuffer;

    float[] _weights;

    private void Awake()
    {
        CreateBuffers();
    }

    void Start()
    {
        _weights = NoiseGenerator.GetNoise();

        _mesh = new Mesh();

        UpdateMesh();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    void CreateBuffers()
    {
        /*
         * The 5 here is because in all the cube configuration there can be only a max of 5 triangles
         */
        _trianglesBuffer = new ComputeBuffer(5 * (GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk), Triangle.SizeOf, ComputeBufferType.Append);
        _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float));
    }

    int ReadTriangleCount()
    {
        int[] triCount = { 0 };
        ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
        _trianglesCountBuffer.GetData(triCount);
        return triCount[0];
    }

    Mesh ConstructMesh()
    {
        int kernel = MarchingShader.FindKernel("March");

        MarchingShader.SetBuffer(kernel, "_Triangles", _trianglesBuffer);
        MarchingShader.SetBuffer(kernel, "_Weights", _weightsBuffer);

        MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        MarchingShader.SetFloat("_IsoLevel", .5f);

        _weightsBuffer.SetData(_weights);
        _trianglesBuffer.SetCounterValue(0);

        MarchingShader.Dispatch(kernel, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads);

        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        _trianglesBuffer.GetData(triangles);

        return CreateMeshFromTriangles(triangles);
    }

    Mesh CreateMeshFromTriangles(Triangle[] triangles)
    {
        Vector3[] verts = new Vector3[triangles.Length * 3];
        int[] tris = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++)
        {
            int startIndex = i * 3;
            verts[startIndex] = triangles[i].a;
            verts[startIndex + 1] = triangles[i].b;
            verts[startIndex + 2] = triangles[i].c;
            tris[startIndex] = startIndex;
            tris[startIndex + 1] = startIndex + 1;
            tris[startIndex + 2] = startIndex + 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    void UpdateMesh()
    {
        Mesh mesh = ConstructMesh();
        MeshFilter.sharedMesh = mesh;
        MeshCollider.sharedMesh = mesh;
    }

    void ReleaseBuffers()
    {
        _trianglesBuffer.Release();
        _trianglesCountBuffer.Release();
        _weightsBuffer.Release();
    }

    public void EditWeights(Vector3 hitPosition, float brushSize, bool add)
    {
        int kernel = MarchingShader.FindKernel("UpdateWeights");

        _weightsBuffer.SetData(_weights);
        MarchingShader.SetBuffer(kernel, "_Weights", _weightsBuffer);

        MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        MarchingShader.SetVector("_HitPosition", hitPosition);
        MarchingShader.SetFloat("_BrushSize", brushSize);

        MarchingShader.SetFloat("_TerraformStrength", add ? 1f : -1f);

        MarchingShader.Dispatch(kernel, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads);

        _weightsBuffer.GetData(_weights);

        UpdateMesh();
    }

    private void OnDrawGizmos()
    {
        if (drawNoiseGizmos)
        {
            if (_weights == null || _weights.Length == 0)
            {
                return;
            }
            for (int x = 0; x < GridMetrics.PointsPerChunk; x++)
            {
                for (int y = 0; y < GridMetrics.PointsPerChunk; y++)
                {
                    for (int z = 0; z < GridMetrics.PointsPerChunk; z++)
                    {
                        int index = x + GridMetrics.PointsPerChunk * (y + GridMetrics.PointsPerChunk * z);
                        float noiseValue = _weights[index];
                        Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
                        Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * .2f);
                    }
                }
            }
        }
    }
}
