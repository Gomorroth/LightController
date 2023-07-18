using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace gomoru.su.LightController
{
    public static class Generator
    {
        private const string PropertyNamePrefix = "material._";
        private const string ParameterNamePrefix = "LightController";

        private static readonly ParameterControl[] Controls = new ParameterControl[]
        {
            CreateControl(param => param.LightMinLimit),
            CreateControl(param => param.LightMaxLimit,
                args => args.List.AddParameter(args.Name, (1 + args.Parameters.LightMaxLimit) / args.Generator.LightMaxLimitMax),
                args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.LightMaxLimit)}", AnimationUtils.Constant(args.Parameters.LightMaxLimit));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.LightMaxLimit)}", AnimationUtils.Linear(0, args.Generator.LightMaxLimitMax));
                }),
            CreateControl(param => param.MonochromeLighting),
            CreateControl(param => param.ShadowEnvStrength),
            CreateControl(param => param.AsUnlit),
            CreateControl(param => param.VertexLightStrength),

            CreateControl(param => param.UseBacklight),
            CreateControl(param => param.BacklightColor,
                args => args.List.AddParameter(args.Name, args.Parameters.BacklightColor.a), 
                args =>
                {
                    var color = args.Parameters.BacklightColor;
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.r", AnimationUtils.Constant(color.r));
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.g", AnimationUtils.Constant(color.g));
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.b", AnimationUtils.Constant(color.b));
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.a", AnimationUtils.Constant(color.a));

                    var zero = default(Color);
                    Color.RGBToHSV(color, out zero.r , out zero.g, out zero.b);
                    zero.b = 0;
                    zero = Color.HSVToRGB(zero.r, zero.g, zero.b);

                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.r", AnimationUtils.Linear(zero.r, color.r));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.g", AnimationUtils.Linear(zero.g, color.g));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.b", AnimationUtils.Linear(zero.b, color.b));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.BacklightColor)}.a", AnimationUtils.Linear(zero.a, color.a));
                }),

            CreateControl(param => param.BacklightMainStrength),
            CreateControl(param => param.BacklightReceiveShadow),
            CreateControl(param => param.BacklightBackfaceMask),
            CreateControl(param => param.BacklightNormalStrength),
            CreateControl(param => param.BacklightBorder),
            CreateControl(param => param.BacklightBlur),
            CreateControl(param => param.BacklightDirectivity),
            CreateControl(param => param.BacklightViewStrength),
        };

        public static void Generate(GameObject avatarObject, LightControllerGenerator generator)
        {
            var fx = generator.FX;
            var go = generator.gameObject;
            if (fx == null)
                fx = Utils.CreateTemporaryAsset();

            var targets = avatarObject.GetComponentsInChildren<Renderer>(true)
                .Where(x => (x is MeshRenderer || x is SkinnedMeshRenderer) && x.tag != "EditorOnly")
                .Select(x =>
                    (Renderer: x,
                     Material: x.sharedMaterials
                        .Where(y => y != null)
                        .FirstOrDefault(y =>
                            y.shader.name.IndexOf("lilToon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            y.shader.EnumeratePropertyNames().Any(z => z == $"_{nameof(LilToonParameters.LightMinLimit)}"))
                        ))
                .Where(x => x.Material != null);

            List<ParameterControl.Parameter> parameters = new List<ParameterControl.Parameter>();
            parameters.AddParameter("Enabled", false);

            var controls = Controls.Where(x => x.Condition(generator)).ToArray();

            foreach (var control in controls)
            {
                control.Default = fx.CreateAnim($"{control.Name} Default");
                control.Control = fx.CreateAnim(control.Name);
            }

            var @params = generator.DefaultParameters;


            foreach (var (renderer, material) in targets)
            {
                var path = renderer.transform.GetRelativePath(avatarObject.transform);
                var type = renderer.GetType();
                if (generator.UseMaterialPropertyAsDefault)
                {
                    @params.SetValuesFromMaterial(material);
                }

                foreach(var control in controls)
                {
                    control.SetAnimationCurves((path, type, control.Default, control.Control, material, generator, @params));
                }
            }

            fx.AddParameter("Enabled", AnimatorControllerParameterType.Bool);

            foreach (var control in controls)
            {
                var layer = new AnimatorControllerLayer()
                {
                    name = control.Name,
                    defaultWeight = 1,
                    stateMachine = new AnimatorStateMachine() { name = control.Name }.HideInHierarchy().AddTo(fx),
                };

                var stateMachine = layer.stateMachine;

                var idle = stateMachine.CreateState("Idle", control.Default);
                var state = stateMachine.CreateState(control.Name, control.Control);
                state.timeParameter = control.Name;
                state.timeParameterActive = true;

                idle.AddTransition(state, new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = "Enabled" });
                state.AddTransition(idle, new AnimatorCondition() { mode = AnimatorConditionMode.IfNot, parameter = "Enabled" });

                stateMachine.AddState(idle, stateMachine.entryPosition + new Vector3(150, 0));
                stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(150, 50));

                fx.AddLayer(layer);

                control.Parameters((control.Name, generator, @params, parameters));
            }

            foreach(var parameter in parameters)
            {
                fx.AddParameter(parameter.ToControllerParameter());
            }

            go.GetOrAddComponent<ModularAvatarMergeAnimator>(x =>
            {
                x.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;
                x.animator = fx;
                x.matchAvatarWriteDefaults = true;
                x.pathMode = MergeAnimatorPathMode.Absolute;
            });

            go.GetOrAddComponent<ModularAvatarMenuInstaller>(x =>
            {
             
                var mainMenu = CreateExpressionMenu("Main Menu").AddTo(fx);
                mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Enable",
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter() { name = "Enabled" },
                });

                Dictionary<string, VRCExpressionsMenu> category = new Dictionary<string, VRCExpressionsMenu>();

                int groupCount = controls.Select(y => y.Group).Where(y => y != null).Distinct().Count();

                foreach (var control in controls)
                {
                    var menu = mainMenu;
                    if (groupCount >= 2 && !string.IsNullOrEmpty(control.Group))
                    {
                        if (!category.TryGetValue(control.Group, out menu))
                        {
                            menu = CreateExpressionMenu($"{control.Group} Menu").AddTo(fx);
                            mainMenu.controls.Add(new VRCExpressionsMenu.Control() { name = control.Group, type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = menu });
                            category.Add(control.Group, menu);
                        }
                    }

                    control.CreateMenu((control.Name, control, menu.controls));
                }

                x.menuToAppend = CreateExpressionMenu("Menu", new VRCExpressionsMenu.Control()
                {
                    name = "Light Controller",
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = mainMenu,
                }).AddTo(fx);
            });

            go.GetOrAddComponent<ModularAvatarParameters>(component =>
            {
                component.parameters.AddRange(parameters.Select(x =>
                {
                    var p = x.ToParameterConfig();
                    p.saved = generator.SaveParameters;
                    p.remapTo = $"{ParameterNamePrefix}{p.nameOrPrefix}";
                    return p;
                }));
            });
        }

        private static List<VRCExpressionsMenu.Control> CreateRadialPuppet(this List<VRCExpressionsMenu.Control> controls, string name, string parameterName = null)
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

        private static List<VRCExpressionsMenu.Control> CreateToggle(this List<VRCExpressionsMenu.Control> controls, string name, string parameterName = null)
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

        private static AnimationClip CreateAnim(this AnimatorController parent, string name = null) => new AnimationClip() { name = name }.AddTo(parent);
        private static AnimatorState CreateState(this AnimatorStateMachine parent, string name, AnimationClip motion = null) => new AnimatorState() { name = name, writeDefaultValues = false, motion = motion }.HideInHierarchy().AddTo(parent);
        private static VRCExpressionsMenu CreateExpressionMenu(string name, VRCExpressionsMenu.Control control = null)
        {
            var result = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            result.name = name;
            if (control != null)
            result.controls.Add(control);
            return result;
        }

        private static List<ParameterControl.Parameter> AddParameter<T>(this List<ParameterControl.Parameter> list, string name, T value, bool boolAsFloat = false)
        {
            list.Add(new ParameterControl.Parameter()
            {
                Name = name,
                Value = 
                    typeof(T) == typeof(int) ? (int)(object)value :
                    typeof(T) == typeof(float) ? (float)(object)value :
                    typeof(T) == typeof(bool) ? (bool)(object)value ? 1f : 0f :
                    0,
                ParameterType =
                    typeof(T) == typeof(int) ? AnimatorControllerParameterType.Int :
                    typeof(T) == typeof(float) ? AnimatorControllerParameterType.Float :
                    typeof(T) == typeof(bool) ? AnimatorControllerParameterType.Bool :
                    0,
                BoolAsFloat = boolAsFloat,
            });
            return list;
        }

        private static List<ParameterControl.Parameter> AddParameter(this List<ParameterControl.Parameter> list, string name, object value, Type type, bool boolAsFloat = false)
        {
            list.Add(new ParameterControl.Parameter()
            {
                Name = name,
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

        private static Dictionary<string, FieldInfo> _conditions;

        private static ParameterControl CreateControl<T>(
            Expression<Func<LilToonParameters, T>> parameter,
            Action<(string Name, LightControllerGenerator Generator, LilToonParameters Parameters, List<ParameterControl.Parameter> List)> setParam = null,
            Action<(string Path, Type Type, AnimationClip Default, AnimationClip Control, Material Material, LightControllerGenerator Generator, LilToonParameters Parameters)> setCurves = null,
            Func<LightControllerGenerator, bool> condition = null)
        {
            var targetField = (parameter.Body as MemberExpression).Member as FieldInfo;
            var nameAttr = targetField.GetCustomAttribute<NameAttribute>();
            var group = targetField.GetCustomAttribute<GroupAttribute>()?.Group;
            var name = nameAttr?.Name ?? targetField.Name;
            var menuName = nameAttr?.MenuName ?? (group != null && name.StartsWith(group) ? name.Substring(group.Length) : name);
            var isToggle = targetField.GetCustomAttribute<ToggleAttribute>() != null;


            if (setParam == null)
            {
                setParam = args => args.List.AddParameter(args.Name, targetField.GetValue(args.Parameters), targetField.FieldType, isToggle);
            }

            if (setCurves == null)
            {
                var range = targetField.GetCustomAttribute<RangeAttribute>();
                setCurves = args =>
                {
                    var (min, max) = range != null ? (range.min, range.max) : (0, 1);
                    var value = targetField.GetValue(args.Parameters);

                    float fValue = 0;
                    if (targetField.FieldType == typeof(bool))
                        fValue = (bool)value ? 1 : 0;
                    else
                        fValue = (float)value;

                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{targetField.Name}", AnimationUtils.Constant(fValue));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{targetField.Name}", AnimationUtils.Linear(min, max));
                };
            }

            if (condition == null)
            {
                if (group != null)
                {
                    if (_conditions == null)
                    {
                        _conditions = typeof(LightControllerGenerator).GetFields().Select(x => (Field: x, Attr: x.GetCustomAttribute<ConditionParameterAttribute>())).Where(x => x.Attr != null).ToDictionary(x => x.Attr.Name, x => x.Field);
                    }
                    if (_conditions.TryGetValue(group, out var cond))
                    {
                        condition = generator =>
                        {
                            return (bool)cond.GetValue(generator);
                        };
                    }
                }

                if (condition == null)
                    condition = _ => true;
            }

            return new ParameterControl()
            {
                Name = name,
                Group = group,
                Condition = condition,
                Parameters = setParam,
                SetAnimationCurves = setCurves,
                CreateMenu = args =>
                {
                    if (isToggle)
                        args.Controls.CreateToggle(menuName, name);
                    else
                        args.Controls.CreateRadialPuppet(menuName, name);
                },
            };
        }

        private sealed class ParameterControl
        {
            public string Name;
            public string Group = null;
            public Func<LightControllerGenerator, bool> Condition = _ => true;
            public Action<(string Name, LightControllerGenerator Generator, LilToonParameters Parameters, List<Parameter> List)> Parameters;
            public Action<(string Path, Type Type, AnimationClip Default, AnimationClip Control, Material Material, LightControllerGenerator Generator, LilToonParameters Parameters)> SetAnimationCurves;
            public Action<(string Name, ParameterControl Self, List<VRCExpressionsMenu.Control> Controls)> CreateMenu;
            public Action<(LilToonParameters Parameters, Material Material)> GetValueFromMaterial;
            public AnimationClip Control;
            public AnimationClip Default;


            public struct Parameter
            {
                public string Name;
                public float Value;
                public AnimatorControllerParameterType ParameterType;
                public bool BoolAsFloat;

                public ParameterConfig ToParameterConfig()
                {
                    var result = new ParameterConfig()
                    {
                        nameOrPrefix = Name,
                        defaultValue = Value,
                    };
                    switch (ParameterType)
                    {
                        case AnimatorControllerParameterType.Float:
                            result.syncType = ParameterSyncType.Float;
                            break;
                        case AnimatorControllerParameterType.Int:
                            result.syncType = ParameterSyncType.Int;
                            break;
                        case AnimatorControllerParameterType.Bool:
                            result.syncType = ParameterSyncType.Bool;
                            break;
                    }
                    return result;
                }

                public AnimatorControllerParameter ToControllerParameter()
                {
                    var result = new AnimatorControllerParameter()
                    {
                        name = Name,
                        type = ParameterType == AnimatorControllerParameterType.Bool && BoolAsFloat ? AnimatorControllerParameterType.Float : ParameterType,
                    };
                    switch (result.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            result.defaultFloat = Value;
                            break;
                        case AnimatorControllerParameterType.Int:
                            result.defaultInt = (int)Value;
                            break;
                        case AnimatorControllerParameterType.Bool:
                            result.defaultBool = Value != 0;
                            break;
                    }
                    return result;
                }
            }
        }
    }
}