#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices.Expando;
using System.ComponentModel;

namespace gomoru.su.LightController.API
{
    partial class ShaderSettings
    {
        public virtual void OnInspectorGUI(SerializedObject serializedObject) => _OnInspectorGUI[GetType()]?.Invoke(serializedObject);

        internal Parameter[] GetParameters() => _GetParameters[this.GetType()](this);

        internal static ImmutableDictionary<Type, Action<SerializedObject>> _OnInspectorGUI;

        internal static ImmutableDictionary<Type, Func<ShaderSettings, Parameter[]>> _GetParameters;

        protected internal static void DrawParameterGroup<T>(SerializedProperty group) where T : ParameterGroup
        {
            var position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            _guiContentSingleton.text = group.name;
            var label = EditorGUI.BeginProperty(position, _guiContentSingleton, group);
            
            position = DrawParameterSettings(position, group);
            var isOpen = group.isExpanded = EditorGUI.Foldout(position, group.isExpanded, label);

            EditorGUI.EndProperty();

            if (!isOpen)
                return;

            EditorGUI.BeginDisabledGroup(!group.FindPropertyRelative(nameof(ParameterGroup.IsEnable)).boolValue);

            ParameterGroupCore<T>.OnGUI(group);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(12);
        }

        protected internal static void DrawParameter<T>(SerializedProperty parameter, SerializedProperty group = null, object[] attributes = null) where T : Parameter
        {
            if (attributes is null)
                attributes = Array.Empty<object>();

            bool isBoolParameter = typeof(Parameter<bool>).IsAssignableFrom(typeof(T));
            var position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * (isBoolParameter ? 1 : 2));
            position = EditorGUI.IndentedRect(position);
            _guiContentSingleton.text = parameter.displayName;
            _guiContentSingleton.tooltip = (attributes.FirstOrDefault(x => x is TooltipAttribute) as TooltipAttribute)?.tooltip ?? string.Empty;
            var label = EditorGUI.BeginProperty(position, _guiContentSingleton, parameter);

            position.height = EditorGUIUtility.singleLineHeight;

            var labelRect = position;
            labelRect.width = EditorGUIUtility.labelWidth;
            if (isBoolParameter)
            {
                var isEnable = parameter.FindPropertyRelative(nameof(Parameter.IsEnable));
                var x = parameter.FindPropertyRelative(nameof(Parameter<bool>.Value));
                EditorGUI.BeginDisabledGroup(!isEnable.boolValue);
                x.boolValue = EditorGUI.ToggleLeft(labelRect, label, x.boolValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            DrawParameterSettings(position, parameter);

            if (isBoolParameter)
                return;

            position.y += position.height;

            EditorGUI.BeginDisabledGroup(!parameter.FindPropertyRelative(nameof(Parameter.IsEnable)).boolValue);

            EditorGUI.indentLevel++;
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
            EditorGUI.indentLevel--;

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }

        private static Rect DrawParameterSettings(Rect position, SerializedProperty property)
        {
            bool hiddenLabel = position.width < 400;
            float hiddenLabelWidth = GetToggleControlWidth("");
            float totalCheckBoxWidth = 0;

            var checkBoxRect = position;
            checkBoxRect.x += position.width;

            checkBoxRect.width = hiddenLabel ? hiddenLabelWidth : GetToggleControlWidth("Save");
            totalCheckBoxWidth += checkBoxRect.width;
            checkBoxRect.x -= checkBoxRect.width + 8 * (hiddenLabel ? 0 : 1);
            var check = property.FindPropertyRelative(nameof(ParameterGroup.IsSave));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, hiddenLabel ? "" : "Save", check.boolValue);

            checkBoxRect.width = hiddenLabel ? hiddenLabelWidth : GetToggleControlWidth("Sync");
            totalCheckBoxWidth += checkBoxRect.width;
            checkBoxRect.x -= checkBoxRect.width + 16 * (hiddenLabel ? 0 : 1);
            check = property.FindPropertyRelative(nameof(ParameterGroup.IsSync));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, hiddenLabel ? "" : "Sync", check.boolValue);

            checkBoxRect.width = hiddenLabel ? hiddenLabelWidth : GetToggleControlWidth("Enable");
            totalCheckBoxWidth += checkBoxRect.width;
            checkBoxRect.x -= checkBoxRect.width + 16 * (hiddenLabel ? 0 : 1);
            check = property.FindPropertyRelative(nameof(ParameterGroup.IsEnable));
            check.boolValue = EditorGUI.ToggleLeft(checkBoxRect, hiddenLabel ? "" : "Enable", check.boolValue);

