using gomoru.su.LightController.API;
using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightController))]
    public sealed class LightControllerEditor : Editor
    {
        private static Material _referenceMaterial;

        private void OnEnable()
        {
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            foreach(var x in (target as Component).GetComponentsInChildren<ShaderSettings>())
            {
                EditorGUILayout.ObjectField(GUIContent.none, x, typeof(ShaderSettings), false);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LightController.Excludes)));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
