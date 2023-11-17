using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.util;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using YamlDotNet.Core.Tokens;
using static VRC.SDKBase.VRCPlayerApi;
using Object = UnityEngine.Object;

namespace gomoru.su.LightController
{
    internal sealed partial class LightControllerCore
    {
        [MenuItem("Test/LightController")]
        public static void Test()
        {
            var container = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            AssetDatabase.CreateAsset(container, "Assets/TestContainer.asset");
            Run(Object.FindObjectOfType<LightController>(), container);
        }

        public static void Run(LightController lightController, Object assetContainer)
        {
            var avatar = lightController.GetComponentInParent<VRCAvatarDescriptor>();
            var shaderSettings = lightController.GetComponentsInChildren<ShaderSettings>();
            if (avatar == null || shaderSettings == null)
                return;

            var excludes = lightController.Excludes.SelectMany(x => x.EnumerateUnderlyingObjects()).ToImmutableHashSet();
            var targetRenderers = avatar.GetComponentsInChildren<Renderer>(true).Where(x => !excludes.Contains(x.gameObject)).ToArray();
            if (targetRenderers.Length == 0) 
                return;

            var animations = new List<(GameObject Root, AnimationClip Motion)>();
            new ObjectReferenceMapper()
                .Register<RuntimeAnimatorController>((component, value) =>
                {
                    var root = component.gameObject;
                    if (component is ModularAvatarMergeAnimator mama && mama.pathMode == MergeAnimatorPathMode.Absolute)
                    {
                        root = avatar.gameObject;
                    }
                    foreach(var anim in value.animationClips)
                    {
                        animations.Add((root, anim));
                    }
                    return value;
                })
                .Map(avatar.gameObject);

            lightController.gameObject.GetOrAddComponent<ModularAvatarMenuItem>(x =>
            {
                x.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                x.MenuSource = SubmenuSource.Children;
            });

            using (var container = new ObjectContainer(assetContainer))
            {
                var directBlendTree = new DirectBlendTree(container.Container);
                var avatarParameters = new List<AvatarParameterInfo>();

                foreach (var shaderSetting in shaderSettings)
                {
                    var root = directBlendTree.AddDirectBlendTree();
                    root.Name = shaderSetting.DisplayName;
                    void Process<T>(T obj, DirectBlendTree treeParent, GameObject menuParent)
                    {
                        foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (typeof(Parameter).IsAssignableFrom(field.FieldType))
                            {
                                var parameter = field.GetValue(obj) as Parameter;

                                if (parameter == null || !parameter.IsEnable)
                                    continue;

                                var name = $"{(obj is ParameterGroup group && group.UseGroupNameAsPrefix ? group.Name : "")}{field.Name}";

                                bool isColorParameter = typeof(Parameter<Color>).IsAssignableFrom(field.FieldType);
                                bool isVectorParameter = !isColorParameter && typeof(Parameter<Vector4>).IsAssignableFrom(field.FieldType);

                                if (isColorParameter | isVectorParameter)
                                {
                                    var treeGroup = treeParent.AddDirectBlendTree();
                                    var menuGroup = new GameObject();
                                    menuGroup.name = treeGroup.Name = field.Name;
                                    menuGroup.transform.parent = menuParent.transform;
                                    var menuItem = menuGroup.AddComponent<ModularAvatarMenuItem>();
                                    menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                                    menuItem.MenuSource = SubmenuSource.Children;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        var f = (isColorParameter ? "rgba" : "xyzw")[i];
                                        var fName = $"{name}.{f}";
                                        var shaderParameterName = $"material._{fName}";

                                        shaderSetting.OnParameterPostProcess(fName, parameter, ref parameter.MinValue, ref parameter.MaxValue);

                                        var min = new AnimationClip() { name = $"{fName}.Min", }.AddTo(container);
                                        var max = new AnimationClip() { name = $"{fName}.Max", }.AddTo(container);

                                        foreach (var renderer in targetRenderers)
                                        {
                                            var path = renderer.gameObject.AvatarRootPath();
                                            var type = renderer.GetType();

                                            min.SetCurve(path, type, shaderParameterName, AnimationCurve.Constant(0, 0, parameter.MinValue));
                                            max.SetCurve(path, type, shaderParameterName, AnimationCurve.Constant(0, 0, parameter.MaxValue));
                                        }
                                        string displayName;
                                        displayName = (isColorParameter ? "RGBA" : "XYZW")[i].ToString();
                                        var menu = new GameObject();
                                        menu.name = displayName;
                                        menu.transform.parent = menuGroup.transform;
                                        menuItem = menu.AddComponent<ModularAvatarMenuItem>();
                                        menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.RadialPuppet;
                                        menuItem.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter() { name = fName } };
                                        if (isColorParameter)
                                        {
                                            var value = (parameter as Parameter<Color>).Value;
                                            avatarParameters.Add(CreateParameter(fName, parameter.IsSync, parameter.IsSave, Unsafe.Add(ref Unsafe.As<Color, float>(ref value), i)));
                                        }
                                        else
                                        {
                                            var value = (parameter as Parameter<Vector4>).Value;
                                            avatarParameters.Add(CreateParameter(fName, parameter.IsSync, parameter.IsSave, Unsafe.Add(ref Unsafe.As<Vector4, float>(ref value), i)));
                                        }
                                    }
                                }
                                else
                                {

                                    string shaderParameterName;
                                    if (field.GetCustomAttribute<VectorProxyAttribute>() is VectorProxyAttribute vectorProxy)
                                    {
                                        var f = 'w' + ((int)vectorProxy.Field / 2 + 1) % 5;
                                        shaderParameterName = $"material._{vectorProxy.TargetName}.{f}";
                                    }
                                    else
                                    {
                                        shaderParameterName = $"material._{name}";
                                    }

                                    var min = new AnimationClip() { name = $"{name}.Min", }.AddTo(container);
                                    var max = new AnimationClip() { name = $"{name}.Max", }.AddTo(container);

                                    if (field.GetCustomAttribute<RangeAttribute>() is RangeAttribute range)
                                    {
                                        (parameter.MinValue, parameter.MaxValue) = (range.min, range.max);
                                    }

                                    shaderSetting.OnParameterPostProcess(name, parameter, ref parameter.MinValue, ref parameter.MaxValue);

                                    foreach (var renderer in targetRenderers)
                                    {
                                        var path = renderer.gameObject.AvatarRootPath();
                                        var type = renderer.GetType();

                                        min.SetCurve(path, type, shaderParameterName, AnimationCurve.Constant(0, 0, parameter.MinValue));
                                        max.SetCurve(path, type, shaderParameterName, AnimationCurve.Constant(0, 0, parameter.MaxValue));
                                    }

                                    var menu = new GameObject();
                                    menu.transform.parent = menuParent.transform;
                                    var menuItem = menu.AddComponent<ModularAvatarMenuItem>();
                                    menu.name = field.Name;
                                    if (parameter is Parameter<float> floatParam)
                                    {
                                        avatarParameters.Add(CreateParameter(name, parameter.IsSync, parameter.IsSave, floatParam.Value));
                                        menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.RadialPuppet;
                                        menuItem.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter() { name = name } };
                                    }
                                    else if (parameter is Parameter<bool> boolParam)
                                    {
                                        avatarParameters.Add(CreateParameter(name, parameter.IsSync, parameter.IsSave, boolParam.Value));
                                        menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                                        menuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter() { name = name };
                                    }

                                    var tree = treeParent.AddToggle(name);
                                    tree.Name = field.Name;
                                    tree.OFF = min;
                                    tree.ON = max;
                                }
                            }
                            else if (typeof(ParameterGroup).IsAssignableFrom(field.FieldType))
                            {
                                var parameterGroup = field.GetValue(obj) as ParameterGroup;
                                if (parameterGroup == null || !parameterGroup.IsEnable)
                                    continue;

                                var menu = new GameObject();
                                menu.transform.parent = menuParent.transform;
                                var menuItem = menu.AddComponent<ModularAvatarMenuItem>();
                                menu.name = parameterGroup.Name;
                                menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                                menuItem.MenuSource = SubmenuSource.Children;

                                var tree = treeParent.AddDirectBlendTree();
                                tree.Name = parameterGroup.Name;
                                Process(parameterGroup, tree, menu);
                            }
                        }
                    }
                    Process(shaderSetting, root, lightController.gameObject);
                }

                var animator = new AnimatorController() { name = "Light Controller" }.AddTo(container);
                var layer = directBlendTree.ToAnimatorControllerLayer();
                layer.name = "Light Controller";
                animator.AddLayer(layer);

                animator.AddParameter(new AnimatorControllerParameter() { name = "1", defaultFloat = 1, type = AnimatorControllerParameterType.Float });

                foreach(var parameter in avatarParameters)
                {
                    animator.AddParameter(parameter.ToAnimatorParameter());
                }

                lightController.gameObject.GetOrAddComponent<ModularAvatarMergeAnimator>(x =>
                {
                    x.animator = animator;
                    x.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                    x.matchAvatarWriteDefaults = false;
                    x.pathMode = MergeAnimatorPathMode.Absolute;
                });

                lightController.gameObject.GetOrAddComponent<ModularAvatarParameters>(x =>
                {
                    x.parameters.AddRange(avatarParameters.Select(parameter =>
                    {
                        var p = parameter.ToParameterConfig();
                        p.internalParameter = true;
                        return p;
                    }));
                });
            }
        }

        private static AvatarParameterInfo CreateParameter(string name, bool isSync, bool isSave, float value)
        {
            return new AvatarParameterInfo(name, AnimatorControllerParameterType.Float, ParameterSyncType.Float, isSync, isSave, value);
        }
        private static AvatarParameterInfo CreateParameter(string name, bool isSync, bool isSave, bool value)
        {
            return new AvatarParameterInfo(name, AnimatorControllerParameterType.Float, ParameterSyncType.Bool, isSync, isSave, value);
        }

        private readonly struct AvatarParameterInfo
        {
            public readonly string Name;
            public readonly AnimatorControllerParameterType AnimatorType;
            public readonly ParameterSyncType ExpressionType;

            public readonly bool IsSync;
            public readonly bool IsSave;
            public readonly float Value;

            public AvatarParameterInfo(string name, AnimatorControllerParameterType animatorType, ParameterSyncType expressionType, bool isSync, bool isSave, bool value) : this(name, animatorType, expressionType, isSync, isSave, value ? 1f : 0f) { }

            public AvatarParameterInfo(string name, AnimatorControllerParameterType animatorType, ParameterSyncType expressionType, bool isSync, bool isSave, int value) : this(name, animatorType, expressionType, isSync, isSave, (float)value) { }

            public AvatarParameterInfo(string name, AnimatorControllerParameterType animatorType, ParameterSyncType expressionType, bool isSync, bool isSave, float value)
            {
                Name = name;
                AnimatorType = animatorType;
                ExpressionType = expressionType;
                IsSync = isSync;
                IsSave = isSave;
                Value = value;
            }

            public AnimatorControllerParameter ToAnimatorParameter()
            {
                return new AnimatorControllerParameter()
                {
                    name = Name,
                    type = AnimatorType,
                    defaultBool = ToBool(),
                    defaultFloat = ToFloat(),
                    defaultInt = ToInt(),
                };
            }

            public ParameterConfig ToParameterConfig()
            {
                return new ParameterConfig()
                {
                    nameOrPrefix = Name,
                    syncType = ExpressionType,
                    localOnly = !IsSync,
                    saved = IsSave,
                    defaultValue = Value,
                };
            }

            private bool ToBool() => Value != 0;
            private float ToFloat() => Value;
            private int ToInt() => (int)Value;
        }
    }
}