#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using static gomoru.su.LightController.API.Editor.ReflectionCache;


namespace gomoru.su.LightController.API.Editor
{
    public abstract class ShaderSettingsEditor : UnityEditor.Editor
    {
        protected void DrawParameterGroup<T>(SerializedProperty group) where T : ParameterGroup
        {
            var isOpen = group.isExpanded = EditorGUILayout.Foldout(group.isExpanded, group.displayName);
            if (!isOpen)
                return;

            DrawParameterGroupCore<T>.Run(group);
        }


        protected static void DrawParameter<T>(SerializedProperty parameter, object[] attributes = null) where T : Parameter
        {
            if (attributes is null)
                attributes = Array.Empty<object>();

            if (typeof(Parameter<bool>).IsAssignableFrom(typeof(T)))
            {
                DrawBoolParameter(parameter, attributes);
                return;
            }

            var position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2);
            _guiContentSingleton.text = parameter.displayName;
            _guiContentSingleton.tooltip = (attributes.FirstOrDefault(x => x is TooltipAttribute) as TooltipAttribute)?.tooltip ?? string.Empty;
            var label = EditorGUI.BeginProperty(position, _guiContentSingleton, parameter);

            position.height = EditorGUIUtility.singleLineHeight;

            var labelRect = position;
            labelRect.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, label);

            var checkBoxRect = position;
            checkBoxRect.x += position.width;

