using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public static class PoissonDiscSampling
    {
        public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, Texture2D vegetationNoiseTexture, AnimationCurve densityCurve, int numSamplesBeforeRejection = 30)
        {
            float cellSize = radius / Mathf.Sqrt(2);

            int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            if(vegetationNoiseTexture == null)
            {
                spawnPoints.Add(sampleRegionSize / 2);
            }
            else
            {
                for (int x = 0; x < vegetationNoiseTexture.width; x++)
                {
                    for (int y = 0; y < vegetationNoiseTexture.height; y++)
                    {
                        spawnPoints.Add(new Vector2(x, y));
                    }
                }
            }

            while (spawnPoints.Count > 0)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                Vector2 spawnCentre = spawnPoints[spawnIndex];
                bool candidateAccepted = false;

                for (int i = 0; i < numSamplesBeforeRejection; i++)
                {
                    float angle = Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius);

                    if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid, vegetationNoiseTexture, densityCurve))
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }

            }

            return points;
        }

        static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid, Texture2D vegetationNoiseTexture, AnimationCurve densityCurve)
        {
            if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
            {
                int cellX = (int)(candidate.x / cellSize);
                int cellY = (int)(candidate.y / cellSize);
                int searchStartX = Mathf.Max(0, cellX - 2);
                int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Mathf.Max(0, cellY - 2);
                int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                            if (sqrDst < radius * radius)
                            {
                                return false;
                            }
                        }
                    }
                }

                if (vegetationNoiseTexture != null)
                {
                    Vector2 pixelUV = new Vector2(candidate.x / sampleRegionSize.x, candidate.y / sampleRegionSize.y);
                    float noiseValue = vegetationNoiseTexture.GetPixelBilinear(pixelUV.x, pixelUV.y).g;

                    float placementProbability = densityCurve.Evaluate(noiseValue);

                    if (Random.value > placementProbability)
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }
    }
}