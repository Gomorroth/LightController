﻿using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightController))]
    [CanEditMultipleObjects]
    public sealed class LightControllerEditor : Editor
    {
        private SerializedProperty SaveParameters;
        private SerializedProperty SyncSettings;
        private SerializedProperty AddLightingControl;
        private SerializedProperty LightMaxLimitMax;
        private SerializedProperty AddBacklightControl;
        private SerializedProperty AddDistanceFadeControl;
        private SerializedProperty DistanceFadeEndMax;
        private SerializedProperty DefaultParameters;
        private SerializedProperty AddResetButton;
        private SerializedProperty Excludes;

        private static Material _referenceMaterial;

        private void OnEnable()
        {
            SaveParameters = serializedObject.FindProperty(nameof(LightController.SaveParameters));
            SyncSettings = serializedObject.FindProperty(nameof(LightController.SyncSettings));
            AddLightingControl = serializedObject.FindProperty(nameof(LightController.AddLightingControl));
            LightMaxLimitMax = serializedObject.FindProperty(nameof(LightController.LightMaxLimitMax));
            AddBacklightControl = serializedObject.FindProperty(nameof(LightController.AddBacklightControl));
            AddDistanceFadeControl = serializedObject.FindProperty(nameof(LightController.AddDistanceFadeControl));
            DistanceFadeEndMax = serializedObject.FindProperty(nameof(LightController.DistanceFadeEndMax));
            AddResetButton = serializedObject.FindProperty(nameof(LightController.AddResetButton));
            DefaultParameters = serializedObject.FindProperty(nameof(LightController.DefaultParameters));
            Excludes = serializedObject.FindProperty(nameof(LightController.Excludes));
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(SaveParameters);
            EditorGUILayout.PropertyField(SyncSettings);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(AddLightingControl);
            EditorGUI.BeginDisabledGroup(!AddLightingControl.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(LightMaxLimitMax);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(AddBacklightControl);
            EditorGUILayout.PropertyField(AddDistanceFadeControl);
            EditorGUI.BeginDisabledGroup(!AddDistanceFadeControl.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(DistanceFadeEndMax);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(AddResetButton);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(DefaultParameters);
            if (DefaultParameters.isExpanded)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                _referenceMaterial = EditorGUILayout.ObjectField(GUIContent.none, _referenceMaterial, typeof(Material), true) as Material;
                EditorGUI.BeginDisabledGroup(_referenceMaterial == null);
                if (GUILayout.Button("Load"))
                {
                    foreach(var target in targets)
                    {
                        Utils.SetParametersFromMaterial((target as LightController).DefaultParameters, _referenceMaterial);
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(Excludes);

            EditorGUILayout.Separator();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
