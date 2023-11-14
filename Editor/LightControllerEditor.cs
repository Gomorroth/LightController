using System;
using System.Linq;
using gomoru.su.LightController.API;
using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(LightController))]
    public sealed class LightControllerEditor : Editor
    {
        private static Material _referenceMaterial;

        private (ShaderSettings Settings, SerializedObject SerializedObject)[] _settings;
        private static bool[] _foldOuts;

        private void OnEnable()
        {
            _settings = (target as Component).GetComponentsInChildren<ShaderSettings>().Select(x => (x, new SerializedObject(x))).ToArray();
            foreach(var (setting, _) in _settings)
            {
                setting.hideFlags |= HideFlags.HideInInspector;
            }
            
            if ((_foldOuts?.Length ?? -1) < _settings.Length)
                Array.Resize(ref _foldOuts, _settings.Length);
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            var lc = target as LightController;
            bool lcChanged = false;

            _ = _foldOuts.Length;
            for (int i = 0; i < _settings.Length; i++)
            {
                var (settings, serializedObject) = _settings[i];
                var qn = settings.QualifiedName;
                bool foldOut = lc.ExpandedGroups.Contains(qn);
                bool prev = foldOut;
                if (foldOut = EditorGUILayout.Foldout(foldOut, settings.DisplayName, true))
                {
                    EditorGUI.indentLevel++;
                    settings.OnInspectorGUI(serializedObject);
                    EditorGUI.indentLevel--;
                }
                if (foldOut != prev)
                {
                    if (foldOut)
                        lc.ExpandedGroups.Add(qn);
                    else
                        lc.ExpandedGroups.Remove(qn);
                    lcChanged = true;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LightController.Excludes)));

            serializedObject.ApplyModifiedProperties();

            if (lcChanged)
            {
                EditorUtility.SetDirty(lc);
            }
        }
    }
}
