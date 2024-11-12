using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public class PoissonTest : MonoBehaviour
    {
        public float radius = 1f;
        public Vector2 regionSize = Vector2.one;
        public int rejectionSamples = 30;
        public float displayRadius = 1f;
        public bool withNoise;

        [SerializeField] private Renderer textureRenderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        private Texture2D vegetationNoiseTexture;

        private List<Vector2> points;


        private void OnValidate()
        {
            vegetationNoiseTexture = GenerateVegetationTexture(100, 100, 40, 0.55f, 2, new Vector2(0, 0));
            points = PoissonDiscSampling.GeneratePoints(radius, regionSize, vegetationNoiseTexture, withNoise, rejectionSamples);
            textureRenderer.sharedMaterial.mainTexture = vegetationNoiseTexture;
            textureRenderer.transform.localScale = new Vector3(vegetationNoiseTexture.width, 1, vegetationNoiseTexture.height);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(regionSize / 2, regionSize);
            if (points != null)
            {
                foreach (Vector2 point in points)
                {
                    Vector3 location = new Vector3(point.x, 5, point.y);
                    Debug.DrawRay(location, Vector3.down * 15, Color.green);
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(point.x, 5, point.y), Vector3.down, out hit, 15))
                    {
                        Gizmos.DrawSphere(location, displayRadius);
                    }
                }
            }
        }

        public Texture2D GenerateVegetationTexture(int mapWidth, int mapHeight, float scale, float vegetationPercentage, int seed, Vector2 offset)
        {
            vegetationNoiseTexture = new Texture2D(mapWidth, mapHeight);
            System.Random prng = new System.Random(seed);
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sample = Mathf.PerlinNoise((x + offsetX) / scale, (y + offsetY) / scale);
                    Color color = (sample > 1 - vegetationPercentage) ? Color.white : Color.black;
                    vegetationNoiseTexture.SetPixel(x, y, color);
                }
            }

            vegetationNoiseTexture.Apply();
            return vegetationNoiseTexture;
        }
    }
}
