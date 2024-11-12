using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    /*public static class PoissonDiscSampling
    {
        public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, Texture2D vegetationNoiseTexture, int numSamplesBeforeRejection = 30)
        {
            // a sqaure = b square + c square, radius is de schuine zijde in het vierkant omgerekend is onderstaande functie de oplossing om de overige zijde te berekenen aka de size van 1 cell
            float cellSize = radius / Mathf.Sqrt(2);

            // Integer array, vb sampleRegionsize is 6 op 4, cellsize is 2 dus de grid wordt een 3 op 2 met gridcellen van 2 breed.
            int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            //Midden
    *//*        for (int y = 0; y < vegetationNoiseTexture.height; y++)
            {
                for (int x = 0; x < vegetationNoiseTexture.width; x++)
                {
                    Color pixelColor = vegetationNoiseTexture.GetPixel(x, y);
                    Debug.Log(pixelColor.grayscale);
                    if (pixelColor.grayscale > 0.5f)
                    {
                        spawnPoints.Add(new Vector2(x, y));
                    }
                }
            }*//*
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                Vector2 spawnCenter = spawnPoints[spawnIndex];

                bool candidateAccepted = false;
                for (int i = 0; i < numSamplesBeforeRejection; i++)
                {
                    float angle = Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = spawnCenter + dir * Random.Range(radius, 2 * radius);
                    if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
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

        private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
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
                return true;
            }

            return false;
        }
    }*/

    public static class PoissonDiscSampling
    {
        public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, Texture2D vegetationNoiseTexture, int numSamplesBeforeRejection = 30)
        {
            float cellSize = radius / Mathf.Sqrt(2);

            int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            for (int x = 0; x < vegetationNoiseTexture.width; x++)
            {
                for (int y = 0; y < vegetationNoiseTexture.height; y++)
                {
                    Color pixelColor = vegetationNoiseTexture.GetPixel(x, y);
                    if (pixelColor.grayscale > 0.5f)
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
                    if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid, vegetationNoiseTexture))
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

        static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid, Texture2D vegetationNoiseTexture)
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
                Vector2 pixelUV = new Vector2(1 - candidate.x / sampleRegionSize.x, 1 - candidate.y / sampleRegionSize.y);
                Color pixelColor = vegetationNoiseTexture.GetPixelBilinear(pixelUV.x, pixelUV.y);

                if (pixelColor.grayscale > 0.5f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /*public static List<Vector2> GeneratePoints(float radius, Texture2D vegetationNoiseTexture, int numSamplesBeforeRejection = 30)
        {
            float cellSize = radius / Mathf.Sqrt(2);
            Debug.Log(vegetationNoiseTexture.width);

            int[,] grid = new int[vegetationNoiseTexture.width, vegetationNoiseTexture.height];
            List<Vector2> points = new List<Vector2>();

            for (int x = 0; x < vegetationNoiseTexture.width; x++)
            {
                for (int y = 0; y < vegetationNoiseTexture.height; y++)
                {
                    Color pixelColor = vegetationNoiseTexture.GetPixel(x, y);
                    if (pixelColor.grayscale > 0.5f) // Check if the pixel is white
                    {
                        ExploreFromPoint(new Vector2(x, y), radius, vegetationNoiseTexture, numSamplesBeforeRejection, points, grid);
                    }
                }
            }

            return points;
        }

        private static void ExploreFromPoint(Vector2 spawnCentre, float radius, Texture2D vegetationNoiseTexture, int numSamplesBeforeRejection, List<Vector2> points, int[,] grid)
        {
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(spawnCentre);

            while (queue.Count > 0)
            {
                Vector2 currentSpawnPoint = queue.Dequeue();

                // Perform Poisson disc sampling around the current white pixel
                for (int i = 0; i < numSamplesBeforeRejection; i++)
                {
                    float angle = Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = currentSpawnPoint + dir * Random.Range(radius, 2 * radius);

                    if (IsValid(candidate, vegetationNoiseTexture) && grid[(int)candidate.x, (int)candidate.y] == 0)
                    {
                        points.Add(candidate);
                        queue.Enqueue(candidate);
                        grid[(int)candidate.x, (int)candidate.y] = 1;
                    }
                }
            }
        }

        private static bool IsValid(Vector2 candidate, Texture2D vegetationNoiseTexture)
        {
            // Check if the candidate point is within the bounds of the texture
            if (candidate.x >= 0 && candidate.x < vegetationNoiseTexture.width && candidate.y >= 0 && candidate.y < vegetationNoiseTexture.height)
            {
                // Check if the pixel color at the candidate point is white
                Color pixelColor = vegetationNoiseTexture.GetPixel((int)candidate.x, (int)candidate.y);
                return pixelColor.grayscale > 0.5f;
            }
            return false;
        }*/
    }
}