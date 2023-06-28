using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace gomoru.su.LightController
{
    public static class Generator
    {
        private const string PropertyNamePrefix = "material._";
        private const string ParameterNamePrefix = "LightController";

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
                            y.shader.EnumeratePropertyNames().Any(z => z == $"_{nameof(LilToonLightParameters.LightMinLimit)}"))
                        ))
                .Where(x => x.Material != null);

            var clips = (
                Default: fx.CreateAnim("Default"),
                Min: fx.CreateAnim("Min"),
                Max: fx.CreateAnim("Max"),
                Monochrome: fx.CreateAnim("Monochrome"),
                ShadowEnv: fx.CreateAnim("ShadowEnvStrength"),
                AsUnlit: fx.CreateAnim("AsUnlit"),
                VertexLight: fx.CreateAnim("VertexLightStrength")
                );

            var @params = generator.DefaultParameters;

            foreach (var (renderer, material) in targets)
            {
                var path = renderer.transform.GetRelativePath(avatarObject.transform);
                var type = renderer.GetType();
                if (generator.UseMaterialPropertyAsDefault)
                {
                    SetParametersFromMaterial(ref @params, material);
                }

                clips.Default.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.LightMinLimit)}", AnimationUtils.Constant(@params.LightMinLimit));
                clips.Default.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.LightMaxLimit)}", AnimationUtils.Constant(@params.LightMaxLimit));
                clips.Default.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.MonochromeLighting)}", AnimationUtils.Constant(@params.MonochromeLighting));
                clips.Default.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.ShadowEnvStrength)}", AnimationUtils.Constant(@params.ShadowEnvStrength));
                clips.Default.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.AsUnlit)}", AnimationUtils.Constant(@params.AsUnlit));
                clips.Default.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.VertexLightStrength)}", AnimationUtils.Constant(@params.VertexLightStrength));

                clips.Min.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.LightMinLimit)}", AnimationUtils.Linear(0, 1));

                clips.Max.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.LightMaxLimit)}", AnimationUtils.Linear(0, generator.LightMaxLimitMax));

                clips.Monochrome.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.MonochromeLighting)}", AnimationUtils.Linear(0, 1));

                clips.ShadowEnv.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.ShadowEnvStrength)}", AnimationUtils.Linear(0, 1));

                clips.AsUnlit.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.AsUnlit)}", AnimationUtils.Linear(0, 1));

                clips.VertexLight.SetCurve(path, type, $"{PropertyNamePrefix}{nameof(LilToonLightParameters.VertexLightStrength)}", AnimationUtils.Linear(0, 1));
            }

            var enumerable = clips.ToEnumerable().Skip(1).Cast<AnimationClip>().Select((x, i) => (x, i));

            // Build AnimatorController
            {
                var layer = new AnimatorControllerLayer()
                {
                    name = "LightController",
                    defaultWeight = 1,
                    stateMachine = new AnimatorStateMachine() { name = "LightController" }.HideInHierarchy().AddTo(fx),
                };

                var stateMachine = layer.stateMachine;

                var idle = stateMachine.CreateState("Idle", fx.CreateAnim("Blank"));
                var @default = stateMachine.CreateState("Default", clips.Default);

                idle.AddTransition(@default, new AnimatorCondition() { mode = AnimatorConditionMode.IfNot, parameter = "Enabled" });
                @default.AddTransition(idle, new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = "Enabled" });

                int idx = 0;
                AddControlState(clips.Min, @params.LightMinLimit, new Vector3(300, idx++ * 100));
                AddControlState(clips.Max, @params.LightMaxLimit, new Vector3(300, idx++ * 100));
                AddControlState(clips.Monochrome, @params.MonochromeLighting, new Vector3(300, idx++ * 100));
                AddControlState(clips.ShadowEnv, @params.ShadowEnvStrength, new Vector3(300, idx++ * 100));
                AddControlState(clips.AsUnlit, @params.AsUnlit, new Vector3(300, idx++ * 100));
                AddControlState(clips.VertexLight, @params.VertexLightStrength, new Vector3(300, idx++ * 100));

                var stroke = clips.ToEnumerable().Skip(1).Cast<AnimationClip>().Select((x, i) =>
                {
                    var state = stateMachine.CreateState(x.name, x);
                    state.timeParameter = x.name;
                    state.timeParameterActive = true;
                    stateMachine.AddState(state, new Vector3(100 * i, 400));
                    return state;

                });

                stroke.Zip(stroke.Skip(1), (x, y) => (x, y)).Select(x =>
                {
                    Debug.Log($"{x.x.name} {x.y.name}");
                    return x;
                }).All(x => true);

                stateMachine.AddState(idle, new Vector3(0, 0));
                stateMachine.defaultState = idle;
                stateMachine.AddState(@default, new Vector3(0, 100));

                fx.AddLayer(layer);
                fx.AddParameter("Enabled", AnimatorControllerParameterType.Bool);

                AnimatorState AddControlState(AnimationClip clip, float defaultValue, Vector3 position)
                {
                    var state = stateMachine.CreateState(clip.name, clip);
                    state.timeParameter = clip.name;
                    state.timeParameterActive = true;
                    fx.AddParameter(new AnimatorControllerParameter() { name = clip.name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });
                    fx.AddParameter(new AnimatorControllerParameter() { name = $"Control{clip.name}", type = AnimatorControllerParameterType.Bool, defaultBool = false });

                    idle.AddTransition(state, new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = $"Control{clip.name}" });
                    state.AddTransition(idle, new AnimatorCondition() { mode = AnimatorConditionMode.IfNot, parameter = $"Control{clip.name}" });

                    state.AddTransition(@default, new AnimatorCondition() { mode = AnimatorConditionMode.IfNot, parameter = "Enabled" });

                    stateMachine.AddState(state, position);

                    return state;
                }

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

                mainMenu.controls.Add(CreateControl(clips.Min.name));
                mainMenu.controls.Add(CreateControl(clips.Max.name));
                mainMenu.controls.Add(CreateControl(clips.Monochrome.name));
                mainMenu.controls.Add(CreateControl(clips.ShadowEnv.name));
                mainMenu.controls.Add(CreateControl(clips.AsUnlit.name));
                mainMenu.controls.Add(CreateControl(clips.VertexLight.name));

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

        }

        private static AnimationClip CreateAnim(this AnimatorController parent, string name = null) => new AnimationClip() { name = name }.AddTo(parent);
        private static AnimatorState CreateState(this AnimatorStateMachine parent, string name, AnimationClip motion = null) => new AnimatorState() { name = name, writeDefaultValues = false, motion = motion }.HideInHierarchy().AddTo(parent);
        private static VRCExpressionsMenu.Control CreateControl(string name) => new VRCExpressionsMenu.Control()
        {
            name = name,
            type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
            parameter = new VRCExpressionsMenu.Control.Parameter() { name = $"Control{name}" },
            subParameters = new VRCExpressionsMenu.Control.Parameter[] { new VRCExpressionsMenu.Control.Parameter() { name = name } }
        };

        private static void SetParametersFromMaterial(ref LilToonLightParameters parameters, Material material)
        {
            parameters.LightMinLimit = material.GetFloat($"_{nameof(parameters.LightMinLimit)}");
            parameters.LightMaxLimit = material.GetFloat($"_{nameof(parameters.LightMaxLimit)}");
            parameters.MonochromeLighting = material.GetFloat($"_{nameof(parameters.MonochromeLighting)}");
            parameters.ShadowEnvStrength = material.GetFloat($"_{nameof(parameters.ShadowEnvStrength)}");
            parameters.AsUnlit = material.GetFloat($"_{nameof(parameters.AsUnlit)}");
            parameters.VertexLightStrength = material.GetFloat($"_{nameof(parameters.VertexLightStrength)}");
        }
    }
}
