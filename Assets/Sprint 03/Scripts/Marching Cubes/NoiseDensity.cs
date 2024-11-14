using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public class NoiseDensity : DensityGenerator
    {
        [Header("Noise")]
        public int seed;
        [Tooltip("Octaves add layers of detail to procedural noise. Higher octaves introduce finer features like small bumps or rough terrain, while lower octaves control larger structures like hills. Adjusting octaves helps balance between large, smooth features and fine details.")]
        public int numOctaves = 4;
        [Tooltip("Lacunarity controls how quickly detail increases with each octave, scaling the frequency to add finer features at higher octaves.")]
        public float lacunarity = 2f;
        [Tooltip("Persistence controls how much each octave contributes, with higher values emphasizing finer details in the terrain.")]
        public float persistence = 0.5f;
        [Tooltip("Noise Scale adjusts the overall size of the noise features, affecting how zoomed in or out the noise appears in the terrain.")]
        public float noiseScale = 1f;
        [Tooltip("Noise Weight determines the influence of the noise on the final terrain height, allowing you to blend it with other height sources effectively.")]
        public float noiseWeight = 1f;
        public bool closeEdges;
        [Tooltip("Floor Offset shifts the base height of the terrain, allowing you to raise or lower the entire landscape without changing its shape.")]
        public float floorOffset = 1f;
        [Tooltip("Weight Multiplier scales the overall contribution of the noise to the terrain height, effectively enhancing or reducing its impact on the final terrain.")]
        public float weightMultiplier = 1f;

        [Tooltip("Hard Floor Height sets a fixed elevation limit for the terrain, preventing any generated terrain from falling below this specified height.")]
        public float hardFloorHeight;
        [Tooltip("Hard Floor Weight determines how strongly the hard floor height influences the terrain, affecting the transition between the terrain and the hard floor limit.")]
        public float hardFloorWeight;

        public Vector4 shaderParams;

        public override ComputeBuffer Generate(ComputeBuffer _weightsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing)
        {
            buffersToRelease = new List<ComputeBuffer>();

            var prng = new System.Random(seed);
            var offsets = new Vector3[numOctaves];
            float offsetRange = 1000f;
            for(int i = 0; i < numOctaves; i++)
            {
                offsets[i] = new Vector3((float) prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1) * offsetRange;
            }

            var offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);
            offsetsBuffer.SetData(offsets);
            buffersToRelease.Add(offsetsBuffer);

            densityShader.SetVector("centre", new Vector4(centre.x, centre.y, centre.z));
            densityShader.SetInt("octaves", Mathf.Max(1, numOctaves));
            densityShader.SetFloat("lacunarity", lacunarity);
            densityShader.SetFloat("persistence", persistence);
            densityShader.SetFloat("noiseScale", noiseScale);
            densityShader.SetFloat("noiseWeight", noiseWeight);
            densityShader.SetBool("closeEdges", closeEdges);
            densityShader.SetBuffer(0, "offsets", offsetsBuffer);
            densityShader.SetFloat("floorOffset", floorOffset);
            densityShader.SetFloat("weightMultiplier", weightMultiplier);
            densityShader.SetFloat("hardFloor", hardFloorHeight);
            densityShader.SetFloat("hardFloorWeight", hardFloorWeight);

            densityShader.SetVector("params", shaderParams);

            return base.Generate(_weightsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
        }
    }
}
