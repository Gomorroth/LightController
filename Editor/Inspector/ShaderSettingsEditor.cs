using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gomoru.su.LightController.API;
using UnityEditor;

namespace gomoru.su.LightController
{
    [CustomEditor(typeof(ShaderSettings))]
    internal sealed class ShaderSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            while(iterator.Next(true))
            {
                EditorGUILayout.LabelField(iterator.name);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
