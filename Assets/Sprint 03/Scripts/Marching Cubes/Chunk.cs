using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public class Chunk : MonoBehaviour
    {
        public Vector3Int coord;

        [HideInInspector]
        public Mesh mesh;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;
        bool generateCollider;

        public void DestroyOrDisable()
        {
            if (Application.isPlaying)
            {
                mesh.Clear();
                gameObject.SetActive(false);
            }
            else
            {
                DestroyImmediate(gameObject, false);
            }
        }

        public void SetUp(Material mat, bool generateCollider)
        {
            this.generateCollider = generateCollider;

            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();

            if (meshFilter == null)
            { 
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if(meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            if(meshCollider == null && generateCollider)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }

            if (meshCollider != null && !generateCollider)
            {
                DestroyImmediate(meshCollider);
            }

            mesh = meshFilter.sharedMesh;
            if(mesh == null)
            {
                mesh = new Mesh();
                //Remove line if to performant heavy, default is 16
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilter.sharedMesh = mesh;
            }

            if(generateCollider)
            {
                if (meshCollider.sharedMesh == null)
                {
                    meshCollider.sharedMesh = mesh;
                }

                //Forces the meshcollider to update to the new mesh data
                meshCollider.enabled = false;
                meshCollider.enabled = true;
            }

            meshRenderer.material = mat;
        }
    }
}
