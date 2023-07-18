using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightControllerGenerator))]
    public sealed class LightControllerGeneratorEditor : Editor
    {
        private SerializedProperty LightMaxLimitMax;
        private SerializedProperty SaveParameters;
        //private SerializedProperty AddLightingControl;
        private SerializedProperty AddBacklightControl;
        private SerializedProperty UseMaterialPropertyAsDefault;
        private SerializedProperty DefaultParameters;

        private bool _debugFoldoutOpen;

        private void OnEnable()
        {
            LightMaxLimitMax = serializedObject.FindProperty(nameof(LightControllerGenerator.LightMaxLimitMax));
            SaveParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.SaveParameters));
            //AddLightingControl = serializedObject.FindProperty(nameof(LightControllerGenerator.AddLightingControl));
            AddBacklightControl = serializedObject.FindProperty(nameof(LightControllerGenerator.AddBacklightControl));
            UseMaterialPropertyAsDefault = serializedObject.FindProperty(nameof(LightControllerGenerator.UseMaterialPropertyAsDefault));
            DefaultParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.DefaultParameters));
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(LightMaxLimitMax, Label("Maximum value of Light Max Limit"));
            EditorGUILayout.PropertyField(SaveParameters);
            //EditorGUILayout.PropertyField(AddLightingControl);
            EditorGUILayout.PropertyField(AddBacklightControl);
            EditorGUILayout.PropertyField(UseMaterialPropertyAsDefault, Label("Use material proeperty value as Default value"));
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
