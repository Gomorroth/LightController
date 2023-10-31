using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    //[CustomPropertyDrawer(typeof(GroupAttribute))]
    internal sealed class GroupMemberDrawer : PropertyDrawer
    {
        private static Dictionary<string, string> _displayNameCahce = new Dictionary<string, string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as GroupAttribute;
            var text = label.text;
            if (!_displayNameCahce.TryGetValue(attr.Group, out var displayName))
            {
                displayName = ObjectNames.NicifyVariableName(attr.Group);
                _displayNameCahce.Add(attr.Group, displayName);
            }
            if (text.StartsWith(displayName, StringComparison.OrdinalIgnoreCase))
            {
                label.text = label.text.Substring(displayName.Length + 1);
            }
            var range = fieldInfo.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                DrawSlider(position, property, label, range);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private void DrawSlider(Rect position, SerializedProperty property, GUIContent label, RangeAttribute range)
        {
            if (property.propertyType == SerializedPropertyType.Float)
                EditorGUI.Slider(position, property, range.min, range.max, label);
            else if (property.propertyType == SerializedPropertyType.Integer)
                EditorGUI.IntSlider(position, property, (int)range.min, (int)range.max, label);
        }
    }
}
