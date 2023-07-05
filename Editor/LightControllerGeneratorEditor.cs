using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightControllerGenerator))]
    public sealed class LightControllerGeneratorEditor : Editor
    {
        private SerializedProperty LightMaxLimitMax;
        private SerializedProperty UseMaterialPropertyAsDefault;
        private SerializedProperty SaveParameters;
        private SerializedProperty DefaultParameters;

        private void OnEnable()
        {
            LightMaxLimitMax = serializedObject.FindProperty(nameof(LightControllerGenerator.LightMaxLimitMax));
            UseMaterialPropertyAsDefault = serializedObject.FindProperty(nameof(LightControllerGenerator.UseMaterialPropertyAsDefault));
            SaveParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.SaveParameters));
            DefaultParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.DefaultParameters));
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(LightMaxLimitMax, Label("Maximum value of Light Max Limit"));
            EditorGUILayout.PropertyField(SaveParameters);
            EditorGUILayout.PropertyField(UseMaterialPropertyAsDefault, Label("Use material proeperty value as Default value"));
            EditorGUI.BeginDisabledGroup(UseMaterialPropertyAsDefault.boolValue);
            EditorGUILayout.PropertyField(DefaultParameters);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
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
