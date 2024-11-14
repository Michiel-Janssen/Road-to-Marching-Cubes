using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public abstract class DensityGenerator : MonoBehaviour
    {
        public ComputeShader densityShader;

        protected List<ComputeBuffer> buffersToRelease;

        private void OnValidate()
        {
            if(FindFirstObjectByType<MeshGenerator>())
            {
                FindObjectOfType<MeshGenerator>().RequestMeshUpdate();
            }
        }

        public virtual ComputeBuffer Generate(ComputeBuffer _weightsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing)
        {
            int kernelIndex = densityShader.FindKernel("Density");

           /* int numPoints = GridMetrics.PointsPerChunk(GridMetrics.LastLod) * GridMetrics.PointsPerChunk(GridMetrics.LastLod) * GridMetrics.PointsPerChunk(GridMetrics.LastLod);*/
            int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / (float)GridMetrics.ThreadGroups(GridMetrics.LastLod));

            //TODO SWITCH
            densityShader.SetBuffer(0, "points", _weightsBuffer);
            //densityShader.SetBuffer(0, "weight", _weightsBuffer);
            //TODO SWITCH
            //densityShader.SetInt("pointsPerChunk", GridMetrics.PointsPerChunk(GridMetrics.LastLod));
            densityShader.SetInt("numPointsPerAxis", GridMetrics.PointsPerChunk(GridMetrics.LastLod));
            densityShader.SetFloat("boundsSize", boundsSize);
            densityShader.SetVector("centre", new Vector4(centre.x, centre.y, centre.z));
            densityShader.SetVector("offset", new Vector4(offset.x, offset.y, offset.z));
            densityShader.SetFloat("spacing", spacing);
            densityShader.SetVector("worldSize", worldBounds);

            densityShader.Dispatch(kernelIndex, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

            if (buffersToRelease != null)
            {
                foreach (var buffer in buffersToRelease)
                {
                    buffer.Release();
                }
            }

            return _weightsBuffer;
        }
    }
}
