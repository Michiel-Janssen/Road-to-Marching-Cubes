using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public class MapDisplay : MonoBehaviour
    {
        [SerializeField] private Renderer textureRenderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        private void Start()
        {
            ShowTexture();
        }

        private void ShowTexture()
        {
            Texture2D vegetationNoiseMap = Noise.GenerateVegetationTexture(50, 50, 5, 2, new Vector2(0, 0));
            DrawTexture(vegetationNoiseMap);
        }

        public void DrawTexture(Texture2D texture)
        {
            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }
    }
}
