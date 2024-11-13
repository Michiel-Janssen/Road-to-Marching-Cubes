using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public enum VegetationTypeCategory
    {
        Tree,
        Rock,
        Grass,
        Bush
    }

    [CreateAssetMenu()]
    public class VegetationData : UpdatableData
    {
        public VegetationType[] vegetationTypes;
    }

    [System.Serializable]
    public struct VegetationType
    {
        public VegetationTypeCategory category;
        public GameObject[] prefabs;
        public AnimationCurve densityCurve;

        public float radius;
        public int rejectionSamples;

        public bool useSlope;
        public float minSlopeAngle;
        public float maxSlopeAngle;

        public bool useNoise;
        public float noiseScale;
        public int noiseSeed;
        public Vector2 noiseOffset;

        public bool useTilt;
        public float tiltScale;
    }
}