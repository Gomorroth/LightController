using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    partial class RuntimeHelper
    {
        private static class ShaderSettings
        {
            private static readonly FieldInfo NameField = typeof(Parameter).GetField(nameof(Parameter.Name));

            public static void Initialize()
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes().Where(y => typeof(API.ShaderSettings).IsAssignableFrom(y)));

                CreateGetParameters(types);
                CreateOnInspectorGUI(types);
            }

            private static void CreateOnInspectorGUI(IEnumerable<Type> types)
            {
                var Update = typeof(SerializedObject).GetMethod(nameof(SerializedObject.Update));
                var ApplyModifiedProperties = typeof(SerializedObject).GetMethod(nameof(SerializedObject.ApplyModifiedProperties));

                var FindProperty = typeof(SerializedObject).GetMethod(nameof(SerializedObject.FindProperty));
                var PropertyField = API.ShaderSettings.ReflectionCache.PropertyField;

                var DrawParameterGroup = typeof(API.ShaderSettings).GetMethod(nameof(API.ShaderSettings.DrawParameterGroup), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var DrawParameter = API.ShaderSettings.ReflectionCache.DrawParameter;

                API.ShaderSettings._OnInspectorGUI = types.Select(type =>
                {
                    var method = new DynamicMethod("", null, new[] { typeof(SerializedObject) });
                    var il = method.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, Update);

                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x =>
                        x.GetCustomAttribute<NonSerializedAttribute>() == null || 
                        x.GetCustomAttribute<HideInInspector>() == null)
                    )
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, field.Name);
                        il.Emit(OpCodes.Callvirt, FindProperty);
                        if (typeof(Parameter).IsAssignableFrom(field.FieldType))
                        {
                            il.Emit(OpCodes.Call, DrawParameter.MakeGenericMethod(field.FieldType));
                        }
                        if (typeof(ParameterGroup).IsAssignableFrom(field.FieldType))
                        {
                            il.Emit(OpCodes.Call, DrawParameterGroup.MakeGenericMethod(field.FieldType));
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldnull);
                            il.Emit(OpCodes.Call, PropertyField);
                            il.Emit(OpCodes.Pop);
                        }
                    }

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, ApplyModifiedProperties);
                    il.Emit(OpCodes.Pop);

                    il.Emit(OpCodes.Ret);


                    return (Type: type, Method: method.CreateDelegate(typeof(Action<SerializedObject>)) as Action<SerializedObject>);
                }).ToImmutableDictionary(x => x.Type, x => x.Method);
            }

            private static void CreateGetParameters(IEnumerable<Type> types)
            {
                API.ShaderSettings._GetParameters = types.Select(type =>
                {
                    var list = new List<(FieldInfo Parent, FieldInfo Field)>();

                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (typeof(Parameter).IsAssignableFrom(field.FieldType))
                        {
                            list.Add((null, field));
                        }
                        if (typeof(ParameterGroup).IsAssignableFrom(field.FieldType))
                        {
                            foreach (var field2 in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                            {
                                if (typeof(Parameter).IsAssignableFrom(field2.FieldType))
                                {
                                    list.Add((field, field2));
                                }
                            }
                        }
                    }

                    var parameters = list.AsSpan();
                    var method = new DynamicMethod("", typeof(Parameter[]), new[] { typeof(API.ShaderSettings) });
                    var il = method.GetILGenerator();

                    var prefix = typeof(ParameterGroup).GetProperty(nameof(ParameterGroup.UseGroupNameAsPrefix));
                    var name = typeof(ParameterGroup).GetProperty(nameof(ParameterGroup.Name));

                    il.DeclareLocal(typeof(string));

                    il.LdcI4(list.Count);
                    il.Emit(OpCodes.Newarr, typeof(Parameter));
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];

                        il.Emit(OpCodes.Dup);

                        il.LdcI4(i);
                        il.Emit(OpCodes.Ldarg_0);
                        if (parameter.Parent != null)
                        {
                            il.Emit(OpCodes.Ldfld, parameter.Parent);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Callvirt, prefix.GetMethod);
                            var end = il.DefineLabel();
                            var then = il.DefineLabel();
                            il.Emit(OpCodes.Brtrue_S, then);
                            il.Emit(OpCodes.Pop);
                            il.Emit(OpCodes.Ldnull);
                            il.Emit(OpCodes.Br_S, end);

                            il.MarkLabel(then);
                            il.Emit(OpCodes.Callvirt, name.GetMethod);

                            il.MarkLabel(end);
                            il.Emit(OpCodes.Stloc_0);
                        }
                        il.Emit(OpCodes.Ldfld, parameter.Field);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldfld, NameField);

                        var label = il.DefineLabel();
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Brtrue_S, label);
                        il.Emit(OpCodes.Dup);


                        if (parameter.Field.GetCustomAttribute<ParameterNameAttribute>() is ParameterNameAttribute attr)
                        {
                            il.Emit(OpCodes.Ldstr, attr.Name);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldstr, "_");
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldstr, parameter.Field.Name);
                            il.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string), typeof(string) }));
                        }

                        il.Emit(OpCodes.Stfld, NameField);

                        il.MarkLabel(label);
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                    il.Emit(OpCodes.Ret);

                    return (Type: type, Method: method.CreateDelegate(typeof(Func<API.ShaderSettings, Parameter[]>)) as Func<API.ShaderSettings, Parameter[]>);
                }).ToImmutableDictionary(x => x.Type, x => x.Method);
            }
        }
    }
}
