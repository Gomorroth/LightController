﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace gomoru.su.LightController
{
    internal static class Utils
    {
        private static MethodInfo _GetGeneratedAssetsFolder = typeof(nadena.dev.modular_avatar.core.editor.AvatarProcessor).Assembly.GetTypes().FirstOrDefault(x => x.Name == "Util")?.GetMethod(nameof(GetGeneratedAssetsFolder), BindingFlags.Static | BindingFlags.NonPublic);

        public static string GetGeneratedAssetsFolder()
        {
            var method = _GetGeneratedAssetsFolder;
            if (method != null)
                return method.Invoke(null, null) as string;

            var path = "Assets/_LightControllerTemporaryAssets";
            if (!AssetDatabase.IsValidFolder(path))
                path = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder("Assets/", "_LightControllerTemporaryAssets"));

            return path;
        }

        public static AnimatorController CreateTemporaryAsset()
        {
            var fx = new AnimatorController() { name = GUID.Generate().ToString() };
            AssetDatabase.CreateAsset(fx, System.IO.Path.Combine(Utils.GetGeneratedAssetsFolder(), $"{fx.name}.controller"));
            AssetDatabase.SaveAssets();
            return fx;
        }

        public static T GetOrAddComponent<T>(this GameObject obj, Action<T> action = null) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            action?.Invoke(component);
            return component;
        }

        public static T AddTo<T>(this T obj, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        public static T HideInHierarchy<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static string GetRelativePath(this Transform transform, Transform root, bool includeRelativeTo = false)
        {
            var buffer = _relativePathBuffer;
            if (buffer is null)
            {
                buffer = _relativePathBuffer = new string[128];
            }

            var t = transform;
            int idx = buffer.Length;
            while (t != null && t != root)
            {
                buffer[--idx] = t.name;
                t = t.parent;
            }
            if (includeRelativeTo && t != null && t == root)
            {
                buffer[--idx] = t.name;
            }

            return string.Join("/", buffer, idx, buffer.Length - idx);
        }

        private static string[] _relativePathBuffer;

        public static IEnumerable<string> EnumeratePropertyNames(this Shader shader) => Enumerable.Range(0, shader.GetPropertyCount()).Select(shader.GetPropertyName);

        private static AnimatorCondition[] _conditions1 = new AnimatorCondition[1];
        private static AnimatorCondition[] _conditions2 = new AnimatorCondition[2];

        public static AnimatorStateTransition AddTransition(this AnimatorState state, AnimatorState destination, AnimatorCondition condition)
        {
            _conditions1[0] = condition;
            return state.AddTransition(destination, _conditions1);
        }

        public static AnimatorStateTransition AddTransition(this AnimatorState state, AnimatorState destination, AnimatorCondition condition1, AnimatorCondition condition2)
        {
            _conditions2[0] = condition1;
            _conditions2[1] = condition2;
            return state.AddTransition(destination, _conditions2);
        }

        public static AnimatorStateTransition AddTransition(this AnimatorState state, AnimatorState destination, AnimatorCondition[] conditions)
        {
            var transition = new AnimatorStateTransition()
            {
                destinationState = destination,
                hasExitTime = false,
                duration = 0,
                hasFixedDuration = true,
                canTransitionToSelf = false,
                conditions = conditions,
            }.HideInHierarchy().AddTo(state);
            state.AddTransition(transition);
            return transition;
        }

        public static AnimatorController AddParameter<T>(this AnimatorController controller, string name, T defaultValue)
        {
            var param = new AnimatorControllerParameter()
            {
                name = name,

            };
            if (typeof(T) == typeof(float))
            {
                param.type = AnimatorControllerParameterType.Float;
                param.defaultFloat = (float)(object)defaultValue;
            }
            else if (typeof(T) == typeof(int))
            {
                param.type = AnimatorControllerParameterType.Int;
                param.defaultInt = (int)(object)defaultValue;
            }
            else if (typeof(T) == typeof(bool))
            {
                param.type = AnimatorControllerParameterType.Bool;
                param.defaultBool = (bool)(object)defaultValue;
            }
            else throw new ArgumentException(nameof(defaultValue));
            controller.AddParameter(param);
            return controller;
        }

        public static List<VRCExpressionsMenu.Control> CreateRadialPuppet(this List<VRCExpressionsMenu.Control> controls, string name, string parameterName = null)
        {
            var control = new VRCExpressionsMenu.Control()
            {
                name = name,
                type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                subParameters = new VRCExpressionsMenu.Control.Parameter[] { new VRCExpressionsMenu.Control.Parameter() { name = parameterName ?? name } }
            };
            controls.Add(control);
            return controls;
        }

        public static List<VRCExpressionsMenu.Control> CreateToggle(this List<VRCExpressionsMenu.Control> controls, string name, string parameterName = null)
        {
            var control = new VRCExpressionsMenu.Control()
            {
                name = name,
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter() { name = parameterName ?? name },
            };
            controls.Add(control);
            return controls;
        }

        public static AnimationClip CreateAnim(this AnimatorController parent, string name = null) => new AnimationClip() { name = name }.AddTo(parent);
        public static AnimatorState CreateState(this AnimatorStateMachine parent, string name, AnimationClip motion = null) => new AnimatorState() { name = name, writeDefaultValues = false, motion = motion }.HideInHierarchy().AddTo(parent);
        
        public static List<ParameterControl.Parameter> AddParameter<T>(this List<ParameterControl.Parameter> list, string name, T value, string group, bool boolAsFloat = false) => AddParameter(list, name, value, typeof(T), group, boolAsFloat);

        public static List<ParameterControl.Parameter> AddParameter(this List<ParameterControl.Parameter> list, string name, object value, Type type, string group, bool boolAsFloat = false)
        {
            list.Add(new ParameterControl.Parameter()
            {
                Name = name,
                Group = group,
                Value =
                    type == typeof(int) ? (int)value :
                    type == typeof(float) ? (float)value :
                    type == typeof(bool) ? (bool)value ? 1f : 0f :
                    0,
                ParameterType =
                    type == typeof(int) ? AnimatorControllerParameterType.Int :
                    type == typeof(float) ? AnimatorControllerParameterType.Float :
                    type == typeof(bool) ? AnimatorControllerParameterType.Bool :
                    0,
                BoolAsFloat = boolAsFloat,
            });
            return list;
        }

        public static T GetAttribute<T>(this IEnumerable<Attribute> attributes) where T : Attribute => attributes.FirstOrDefault(x => x is T) as T;

        private delegate void InternalSetParametersFromMaterialDelegate(LilToonParameters parameters, Material material);
        private static InternalSetParametersFromMaterialDelegate _internalSetParametersFromMaterial;

        public static void SetParametersFromMaterial(this LilToonParameters parameters, Material material)
        {
            if (_internalSetParametersFromMaterial == null)
            {
                try
                {
                    var method = new DynamicMethod("", null, new Type[] { typeof(LilToonParameters), typeof(Material) });

                    var il = method.GetILGenerator();

                    var fields = typeof(LilToonParameters).GetFields().Where(x => !x.IsLiteral && x.GetCustomAttribute<InternalPropertyAttribute>() == null).Select(x => (Field: x, Proxy: x.GetCustomAttribute<VectorProxyAttribute>()));

                    var methodArgs = new Type[] { typeof(string) };
                    var getFloat = typeof(Material).GetMethod(nameof(Material.GetFloat), methodArgs);
                    var getInt = typeof(Material).GetMethod(nameof(Material.GetInt), methodArgs);
                    var getVector = typeof(Material).GetMethod(nameof(Material.GetVector), methodArgs);
                    var getColor = typeof(Material).GetMethod(nameof(Material.GetColor), methodArgs);

                    foreach (var (field, _) in fields.Where(x => x.Proxy == null))
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldstr, $"_{field.Name}");
                        if (field.FieldType == typeof(float))
                        {
                            il.Emit(OpCodes.Callvirt, getFloat);
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            il.Emit(OpCodes.Callvirt, getInt);
                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Cgt_Un);
                        }
                        else if (field.FieldType == typeof(Color))
                        {
                            il.Emit(OpCodes.Callvirt, getColor);
                        }
                        else if (field.FieldType == typeof(Vector4))
                        {
                            il.Emit(OpCodes.Callvirt, getVector);
                        }
                        il.Emit(OpCodes.Stfld, field);
                    }

                    foreach (var (field, _) in fields.Where(x => x.Proxy != null))
                    {
                        var attr = field.GetCustomAttribute<VectorProxyAttribute>();
                        var target = fields.FirstOrDefault(x => x.Field.Name == attr.TargetName);
                        if (target.Field != null)
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldflda, target.Field);
                            il.Emit(OpCodes.Conv_U);
                            switch (attr.Index)
                            {
                                case 1:
                                    il.Emit(OpCodes.Ldc_I4_4);
                                    goto add;
                                case 2:
                                    il.Emit(OpCodes.Ldc_I4_8);
                                    goto add;
                                case 3:
                                    il.Emit(OpCodes.Ldc_I4_S, 12);
                                    goto add;
                                add:
                                    il.Emit(OpCodes.Add);
                                    break;
                            }
                            il.Emit(OpCodes.Ldind_R4);
                            if (field.FieldType == typeof(bool))
                            {
                                il.Emit(OpCodes.Ldc_R4, 0f);
                                il.Emit(OpCodes.Ceq);
                                il.Emit(OpCodes.Ldc_I4_0);
                                il.Emit(OpCodes.Ceq);

                            }
                            il.Emit(OpCodes.Stfld, field);
                        }
                    }

                    il.Emit(OpCodes.Ret);
                    _internalSetParametersFromMaterial = method.CreateDelegate(typeof(InternalSetParametersFromMaterialDelegate)) as InternalSetParametersFromMaterialDelegate;
                }
                catch (Exception e) { Debug.LogError(e); }
            }

            _internalSetParametersFromMaterial?.Invoke(parameters, material);
        }
    }
}
