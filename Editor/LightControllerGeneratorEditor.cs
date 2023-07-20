using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightControllerGenerator))]
    public sealed class LightControllerGeneratorEditor : Editor
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

        private bool _debugFoldoutOpen;

        private void OnEnable()
        {
            SaveParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.SaveParameters));
            SyncSettings = serializedObject.FindProperty(nameof(LightControllerGenerator.SyncSettings));
            AddLightingControl = serializedObject.FindProperty(nameof(LightControllerGenerator.AddLightingControl));
            LightMaxLimitMax = serializedObject.FindProperty(nameof(LightControllerGenerator.LightMaxLimitMax));
            AddBacklightControl = serializedObject.FindProperty(nameof(LightControllerGenerator.AddBacklightControl));
            AddDistanceFadeControl = serializedObject.FindProperty(nameof(AddDistanceFadeControl));
            DistanceFadeEndMax = serializedObject.FindProperty(nameof(DistanceFadeEndMax));
            UseMaterialPropertyAsDefault = serializedObject.FindProperty(nameof(LightControllerGenerator.UseMaterialPropertyAsDefault));
            DefaultParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.DefaultParameters));
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

            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(UseMaterialPropertyAsDefault.boolValue);
            EditorGUILayout.PropertyField(DefaultParameters);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (_debugFoldoutOpen = EditorGUILayout.Foldout(_debugFoldoutOpen, Label("Debug")))
            {
                var generator = target as LightControllerGenerator;
                var avatar = generator?.GetComponentInParent<VRCAvatarDescriptor>();
                EditorGUI.BeginDisabledGroup(generator == null || avatar == null);
                if (GUILayout.Button("Generate Manually"))
                {
                    Generator.Generate(avatar.gameObject, generator);
                    AssetDatabase.SaveAssets();
                    generator.enabled = false;
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private static readonly GUIContent _labelSingleton = new GUIContent();

        private static GUIContent Label(string text)
        {
            var label = _labelSingleton;
            label.text = text;
            return label;
        }
    }
}
