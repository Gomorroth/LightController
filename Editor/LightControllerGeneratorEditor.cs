using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightControllerGenerator))]
    public sealed class LightControllerGeneratorEditor : Editor
    {
        private SerializedProperty LightMaxLimitMax;
        private SerializedProperty UseMaterialPropertyAsDefault;
        private SerializedProperty DefaultParameters;

        private SerializedProperty LightMinLimit;
        private SerializedProperty LightMaxLimit;
        private SerializedProperty MonochromeLighting;
        private SerializedProperty ShadowEnvStrength;
        private SerializedProperty AsUnlit;
        private SerializedProperty VertexLightStrength;

        private void OnEnable()
        {
            LightMaxLimitMax = serializedObject.FindProperty(nameof(LightControllerGenerator.LightMaxLimitMax));
            UseMaterialPropertyAsDefault = serializedObject.FindProperty(nameof(LightControllerGenerator.UseMaterialPropertyAsDefault));
            var param = DefaultParameters = serializedObject.FindProperty(nameof(LightControllerGenerator.DefaultParameters));

            LightMinLimit = param.FindPropertyRelative(nameof(LilToonParameters.LightMinLimit));
            LightMaxLimit = param.FindPropertyRelative(nameof(LilToonParameters.LightMaxLimit));
            MonochromeLighting = param.FindPropertyRelative(nameof(LilToonParameters.MonochromeLighting));
            ShadowEnvStrength = param.FindPropertyRelative(nameof(LilToonParameters.ShadowEnvStrength));
            AsUnlit = param.FindPropertyRelative(nameof(LilToonParameters.AsUnlit));
            VertexLightStrength = param.FindPropertyRelative(nameof(LilToonParameters.VertexLightStrength));
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(LightMaxLimitMax, Label("Maximum value of Light Max Limit"));
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
