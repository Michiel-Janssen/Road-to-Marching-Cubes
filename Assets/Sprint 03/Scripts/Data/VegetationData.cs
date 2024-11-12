using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    [CreateAssetMenu()]
    public class VegetationData : UpdatableData
    {
        [Tooltip("This value changes the distance you view your noiseMap at")]
        public float noiseScale = 40f;
        [Tooltip("This value changes the threshold from black to white colors, meaning higher value, less vegetation")]
        [Range(0, 1)]
        public float vegetationPercentage = 0.5f;

        [Header("Seeds and offset")]
        [Tooltip("Starting point")]
        public int seed;
        [Tooltip("2 Values to offset the map")]
        public Vector2 offset;

        public VegetationType[] vegetationTypes;
    }

    [System.Serializable]
    public struct VegetationType
    {
        [SerializeField] private string category;
        public GameObject[] prefabs;
        public float density;
    }
}