using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
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
            new ParameterControl()
            {
                Name = "Min",
                AddParameters = args => args.FX.AddParameter(args.Name, args.Parameters.LightMinLimit),
                SetAnimationCurves = args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.LightMinLimit)}", AnimationUtils.Constant(args.Parameters.LightMinLimit));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.LightMinLimit)}", AnimationUtils.Linear(0, 1));
                },
                CreateMenu = args => args.Controls.CreateRadialPuppet(args.Name),
            },
            new ParameterControl()
            {
                Name = "Max",
                AddParameters = args => args.FX.AddParameter(args.Name, (1 + args.Parameters.LightMaxLimit) / args.Generator.LightMaxLimitMax),
                SetAnimationCurves = args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.LightMaxLimit)}", AnimationUtils.Constant(args.Parameters.LightMaxLimit));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.LightMaxLimit)}", AnimationUtils.Linear(0, args.Generator.LightMaxLimitMax));
                },
                CreateMenu = args => args.Controls.CreateRadialPuppet(args.Name),
            },
            new ParameterControl()
            {
                Name = "Monochrome",
                AddParameters = args => args.FX.AddParameter(args.Name, args.Parameters.MonochromeLighting),
                SetAnimationCurves = args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.MonochromeLighting)}", AnimationUtils.Constant(args.Parameters.MonochromeLighting));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.MonochromeLighting)}", AnimationUtils.Linear(0, 1));
                },
                CreateMenu = args => args.Controls.CreateRadialPuppet(args.Name),
            },
            new ParameterControl()
            {
                Name = "ShadowEnvStrength",
                AddParameters = args => args.FX.AddParameter(args.Name, args.Parameters.ShadowEnvStrength),
                SetAnimationCurves = args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.ShadowEnvStrength)}", AnimationUtils.Constant(args.Parameters.ShadowEnvStrength));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.ShadowEnvStrength)}", AnimationUtils.Linear(0, 1));
                },
                CreateMenu = args => args.Controls.CreateRadialPuppet(args.Name),
            },
            new ParameterControl()
            {
                Name = "AsUnlit",
                AddParameters = args => args.FX.AddParameter(args.Name, args.Parameters.AsUnlit),
                SetAnimationCurves = args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.AsUnlit)}", AnimationUtils.Constant(args.Parameters.AsUnlit));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.AsUnlit)}", AnimationUtils.Linear(0, 1));
                },
                CreateMenu = args => args.Controls.CreateRadialPuppet(args.Name),
            },
            new ParameterControl()
            {
                Name = "VertexLightStrength",
                AddParameters = args => args.FX.AddParameter(args.Name, args.Parameters.VertexLightStrength),
                SetAnimationCurves = args =>
                {
                    args.Default.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.VertexLightStrength)}", AnimationUtils.Constant(args.Parameters.VertexLightStrength));
                    args.Control.SetCurve(args.Path, args.Type, $"{PropertyNamePrefix}{nameof(LilToonParameters.VertexLightStrength)}", AnimationUtils.Linear(0, 1));
                },
                CreateMenu = args => args.Controls.CreateRadialPuppet(args.Name),
            },
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

            foreach (var control in Controls)
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

                foreach(var control in Controls)
                {
                    control.SetAnimationCurves((path, type, control.Default, control.Control, material, generator, @params));
                }
            }

            fx.AddParameter("Enabled", AnimatorControllerParameterType.Bool);

            foreach (var control in Controls)
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

                control.AddParameters((control.Name, fx, generator, @params));

                fx.AddLayer(layer);
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
                var mainMenu = new VRCExpressionsMenu() { name = "Main Menu" }.AddTo(fx);
                mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Enable",
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter() { name = "Enabled" },
                });

                Dictionary<string, VRCExpressionsMenu> category = new Dictionary<string, VRCExpressionsMenu>();

                foreach (var control in Controls)
                {
                    var menu = mainMenu;
                    if (!string.IsNullOrEmpty(control.Group))
                    {
                        if (!category.TryGetValue(control.Group, out menu))
                        {
                            menu = new VRCExpressionsMenu() { name = $"{control.Group} Menu" }.AddTo(fx);
                            mainMenu.controls.Add(new VRCExpressionsMenu.Control() { name = control.Group, type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = menu });
                            category.Add(control.Group, menu);
                        }
                    }

                    control.CreateMenu((control.Name, control, menu.controls));
                }

                x.menuToAppend = new VRCExpressionsMenu()
                {
                    name = "Menu",
                    controls =
                    {
                        new VRCExpressionsMenu.Control()
                        {
                            name = "Light Controller",
                            type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                            subMenu = mainMenu,
                        }
                    }
                }.AddTo(fx);
            });

            go.GetOrAddComponent<ModularAvatarParameters>(component =>
            {
                component.parameters.AddRange(fx.parameters.Select(x =>
                {
                    var p = x.ToParameterConfig();
                    p.saved = generator.SaveParameters;
                    p.remapTo = $"{ParameterNamePrefix}{p.nameOrPrefix}";
                    return p;
                }));
            });
        }

        private static ParameterConfig ToParameterConfig(this AnimatorControllerParameter parameter)
        {
            var result = new ParameterConfig()
            {
                nameOrPrefix = parameter.name,
            };
            switch(parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    result.syncType = ParameterSyncType.Float;
                    result.defaultValue = parameter.defaultFloat;
                    break;
                case AnimatorControllerParameterType.Int:
                    result.syncType = ParameterSyncType.Int;
                    result.defaultValue = parameter.defaultInt;
                    break;
                case AnimatorControllerParameterType.Bool:
                    result.syncType = ParameterSyncType.Bool;
                    result.defaultValue = parameter.defaultBool ? 1 : 0;
                    break;
            }
            return result;
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
        
        private sealed class ParameterControl
        {
            public string Name;
            public string Group = null;
            public Action<(string Name, AnimatorController FX, LightControllerGenerator Generator, LilToonParameters Parameters)> AddParameters;
            public Action<(string Path, Type Type, AnimationClip Default, AnimationClip Control, Material Material, LightControllerGenerator Generator, LilToonParameters Parameters)> SetAnimationCurves;
            public Action<(string Name, ParameterControl Self, List<VRCExpressionsMenu.Control> Controls)> CreateMenu; 
            public AnimationClip Control;
            public AnimationClip Default;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit {}
}