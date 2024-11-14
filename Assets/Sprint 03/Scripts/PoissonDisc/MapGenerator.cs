using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBytes.Week3
{
    public class MapGenerator : MonoBehaviour
    {
        public VegetationData vegetationData;

        private ObjectPlacementGenerator placementGenerator;

        private void Awake()
        {
            placementGenerator = GetComponent<ObjectPlacementGenerator>();
        }

        private void Start()
        {
            placementGenerator.GenerateTrees(vegetationData);
            //placementGenerator.PlaceObjects();
        }
    }
}