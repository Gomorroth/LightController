using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

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
        private SerializedProperty UseMaterialPropertyAsDefault;
        private SerializedProperty DefaultParameters;
        private SerializedProperty AddResetButton;

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
            UseMaterialPropertyAsDefault = serializedObject.FindProperty(nameof(LightController.UseMaterialPropertyAsDefault));
            DefaultParameters = serializedObject.FindProperty(nameof(LightController.DefaultParameters));
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
            EditorGUILayout.PropertyField(UseMaterialPropertyAsDefault);
            EditorGUILayout.PropertyField(AddResetButton);

            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(UseMaterialPropertyAsDefault.boolValue);
            EditorGUILayout.PropertyField(DefaultParameters);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
