using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [Range(0, 4)]
    public int LOD;

    public ComputeShader MarchingShader;
    public NoiseGenerator NoiseGenerator;
    public MeshFilter MeshFilter;
    public MeshCollider MeshCollider;

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

    void Start()
    {
        Create();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Create();
        }
    }

    void Create()
    {
        CreateBuffers();

        if (_weights == null)
        {
            _weights = NoiseGenerator.GetNoise(GridMetrics.LastLod);
        }

        _mesh = new Mesh();

        UpdateMesh();

        ReleaseBuffers();
    }

    void CreateBuffers()
    {
        /*
         * The 5 here is because in all the cube configuration there can be only a max of 5 triangles
         */
        _trianglesBuffer = new ComputeBuffer(5 * (GridMetrics.PointsPerChunk(LOD) * GridMetrics.PointsPerChunk(LOD) * GridMetrics.PointsPerChunk(LOD)), Triangle.SizeOf, ComputeBufferType.Append);
        _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk(GridMetrics.LastLod) * GridMetrics.PointsPerChunk(GridMetrics.LastLod) * GridMetrics.PointsPerChunk(GridMetrics.LastLod), sizeof(float));
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

        float lodScaleFactor = ((float)GridMetrics.PointsPerChunk(GridMetrics.LastLod) + 1) / (float)GridMetrics.PointsPerChunk(LOD);

        MarchingShader.SetFloat("_LodScaleFactor", lodScaleFactor);
        MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(GridMetrics.LastLod));
        MarchingShader.SetInt("_LODSize", GridMetrics.PointsPerChunk(LOD));
        MarchingShader.SetInt("_Scale", GridMetrics.Scale);
        MarchingShader.SetFloat("_IsoLevel", .5f);

        _weightsBuffer.SetData(_weights);
        _trianglesBuffer.SetCounterValue(0);

        MarchingShader.Dispatch(kernel, GridMetrics.ThreadGroups(LOD), GridMetrics.ThreadGroups(LOD), GridMetrics.ThreadGroups(LOD));

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

    public void EditWeights(Vector3 hitPosition, float brushSize, bool add, float BrushStrength)
    {
        CreateBuffers();

        int kernel = MarchingShader.FindKernel("UpdateWeights");

        _weightsBuffer.SetData(_weights);
        MarchingShader.SetBuffer(kernel, "_Weights", _weightsBuffer);

        MarchingShader.SetInt("_Scale", GridMetrics.Scale);
        MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(GridMetrics.LastLod));
        MarchingShader.SetVector("_HitPosition", hitPosition);
        MarchingShader.SetFloat("_BrushSize", brushSize);
        MarchingShader.SetFloat("_TerraformStrength", add ? BrushStrength : -BrushStrength);

        MarchingShader.Dispatch(kernel, GridMetrics.ThreadGroups(GridMetrics.LastLod), GridMetrics.ThreadGroups(GridMetrics.LastLod), GridMetrics.ThreadGroups(GridMetrics.LastLod));

        _weightsBuffer.GetData(_weights);

        UpdateMesh();

        ReleaseBuffers();
    }
}
