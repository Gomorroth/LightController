using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightController))]
    [CanEditMultipleObjects]
    public sealed class LightControllerEditor : Editor
    {
        private static Material _referenceMaterial;

        private void OnEnable()
        {
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
