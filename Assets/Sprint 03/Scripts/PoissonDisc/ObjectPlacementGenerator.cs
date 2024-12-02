using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoffeeBytes.Week3
{
    public class ObjectPlacementGenerator : MonoBehaviour
    {
        [Header("Poisson Disc Settings")]
        [SerializeField] private Vector2 regionSize = Vector2.one;
        [SerializeField] private float rayStartHeight = 100;

        private float boundsSize = 20f;

        private float rayDist;
        //private List<Vector2> points;
        private MapGenerator mapGenerator;

        private void Awake()
        {
            mapGenerator = GetComponent<MapGenerator>();
        }

        public void GenerateTrees(VegetationData vegetationData)
        {
            rayDist = rayStartHeight;

            foreach (var vegetationType in vegetationData.vegetationTypes)
            {
                Texture2D vegetationNoiseMap = new Texture2D(50, 50);
                List<Vector2> points;

                if (vegetationType.useNoise)
                {
                    vegetationNoiseMap = Noise.GenerateVegetationTexture(50, 50, vegetationType.noiseScale, vegetationType.noiseSeed, vegetationType.noiseOffset);
                }

                points = PoissonDiscSampling.GeneratePoints(vegetationType.radius, regionSize, vegetationNoiseMap, vegetationType.densityCurve, vegetationType.rejectionSamples);
                
                foreach (Vector2 point in points)
                {
                    RaycastHit hit;
                    Vector3 rayOrigin = new Vector3(point.x + transform.position.x, rayStartHeight, point.y + transform.position.y);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDist))
                    {
                        float randomYRotation = Random.Range(0f, 360f);
                        Quaternion treeQuaternion = Quaternion.Euler(0, randomYRotation, 0);

                        if (vegetationType.useTilt)
                        {
                            treeQuaternion = RandomRotations(vegetationType.tiltScale);
                        }

                        bool validOnSlope = false;

                        if (vegetationType.useSlope)
                        {
                            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                            if (slopeAngle >= vegetationType.minSlopeAngle && slopeAngle <= vegetationType.maxSlopeAngle)
                            {
                                validOnSlope = true;
                            }
                        }
                        else
                        {
                            validOnSlope = true;
                        }

                        if (validOnSlope)
                        {
                            Instantiate(vegetationType.prefabs[Random.Range(0, vegetationType.prefabs.Length)], hit.point, treeQuaternion, transform);
                        }
                    }
                }
            }
        }

        private Quaternion RandomRotations(float tiltScale)
        {
            float randomYRotation = Random.Range(0f, 360f);
            float randomXRotation = Random.Range(0f, tiltScale);
            float randomZRotation = Random.Range(0f, tiltScale);

            Quaternion randomRotation = Quaternion.Euler(randomXRotation, randomYRotation, randomZRotation);
            return randomRotation;
        }
    }
}