            position.width -= totalCheckBoxWidth;
            return position;
        }

        private static readonly GUIContent _guiContentSingleton = new GUIContent();

        private static float GetToggleControlWidth(string text)
        {
            _guiContentSingleton.text = text;
            return EditorStyles.toggle.CalcSize(_guiContentSingleton).x + (EditorGUI.indentLevel * 15f);
        }

        private static class ParameterGroupCore<T> where T : ParameterGroup
        {
            public static Action<SerializedProperty> OnGUI;

            static ParameterGroupCore()
            {
                CreateOnGUI();
            }

            private static void CreateOnGUI()
            {
                var method = new DynamicMethod(nameof(OnGUI), null, new[] { typeof(SerializedProperty) });
                var il = method.GetILGenerator();
                il.DeclareLocal(typeof(SerializedProperty));

                var fields = typeof(T).GetFields();

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo parameter = fields[i];
                    if (parameter.GetCustomAttribute<HideInInspector>() != null)
                        continue;

                    // var property = ${arg0}.FindPropertyRelative(${parameter.Name});
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, parameter.Name);
                    il.Emit(OpCodes.Callvirt, ReflectionCache.FindPropertyRelative);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc_0);

                    var label = il.DefineLabel();
                    // if (property is null) continue;
                    il.Emit(OpCodes.Brfalse_S, label);

                    if (typeof(Parameter).IsAssignableFrom(parameter.FieldType))
                    {
                        // ${attributes} = typeof(T).GetField(property).GetCustomAttributes();
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldtoken, typeof(T));
                        il.Emit(OpCodes.Call, ReflectionCache.GetTypeFromHandle);
                        il.Emit(OpCodes.Ldstr, parameter.Name);
                        il.Emit(OpCodes.Call, ReflectionCache.GetMethod);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Callvirt, ReflectionCache.GetCustomAttributes);

                        // DrawParameter(property, ${attributes});
                        il.Emit(OpCodes.Call, ReflectionCache.DrawParameter.MakeGenericMethod(parameter.FieldType));
                    }
                    else
                    {
                        IncrementIndentLevel();
                        // EditorGUILayout.PropertyField(property);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Call, ReflectionCache.PropertyField);
                        il.Emit(OpCodes.Pop);
                        DecrementIndentLevel();
                    }

                    if (i != fields.Length - 1)
                    {
                        // EditorGUILayout.Space(12);
                        il.Emit(OpCodes.Ldc_R4, 12);
                        il.Emit(OpCodes.Call, ReflectionCache.Space);
                    }
                    il.MarkLabel(label);
                }
                il.Emit(OpCodes.Ret);

                OnGUI = method.CreateDelegate(typeof(Action<SerializedProperty>)) as Action<SerializedProperty>;

                void IncrementIndentLevel()
                {
                    il.Emit(OpCodes.Call, ReflectionCache.IndentLevel.GetMethod);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Call, ReflectionCache.IndentLevel.SetMethod);
                }
                void DecrementIndentLevel()
                {
                    il.Emit(OpCodes.Call, ReflectionCache.IndentLevel.GetMethod);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Call, ReflectionCache.IndentLevel.SetMethod);
                }
            }
        }

        internal static class ReflectionCache
        {
            public static MethodInfo GetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
            public static MethodInfo GetMethod = typeof(Type).GetMethod(nameof(Type.GetField), new[] { typeof(string) });
            public static MethodInfo GetCustomAttributes = typeof(MemberInfo).GetMethod(nameof(MemberInfo.GetCustomAttributes), new[] { typeof(bool) });
            public static MethodInfo DrawParameter = typeof(ShaderSettings).GetMethod(nameof(ShaderSettings.DrawParameter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            public static PropertyInfo BoolValue = typeof(SerializedProperty).GetProperty(nameof(SerializedProperty.boolValue));

            public static MethodInfo FindPropertyRelative = typeof(SerializedProperty).GetMethod(nameof(SerializedProperty.FindPropertyRelative));
            public static MethodInfo PropertyField = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.PropertyField), new[] { typeof(SerializedProperty), typeof(GUILayoutOption[]) });
            public static MethodInfo Space = typeof(GUILayout).GetMethod(nameof(GUILayout.Space));
            public static PropertyInfo IndentLevel = typeof(EditorGUI).GetProperty(nameof(EditorGUI.indentLevel));
        }
    }

    internal readonly struct ParameterInfo
    {
        public readonly Type Container;
        public readonly FieldInfo FieldInfo;


    }
}

#endif