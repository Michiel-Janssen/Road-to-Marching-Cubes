using CoffeeBytes.Week3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VegetationData))]
public class VegetationDataEditor : Editor
{
    private SerializedProperty vegetationTypes;

    private void OnEnable()
    {
        vegetationTypes = serializedObject.FindProperty("vegetationTypes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        for (int i = 0; i < vegetationTypes.arraySize; i++)
        {
            SerializedProperty vegetationType = vegetationTypes.GetArrayElementAtIndex(i);

            EditorGUILayout.LabelField($"Vegetation Type {i + 1}", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("category"));
            EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("prefabs"), true);

            GUILayout.Space(25);

            EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("densityCurve"));
            EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("radius"));
            EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("rejectionSamples"));

            GUILayout.Space(25);

            SerializedProperty useSlope = vegetationType.FindPropertyRelative("useSlope");
            useSlope.boolValue = EditorGUILayout.Toggle("Use Slope", useSlope.boolValue);

            if (useSlope.boolValue)
            {
                EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("minSlopeAngle"));
                EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("maxSlopeAngle"));
            }

            GUILayout.Space(25);

            SerializedProperty useNoiseProp = vegetationType.FindPropertyRelative("useNoise");
            useNoiseProp.boolValue = EditorGUILayout.Toggle("Use Noise", useNoiseProp.boolValue);

            if (useNoiseProp.boolValue)
            {
                EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("noiseScale"));
                EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("noiseSeed"));
                EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("noiseOffset"));
            }

            GUILayout.Space(25);

            SerializedProperty useTiltProp = vegetationType.FindPropertyRelative("useTilt");
            useTiltProp.boolValue = EditorGUILayout.Toggle("Use Tilt", useTiltProp.boolValue);

            if (useTiltProp.boolValue)
            {
                EditorGUILayout.PropertyField(vegetationType.FindPropertyRelative("tiltScale"));
            }

            GUILayout.Space(15);

            if (GUILayout.Button("Remove Vegetation Type"))
            {
                vegetationTypes.DeleteArrayElementAtIndex(i);
            }

            GUILayout.Space(25);
        }

        if (GUILayout.Button("Add Vegetation Type"))
        {
            vegetationTypes.InsertArrayElementAtIndex(vegetationTypes.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}