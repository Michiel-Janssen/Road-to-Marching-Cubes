using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public static class Noise
    {
        public static Texture2D GenerateVegetationTexture(int mapWidth, int mapHeight, float scale, int seed, Vector2 offset)
        {
            Texture2D texture = new Texture2D(mapWidth, mapHeight);
            System.Random prng = new System.Random(seed);
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sample = Mathf.PerlinNoise((x + offsetX) / mapWidth * scale, (y + offsetY) / mapHeight * scale);
                    texture.SetPixel(x, y, new Color(0, sample, 0));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}