using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [CustomPropertyDrawer(typeof(BoolParameter))]
    [CustomPropertyDrawer(typeof(FloatParameter))]
    public sealed class ParameterDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (1 + (property.isExpanded ? 3 : 0));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propertyRect = position;
            propertyRect.height = EditorGUIUtility.singleLineHeight;
            label = EditorGUI.BeginProperty(position, label, property);

            var labelRect = propertyRect;
            labelRect.width = EditorGUIUtility.labelWidth;

            var fieldRect = propertyRect;
            fieldRect.width -= labelRect.width;
            fieldRect.x += labelRect.width;

            if (property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label))
            {
                var settingsRect = propertyRect;

                var props = new[] { property.FindPropertyRelative("Enable"), property.FindPropertyRelative("Save"), property.FindPropertyRelative("Sync") };
                foreach(var prop in props)
                {
                    settingsRect.y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.indentLevel++;
                    labelRect = settingsRect;
                    labelRect.width = EditorGUIUtility.labelWidth;

                    var fieldRect2 = settingsRect;
                    fieldRect2.width -= labelRect.width;
                    fieldRect2.x += labelRect.width;


                    EditorGUI.LabelField(labelRect, prop.displayName);
                    EditorGUI.indentLevel--;
                    EditorGUI.PropertyField(fieldRect2, prop, GUIContent.none);

                }

                //EditorGUI.indentLevel--;
            }

            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("Value"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
