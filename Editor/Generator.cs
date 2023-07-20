using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace gomoru.su.LightController
{
    public static class Generator
    {
        private const string PropertyNamePrefix = "material._";
        private const string ParameterNamePrefix = "LightController";

        static Generator()
        {
             _limitters = typeof(LightControllerGenerator).GetFields().Where(x => x.GetCustomAttribute<LimitParameterAttribute>() != null).ToDictionary(x => x.GetCustomAttribute<LimitParameterAttribute>().Name);
             _conditions = typeof(LightControllerGenerator).GetFields().Where(x => x.GetCustomAttribute<ConditionParameterAttribute>() != null).ToDictionary(x => x.GetCustomAttribute<ConditionParameterAttribute>().Name);

            Controls = new ParameterControl[]
            {
                CreateControl(param => param.UseLighting, setCurves: _ => {}),
                CreateControl(param => param.LightMinLimit),
                CreateControl(param => param.LightMaxLimit),
                CreateControl(param => param.MonochromeLighting),
                CreateControl(param => param.ShadowEnvStrength),
                CreateControl(param => param.AsUnlit),
                CreateControl(param => param.VertexLightStrength),

                CreateControl(param => param.UseBacklight),
                CreateControl(param => param.BacklightColor,
                    args => args.List.AddParameter(args.Name, args.Parameters.BacklightColor.a, LilToonParameters.GroupName_Backlight),
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

                CreateControl(param => param.UseDistanceFade, setCurves: _ => { }),
                CreateControl(param => param.DistanceFadeStart),
                CreateControl(param => param.DistanceFadeEnd),
                CreateControl(param => param.DistanceFadeStrength),
                CreateControl(param => param.DistanceFadeBackfaceForceShadow),
            };
        }

        private static readonly ParameterControl[] Controls;
        private static Dictionary<string, FieldInfo> _limitters;
        private static Dictionary<string, FieldInfo> _conditions;


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
            
            var controls = Controls.Where(x => x.Condition(generator)).ToArray();

            if (!controls.Any())
            {
                var installer = generator.GetComponent<ModularAvatarMenuInstaller>();
                if (installer != null)
                    installer.enabled = false;
                return;
            }

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
                if (!control.IsMaster)
                {
                    state.timeParameter = control.Name;
                    state.timeParameterActive = true;
                }

                idle.AddTransition(state, new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = control.Group });
                state.AddTransition(idle, new AnimatorCondition() { mode = AnimatorConditionMode.IfNot, parameter = control.Group });

                stateMachine.AddState(idle, stateMachine.entryPosition + new Vector3(150, 0));
                stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(150, 50));

                fx.AddLayer(layer);

                control.Parameters((control.Name, generator, @params, parameters));
            }

            var groups = controls.Select(y => y.Group).Where(y => y != null).Distinct();
            int groupCount = groups.Count();

            if (generator.AddResetButton)
            {
                var layer = new AnimatorControllerLayer()
                {
                    name = "Reset",
                    defaultWeight = 1,
                    stateMachine = new AnimatorStateMachine() { name = "Reset" }.HideInHierarchy().AddTo(fx),
                };
                var stateMachine = layer.stateMachine;

                var blank = new AnimationClip() { name = "Blank" }.HideInHierarchy().AddTo(fx);

                var idle = stateMachine.CreateState("Idle", blank);
                var states = new AnimatorState[groupCount];

                foreach(var (group, i) in groups.Select((x, i) => (x, i)))
                {
                    var state = stateMachine.CreateState(group, blank);
                    states[i] = state;
                    stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(300, i * 150 + 200));

                    var dr = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    dr.localOnly = true;
                    foreach(var x in parameters.Where(x => x.Group == group))
                    {
                        dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter()
                        {
                            name = x.Name,
                            type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                            value = x.Value,
                        });
                    }
                }

                stateMachine.AddState(idle, stateMachine.entryPosition + new Vector3(0, 200));

                for (int i = 0; i < states.Length; i++)
                {
                    var state = states[i];
                    for (int i2 = i + 1; i2 < states.Length; i2++)
                    {
                        var state2 = states[i2];

                        state.AddTransition(state2, new AnimatorCondition() { mode = AnimatorConditionMode.Equals, parameter = "Reset", threshold = i2 + 1 });
                        state2.AddTransition(state, new AnimatorCondition() { mode = AnimatorConditionMode.Equals, parameter = "Reset", threshold = i + 1 });
                    }

                    idle.AddTransition(state, new AnimatorCondition() { mode = AnimatorConditionMode.Equals, parameter = "Reset", threshold = i + 1 });
                    state.AddTransition(idle, new AnimatorCondition() { mode = AnimatorConditionMode.Equals, parameter = "Reset", threshold = 0 });
                }

                fx.AddLayer(layer);
                parameters.AddParameter("Reset", 0, null);
            }

            foreach (var parameter in parameters)
            {
                fx.AddParameter(parameter.ToControllerParameter());
            }

            go.GetOrAddComponent<ModularAvatarMergeAnimator>(x =>
            {
                x.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                x.animator = fx;
                x.matchAvatarWriteDefaults = true;
                x.pathMode = MergeAnimatorPathMode.Absolute;
            });

            go.GetOrAddComponent<ModularAvatarMenuInstaller>(x =>
            {
                var mainMenu = CreateExpressionMenu("Main Menu").AddTo(fx);
             
                Dictionary<string, VRCExpressionsMenu> categories = new Dictionary<string, VRCExpressionsMenu>();

                foreach (var control in controls)
                {
                    var menu = mainMenu;
                    if (groupCount >= 2 && !string.IsNullOrEmpty(control.Group))
                    {
                        if (!categories.TryGetValue(control.Group, out menu))
                        {
                            menu = CreateExpressionMenu($"{control.Group} Menu").AddTo(fx);
                            mainMenu.controls.Add(new VRCExpressionsMenu.Control() { name = control.Group, type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = menu });
                            categories.Add(control.Group, menu);
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

                if (generator.AddResetButton)
                {
                    foreach(var (category, i) in categories.Select((a, i) => (a, i)))
                    {
                        var menu = category.Value;
                        Debug.Log($"{category} {i}");
                        menu.controls.Insert(1, new VRCExpressionsMenu.Control()
                        {
                            name = "Reset",
                            type = VRCExpressionsMenu.Control.ControlType.Button,
                            parameter = new VRCExpressionsMenu.Control.Parameter() { name = $"Reset" },
                            value = i + 1
                        });
                    }
                }
            });

            go.GetOrAddComponent<ModularAvatarParameters>(component =>
            {
                var syncSettings = typeof(ParameterSyncSettings).GetFields().ToDictionary(x => x.Name, x => (bool)x.GetValue(generator.SyncSettings));
                component.parameters.Clear();
                component.parameters.AddRange(parameters.Select(x =>
                {
                    var p = x.ToParameterConfig();
                    p.saved = generator.SaveParameters;
                    p.remapTo = $"{ParameterNamePrefix}{p.nameOrPrefix}";
                    if (x.Group == null || (syncSettings.TryGetValue(x.Group, out var flag) && !flag))
                    {
                        p.syncType = ParameterSyncType.NotSynced;
                    }
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

        private static List<ParameterControl.Parameter> AddParameter<T>(this List<ParameterControl.Parameter> list, string name, T value, string group, bool boolAsFloat = false) => AddParameter(list, name, value, typeof(T), group, boolAsFloat);

        private static List<ParameterControl.Parameter> AddParameter(this List<ParameterControl.Parameter> list, string name, object value, Type type, string group, bool boolAsFloat = false)
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

        private static ParameterControl CreateControl<T>(
            Expression<Func<LilToonParameters, T>> parameter,
            Action<(string Name, LightControllerGenerator Generator, LilToonParameters Parameters, List<ParameterControl.Parameter> List)> setParam = null,
            Action<(string Path, Type Type, AnimationClip Default, AnimationClip Control, Material Material, LightControllerGenerator Generator, LilToonParameters Parameters)> setCurves = null,
            Func<LightControllerGenerator, bool> condition = null)
        {
            var targetField = (parameter.Body as MemberExpression).Member as FieldInfo;
            var attributes = targetField.GetCustomAttributes();

            var nameAttr = attributes.GetAttribute<NameAttribute>(); 
            var group = attributes.GetAttribute<GroupAttribute>()?.Group;
            var isMaster = attributes.GetAttribute<GroupMasterAttribute>() != null;
            var isToggle = attributes.GetAttribute<ToggleAttribute>() != null;
            var vectorProxy = attributes.GetAttribute<VectorProxyAttribute>();

            var name = isMaster ? group : nameAttr?.Name ?? targetField.Name;
            var menuName = nameAttr?.MenuName ?? (group != null && name.StartsWith(group) ? name.Substring(group.Length) : name);

            bool boolAsFloat = !isMaster && isToggle;

            if (setParam == null)
            {
                setParam = args =>
                {
                    if (_limitters.TryGetValue(targetField.Name, out var limitField))
                    {
                        var limit = (float)limitField.GetValue(args.Generator);
                        var value = (float)targetField.GetValue(args.Parameters);
                        value /= limit;
                        args.List.AddParameter(args.Name, value, group, !isMaster && isToggle);
                    }
                    else
                    {
                        args.List.AddParameter(args.Name, targetField.GetValue(args.Parameters), targetField.FieldType, group, !isMaster && isToggle);
                    }
                };
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

                    if (_limitters.TryGetValue(targetField.Name, out var limitField))
                    {
                        var limit = (float)limitField.GetValue(args.Generator);
                        max = limit;
                    }


                    if (isMaster)
                    {
                        var prop = $"{PropertyNamePrefix}{targetField.Name}";
                        args.Default.SetCurve(args.Path, args.Type, prop, AnimationUtils.Constant(min));
                        args.Control.SetCurve(args.Path, args.Type, prop, AnimationUtils.Constant(max));
                    }
                    else
                    {
                        string prop;
                        if (vectorProxy != null)
                        {
                            var target = vectorProxy.TargetName;
                            prop = $"{PropertyNamePrefix}{target}.{"xyzw"[vectorProxy.Index]}";
                        }
                        else
                        {
                            prop = $"{PropertyNamePrefix}{targetField.Name}";
                        }
                        args.Default.SetCurve(args.Path, args.Type, prop, AnimationUtils.Constant(fValue));
                        args.Control.SetCurve(args.Path, args.Type, prop, AnimationUtils.Linear(min, max));
                    }
                };
            }

            if (condition == null)
            {
                if (group != null)
                {
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
                IsMaster = isMaster,
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

        private static T GetAttribute<T>(this IEnumerable<Attribute> attributes) where T : Attribute => attributes.FirstOrDefault(x => x is T) as T;

        private sealed class ParameterControl
        {
            public string Name;
            public string Group = null;
            public bool IsMaster = false;
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
                public string Group;
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