            checkBoxRect.width = GetToggleControlWidth("Save");
            checkBoxRect.x -= checkBoxRect.width + 8;
            var check = parameter.FindPropertyRelative(nameof(Parameter.IsSave));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, "Save", check.boolValue);

            checkBoxRect.width = GetToggleControlWidth("Sync");
            checkBoxRect.x -= checkBoxRect.width + 16;
            check = parameter.FindPropertyRelative(nameof(Parameter.IsSync));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, "Sync", check.boolValue);

            checkBoxRect.width = GetToggleControlWidth("Enable");
            checkBoxRect.x -= checkBoxRect.width + 16;
            check = parameter.FindPropertyRelative(nameof(Parameter.IsEnable));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, "Enable", check.boolValue);

            position.y += position.height;

            EditorGUI.BeginDisabledGroup(!check.boolValue);

            var value = parameter.FindPropertyRelative("Value");
            if (attributes.FirstOrDefault(x => x is RangeAttribute) is RangeAttribute range)
            {
                if (typeof(Parameter<float>).IsAssignableFrom(typeof(T)))
                {
                    value.floatValue = EditorGUI.Slider(position, value.floatValue, range.min, range.max);
                }
                if (typeof(Parameter<int>).IsAssignableFrom(typeof(T)))
                {
                    value.intValue = EditorGUI.IntSlider(position, value.intValue, (int)range.min, (int)range.max);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, value, GUIContent.none);
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void DrawBoolParameter(SerializedProperty parameter, object[] attributes = null)
        {
            var position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            _guiContentSingleton.text = parameter.displayName;
            _guiContentSingleton.tooltip = (attributes.FirstOrDefault(x => x is TooltipAttribute) as TooltipAttribute)?.tooltip ?? string.Empty;
            var label = EditorGUI.BeginProperty(position, _guiContentSingleton, parameter);

            var isEnable = parameter.FindPropertyRelative(nameof(Parameter.IsEnable));

            var labelRect = position;
            labelRect.width = GetToggleControlWidth(label.text);
            var check = parameter.FindPropertyRelative(nameof(Parameter<bool>.Value));
            EditorGUI.BeginDisabledGroup(!isEnable.boolValue);
            check.boolValue = EditorGUI.ToggleLeft(labelRect, label, check.boolValue);
            EditorGUI.EndDisabledGroup();

            var checkBoxRect = position;
            checkBoxRect.x += position.width;

            checkBoxRect.width = GetToggleControlWidth("Save");
            checkBoxRect.x -= checkBoxRect.width + 8;
            check = parameter.FindPropertyRelative(nameof(Parameter.IsSave));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, "Save", check.boolValue);

            checkBoxRect.width = GetToggleControlWidth("Sync");
            checkBoxRect.x -= checkBoxRect.width + 16;
            check = parameter.FindPropertyRelative(nameof(Parameter.IsSync));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, "Sync", check.boolValue);

            checkBoxRect.width = GetToggleControlWidth("Enable");
            checkBoxRect.x -= checkBoxRect.width + 16;
            isEnable.boolValue = EditorGUI.ToggleLeft(checkBoxRect, "Enable", isEnable.boolValue);
        }

        private static readonly GUIContent _guiContentSingleton = new GUIContent();

        private static float GetToggleControlWidth(string text)
        {
            _guiContentSingleton.text = text;
            return EditorStyles.toggle.CalcSize(_guiContentSingleton).x;
        }

        private static class DrawParameterGroupCore<T> where T : ParameterGroup
        {
            public static readonly Action<SerializedProperty> Run;

            static DrawParameterGroupCore()
            {
                var method = new DynamicMethod("", null, new[] { typeof(SerializedProperty) });
                var il = method.GetILGenerator();
                il.DeclareLocal(typeof(SerializedProperty));
                il.DeclareLocal(typeof(SerializedProperty));

                var parameters = typeof(T).GetFields();

                for (int i = 0; i < parameters.Length; i++)
                {
                    FieldInfo parameter = parameters[i];

                    // ${property} = ${arg0}.FindPropertyRelative(${parameter.Name});
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, parameter.Name);
                    il.Emit(OpCodes.Callvirt, FindPropertyRelative);

                    if (typeof(Parameter).IsAssignableFrom(parameter.FieldType))
                    {
                        // ${attributes} = typeof(T).GetField(${parameter}).GetCustomAttributes();
                        il.Emit(OpCodes.Ldtoken, typeof(T));
                        il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
                        il.Emit(OpCodes.Ldstr, parameter.Name);
                        il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetField), new[] { typeof(string) }));
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetMethod(nameof(MemberInfo.GetCustomAttributes), new[] { typeof(bool) }));

                        // DrawParameter(${property}, ${attributes});
                        il.Emit(OpCodes.Call, typeof(ShaderSettingsEditor).GetMethod(nameof(DrawParameter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(parameter.FieldType));
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Call, typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.PropertyField), new[] { typeof(SerializedProperty), typeof(GUILayoutOption[]) }));
                        il.Emit(OpCodes.Pop);
                    }

                    if (i != parameters.Length - 1)
                    {
                        il.Emit(OpCodes.Ldc_R4, 12);
                        il.Emit(OpCodes.Call, ReflectionCache.Space);
                    }
                }
                il.Emit(OpCodes.Ret);

                Run = method.CreateDelegate(typeof(Action<SerializedProperty>)) as Action<SerializedProperty>;
            }
        }
    }

    internal static class ReflectionCache
    {
        public static MethodInfo GetEmptyGUILayoutOptionArray = typeof(Array).GetMethod("Empty").MakeGenericMethod(typeof(GUILayoutOption));

        public static MethodInfo FindPropertyRelative = typeof(SerializedProperty).GetMethod(nameof(SerializedProperty.FindPropertyRelative));
        public static PropertyInfo FloatValue = typeof(SerializedProperty).GetProperty(nameof(SerializedProperty.floatValue));
        public static PropertyInfo BoolValue = typeof(SerializedProperty).GetProperty(nameof(SerializedProperty.boolValue));
        public static PropertyInfo DisplayName = typeof(SerializedProperty).GetProperty(nameof(SerializedProperty.displayName));
        public static FieldInfo EmptyGUIContent = typeof(GUIContent).GetField(nameof(GUIContent.none), BindingFlags.Public | BindingFlags.Static);

        public static MethodInfo PropertyField = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.PropertyField), new[] { typeof(SerializedProperty), typeof(GUILayoutOption[]) });
        public static MethodInfo PropertyFieldWithLabel = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.PropertyField), new[] { typeof(SerializedProperty), typeof(GUIContent), typeof(GUILayoutOption[]) });
        public static MethodInfo LabelField = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.LabelField), new[] { typeof(string), typeof(GUILayoutOption[]) });
        public static MethodInfo Slider = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.Slider), new[] { typeof(float), typeof(float), typeof(float), typeof(GUILayoutOption[]) });
        public static MethodInfo ToggleLeft = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.ToggleLeft), new[] { typeof(string), typeof(bool), typeof(GUILayoutOption[]) });

        public static MethodInfo BeginHorizontal = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.BeginHorizontal), new[] { typeof(GUILayoutOption[]) });
        public static MethodInfo EndHorizontal = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.EndHorizontal));

        public static MethodInfo FlexibleSpace = typeof(GUILayout).GetMethod(nameof(GUILayout.FlexibleSpace));
        public static MethodInfo Space = typeof(GUILayout).GetMethod(nameof(GUILayout.Space));

        public static FieldInfo ToggleWidth = typeof(ReflectionCache).GetField(nameof(_ToggleWidth), BindingFlags.Public | BindingFlags.Static);
        public static readonly GUILayoutOption[] _ToggleWidth = new[] { GUILayout.Width(64) };
    }

}

#endif