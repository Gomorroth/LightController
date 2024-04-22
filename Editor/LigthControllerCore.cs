using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using ExpressionMenuItemType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace gomoru.su.LightController;

internal sealed partial class LightControllerCore
{
    private const string PropertyNamePrefix = "material._";

    private static void Generate(BuildContext context, LightController controller)
    {
        var go = controller.gameObject;

        var renderers = context.AvatarRootObject.GetComponentsInChildren<Renderer>(true)
            .Where(x => (x is MeshRenderer || x is SkinnedMeshRenderer) && !x.CompareTag("EditorOnly") && !controller.Excludes.Contains(x.gameObject))
            .Select(x => (Renderer: x, Material: x.sharedMaterials.FirstOrDefault(y => y != null && y.shader.name.Contains("lilToon", StringComparison.OrdinalIgnoreCase))))
            .Where(x => x.Material != null)
            .Select(x => x.Renderer);
        var bindings = renderers
            .Select(x => new EditorCurveBinding() { type = x.GetType(), path = x.gameObject.AvatarRootPath() })
            .ToArray();

        var lilToonParameters = controller.DefaultParameters;

        if (controller.OverwriteMaterialSettings)
        {
            var map = new Dictionary<Material, Material>();
            foreach (var x in renderers)
            {
                var mats = x.sharedMaterials;
                foreach (ref var mat in mats.AsSpan())
                {
                    if (!map.TryGetValue(mat, out var cloned))
                    {
                        cloned = Material.Instantiate(mat);
                        ObjectRegistry.RegisterReplacedObject(mat, cloned);
                        map.Add(mat, cloned);
                    }
                    mat = cloned;
                }
                x.sharedMaterials = mats;
            }
            SetMaterialParameters(map.Values.ToArray(), lilToonParameters);
        }

        var parameters = new List<ParameterConfig>()
        {
            new() { nameOrPrefix = "One", syncType = ParameterSyncType.NotSynced, localOnly = true, defaultValue = 1, },
            new() { nameOrPrefix = "ControledParameterIndex", syncType = ParameterSyncType.Int, localOnly = true, defaultValue = 0, },
            new() { nameOrPrefix = "SyncTargetIndex", syncType = ParameterSyncType.Int, localOnly = false, defaultValue = 0, },
            new() { nameOrPrefix = "SyncedValue", syncType = ParameterSyncType.Float, localOnly = false, defaultValue = 0, },
        };

        parameters.AddRange(new ParameterConfig[]
        {
            new() { nameOrPrefix = "LightMinLimit", defaultValue = ClampParameterValue(lilToonParameters.LightMinLimit, lilToonParameters.LightMinLimitMin, lilToonParameters.LightMinLimitMax), },
            new() { nameOrPrefix = "LightMaxLimit", defaultValue = ClampParameterValue(lilToonParameters.LightMaxLimit, lilToonParameters.LightMaxLimitMin, lilToonParameters.LightMaxLimitMax), },
            new() { nameOrPrefix = "MonochromeLighting", defaultValue = lilToonParameters.MonochromeLighting, },
            new() { nameOrPrefix = "ShadowEnvStrength", defaultValue = lilToonParameters.ShadowEnvStrength, },
            new() { nameOrPrefix = "AsUnlit", defaultValue = lilToonParameters.AsUnlit, },
            new() { nameOrPrefix = "VertexLightStrength", defaultValue = lilToonParameters.VertexLightStrength, },

            new() { nameOrPrefix = "UseBacklight", defaultValue = lilToonParameters.UseBacklight ? 1 : 0, syncType = ParameterSyncType.Bool },
            new() { nameOrPrefix = "BacklightColor/R", defaultValue = lilToonParameters.BacklightColor.r, }, 
            new() { nameOrPrefix = "BacklightColor/G", defaultValue = lilToonParameters.BacklightColor.g, }, 
            new() { nameOrPrefix = "BacklightColor/B", defaultValue = lilToonParameters.BacklightColor.b, }, 
            new() { nameOrPrefix = "BacklightColor/A", defaultValue = lilToonParameters.BacklightColor.a, },
            new() { nameOrPrefix = "BacklightMainStrength", defaultValue = lilToonParameters.BacklightMainStrength, },
            new() { nameOrPrefix = "BacklightReceiveShadow", defaultValue = lilToonParameters.BacklightReceiveShadow ? 1 : 0, syncType = ParameterSyncType.Bool },
            new() { nameOrPrefix = "BacklightBackfaceMask", defaultValue = lilToonParameters.BacklightBackfaceMask ? 1 : 0, syncType = ParameterSyncType.Bool },
            new() { nameOrPrefix = "BacklightNormalStrength", defaultValue = lilToonParameters.BacklightNormalStrength, },
            new() { nameOrPrefix = "BacklightBorder", defaultValue = lilToonParameters.BacklightBorder, },
            new() { nameOrPrefix = "BacklightBlur", defaultValue = lilToonParameters.BacklightBlur, },
            new() { nameOrPrefix = "BacklightDirectivity", defaultValue = lilToonParameters.BacklightDirectivity, },
            new() { nameOrPrefix = "BacklightViewStrength", defaultValue = lilToonParameters.BacklightViewStrength, },

            new() { nameOrPrefix = "DistanceFadeStart", defaultValue = lilToonParameters.DistanceFadeStart, },
            new() { nameOrPrefix = "DistanceFadeEnd", defaultValue = lilToonParameters.DistanceFadeEnd, },
            new() { nameOrPrefix = "DistanceFadeStrength", defaultValue = lilToonParameters.DistanceFadeStrength, },
            new() { nameOrPrefix = "DistanceFadeBackfaceForceShadow", defaultValue = lilToonParameters.DistanceFadeBackfaceForceShadow ? 1 : 0, syncType = ParameterSyncType.Bool },
        }
        .Select(x => x.syncType is ParameterSyncType.Bool ? x with { localOnly = false } : x with { syncType = ParameterSyncType.Float, localOnly = true }));
        var parameterIDTable = parameters.Skip(4).Where(x => !x.nameOrPrefix.Contains("Cache")).Select((x, i) => (x.nameOrPrefix, i)).ToImmutableDictionary(x => x.nameOrPrefix, x => x.i + 1);

        var blendTree = new DirectBlendTree("One") { Name = "Light Controller" };

        // Lighting
        {
            var lighting = blendTree.AddDirectBlendTree("Lighting");

            AddControl(lighting, "LightMinLimit", lilToonParameters.LightMinLimitMin, lilToonParameters.LightMinLimitMax);
            AddControl(lighting, "LightMaxLimit", lilToonParameters.LightMaxLimitMin, lilToonParameters.LightMaxLimitMax);
            AddControl(lighting, "MonochromeLighting", 0, 1);
            AddControl(lighting, "ShadowEnvStrength", 0, 1);
            AddControl(lighting, "AsUnlit", 0, 1);
            AddControl(lighting, "VertexLightStrength", 0, 1);

            void AddControl(DirectBlendTree target, string name, float min, float max)
            {
                var tree = target.AddBlendTree(name);
                tree.ParameterName = name;
                var enable0 = new AnimationClip() { name = "Start" }.AddTo(context.AssetContainer);
                var enable1 = new AnimationClip() { name = "End" }.AddTo(context.AssetContainer);
                foreach (var bind in bindings)
                {
                    var x = bind with { propertyName = $"{PropertyNamePrefix}{name}" };

                    AnimationUtility.SetEditorCurve(enable0, x, AnimationUtils.Constant(min));
                    AnimationUtility.SetEditorCurve(enable1, x, AnimationUtils.Constant(max));
                }
                tree.Motions.Add(enable0);
                tree.Motions.Add(enable1);
            }
        }

        // Backlight
        {
            var backlightRoot = blendTree.AddToggle("Backlight");
            backlightRoot.ParameterName = "UseBacklight";
            var backlight = (
                Disable: backlightRoot.AddDirectBlendTree(DirectBlendTree.Target.OFF, "Disable"),
                Enable: backlightRoot.AddDirectBlendTree(DirectBlendTree.Target.ON, "Enable")
                );

            {
                var disable = new AnimationClip() { name = "Disable" }.AddTo(context.AssetContainer);
                var enable = new AnimationClip() { name = "Enable" }.AddTo(context.AssetContainer);

                foreach (var bind in bindings)
                {
                    var x = bind with { propertyName = $"{PropertyNamePrefix}UseBacklight" };

                    AnimationUtility.SetEditorCurve(disable, x, AnimationUtils.Constant(0));
                    AnimationUtility.SetEditorCurve(enable, x, AnimationUtils.Constant(1));
                }

                backlight.Disable.AddMotion(disable);
                backlight.Enable.AddMotion(enable);
            }
            
            // BacklightColor
            {
                var disable = new AnimationClip() { name = "BacklightColor" }.AddTo(context.AssetContainer);
                var tree = backlight.Enable.AddDirectBlendTree("Color");

                var r = tree.AddBlendTree("R");
                var g = tree.AddBlendTree("G");
                var b = tree.AddBlendTree("B");
                var a = tree.AddBlendTree("A");
                var array = new[] { r, g, b, a };

                foreach(var x in array)
                {
                    x.ParameterName = $"BacklightColor/{x.Name}";
                    x.Motions.Add(new AnimationClip() { name = "Start" }.AddTo(context.AssetContainer));
                    x.Motions.Add(new AnimationClip() { name = "End" }.AddTo(context.AssetContainer));
                }

                foreach (var bind in bindings)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        var color = array[i];
                        var x = bind with { propertyName = $"{PropertyNamePrefix}BacklightColor.{color.Name.ToLowerInvariant()}" };

                        AnimationUtility.SetEditorCurve(disable, x, AnimationUtils.Constant(lilToonParameters.BacklightColor[i]));
                        AnimationUtility.SetEditorCurve(color.Motions[0] as AnimationClip, x, AnimationUtils.Constant(0));
                        AnimationUtility.SetEditorCurve(color.Motions[1] as AnimationClip, x, AnimationUtils.Constant(1));
                    }
                }

                backlight.Disable.AddMotion(disable);
            }

            AddControl(backlight, "BacklightMainStrength", lilToonParameters.BacklightMainStrength, 0, 1);
            AddControl(backlight, "BacklightReceiveShadow", lilToonParameters.BacklightReceiveShadow ? 1 : 0, 0, 1);
            AddControl(backlight, "BacklightBackfaceMask", lilToonParameters.BacklightBackfaceMask ? 1 : 0, 0, 1);
            AddControl(backlight, "BacklightNormalStrength", lilToonParameters.BacklightNormalStrength, 0, 1);
            AddControl(backlight, "BacklightBorder", lilToonParameters.BacklightBorder, 0, 1);
            AddControl(backlight, "BacklightBlur", lilToonParameters.BacklightBlur, 0, 1);
            AddControl(backlight, "BacklightDirectivity", lilToonParameters.BacklightDirectivity, 0, 20);
            AddControl(backlight, "BacklightViewStrength", lilToonParameters.BacklightViewStrength, 0, 1);

            void AddControl(in (DirectBlendTree Disable, DirectBlendTree Enable) target, string name, float defaultValue, float min, float max)
            {
                var tree = target.Enable.AddBlendTree(name["Backlight".Length..]);
                tree.ParameterName = name;
                var disable = new AnimationClip() { name = name }.AddTo(context.AssetContainer);
                var enable0 = new AnimationClip() { name = "Start" }.AddTo(context.AssetContainer);
                var enable1 = new AnimationClip() { name = "End" }.AddTo(context.AssetContainer);
                foreach (var bind in bindings)
                {
                    var x = bind with { propertyName = $"{PropertyNamePrefix}{name}" };

                    AnimationUtility.SetEditorCurve(disable, x, AnimationUtils.Constant(defaultValue));
                    AnimationUtility.SetEditorCurve(enable0, x, AnimationUtils.Constant(min));
                    AnimationUtility.SetEditorCurve(enable1, x, AnimationUtils.Constant(max));
                }
                target.Disable.AddMotion(disable);
                tree.Motions.Add(enable0);
                tree.Motions.Add(enable1);
            }
        }

        //Distance Fade
        {
            var distFade = blendTree.AddDirectBlendTree("DistanceFade");

            AddControl(distFade, "DistanceFadeStart", 0, lilToonParameters.DistanceFadeStart, 0, 1);
            AddControl(distFade, "DistanceFadeEnd", 1, lilToonParameters.DistanceFadeEnd, 0, 1);
            AddControl(distFade, "DistanceFadeStrength", 2, lilToonParameters.DistanceFadeStrength, 0, 1);
            AddControl(distFade, "DistanceFadeBackfaceForceShadow", 3, lilToonParameters.DistanceFadeBackfaceForceShadow ? 1 : 0, 0, 1);

            void AddControl(DirectBlendTree target, string name, int index, float defaultValue, float min, float max)
            {
                var tree = target.AddBlendTree(name["DistanceFade".Length..]);
                tree.ParameterName = name;
                var enable0 = new AnimationClip() { name = "Start" }.AddTo(context.AssetContainer);
                var enable1 = new AnimationClip() { name = "End" }.AddTo(context.AssetContainer);
                foreach (var bind in bindings)
                {
                    var x = bind with { propertyName = $"{PropertyNamePrefix}DistanceFade.{"xyzw"[index]}" };

                    AnimationUtility.SetEditorCurve(enable0, x, AnimationUtils.Constant(min));
                    AnimationUtility.SetEditorCurve(enable1, x, AnimationUtils.Constant(max));
                }
                tree.Motions.Add(enable0);
                tree.Motions.Add(enable1);
            }
        }

        var fx = new AnimatorController() { name = "LightController" }.AddTo(context.AssetContainer);
        fx.AddLayer(blendTree.ToAnimatorControllerLayer(context.AssetContainer));

        var parameterSync = new AnimatorController() { name = "Light Controller Parameter Sync" }.AddTo(context.AssetContainer);
        parameterSync.AddParameter("IsLocal", false);

        {
            parameterSync.AddLayer("Light Controller Parameter Sync");
            var layer = parameterSync.layers[^1];
            layer.defaultWeight = 1;

            var blank = new AnimationClip() { name = "Blank" };

            var idleState = AddState("Idle");
            var localState = AddState("Local", (idleState, new() { parameter = "IsLocal", mode = AnimatorConditionMode.If }));
            var remoteState = AddState("Remote", (idleState, new() { parameter = "IsLocal", mode = AnimatorConditionMode.IfNot }));

            foreach(var x in parameterIDTable.Keys)
            {
                AddParameterController(x);
            }

            void AddParameterController(string name)
            {
                int index = parameterIDTable[name];
                var localWait = AddState($"{name} (Wait)", (localState, new() { parameter = "ControledParameterIndex", mode = AnimatorConditionMode.Equals, threshold = index }));
                var local = AddState ($"{name} (Local => Remote)" , (localWait, new() { parameter = "ControledParameterIndex", mode = AnimatorConditionMode.NotEqual, threshold = index }));
                local.AddTransition(localState, new AnimatorCondition() { parameter = "ControledParameterIndex", mode = AnimatorConditionMode.NotEqual, threshold = index });
                
                var remote = AddState($"{name} (Remote => Local)", (remoteState, new() { parameter = "SyncTargetIndex", mode = AnimatorConditionMode.Equals, threshold = index }));
                remote.AddTransition(remoteState, new AnimatorCondition() { parameter = "SyncTargetIndex", mode = AnimatorConditionMode.NotEqual, threshold = index });

                var dr = local.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                dr.parameters.Add(new() { name = "SyncTargetIndex", type = VRC_AvatarParameterDriver.ChangeType.Set, value = index });
                dr.parameters.Add(new() { name = "SyncedValue", type = VRC_AvatarParameterDriver.ChangeType.Copy ,source = name });

                dr = remote.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                dr.parameters.Add(new() { name = name, type = VRC_AvatarParameterDriver.ChangeType.Copy, source = "SyncedValue" });
                dr.parameters.Add(new() { name = "SyncTargetIndex", type = VRC_AvatarParameterDriver.ChangeType.Set, value = 0 });
            }

            AnimatorState AddState(string name, (AnimatorState Parent, AnimatorCondition Condition)? parent = null)
            {
                var x = layer.stateMachine.AddState(name);
                x.motion = blank;
                x.writeDefaultValues = false;
                if (parent is { } p)
                {
                    p.Parent.AddTransition(x, p.Condition);
                }
                return x;
            }
        }

        foreach (ref var x in parameters.AsSpan())
        {
            x.remapTo = $"LightController/{x.nameOrPrefix}";
            var p = new AnimatorControllerParameter()
            {
                name = x.nameOrPrefix,
                type = x.syncType switch
                {
                    ParameterSyncType.Int => AnimatorControllerParameterType.Int,
                    _ => AnimatorControllerParameterType.Float,
                },
                defaultFloat = x.defaultValue,
                defaultInt = (int)x.defaultValue,
            };
            fx.AddParameter(p);
            parameterSync.AddParameter(p);
        }

        {
            var root = go.GetOrAddComponent<ModularAvatarMenuItem>();
            root.MenuSource = SubmenuSource.Children;
            root.Control.type = ExpressionMenuItemType.SubMenu;

            new SubMenu()
            {
                Name = "Lighting", Children = new[]
                {
                    new RadialPuppet(){ ParameterName = "LightMinLimit", Name = "Min", },
                    new RadialPuppet(){ ParameterName = "LightMaxLimit", Name = "Max", },
                    new RadialPuppet(){ ParameterName = "MonochromeLighting", Name = "Monochrome" },
                    new RadialPuppet(){ ParameterName = "ShadowEnvStrength", },
                    new RadialPuppet(){ ParameterName = "AsUnlit", },
                    new RadialPuppet(){ ParameterName = "VertexLightStrength", },
                }
            }
            .ToMenuItem(root);

            new SubMenu()
            {
                Name = "Backlight",
                Children = new MenuTree[]
                {
                    new Toggle(){ ParameterName = "UseBacklight", Name = "Enable" },
                    new SubMenu() { Name = "Color", Children = new[]
                    {
                        new RadialPuppet(){ ParameterName = "BacklightColor/R", Name = "R", },
                        new RadialPuppet(){ ParameterName = "BacklightColor/G", Name = "G", },
                        new RadialPuppet(){ ParameterName = "BacklightColor/B", Name = "B", },
                        new RadialPuppet(){ ParameterName = "BacklightColor/A", Name = "A", },
                    }},
                    new RadialPuppet(){ ParameterName = "BacklightMainStrength",   Name = "MainStrength"},
                          new Toggle(){ ParameterName = "BacklightReceiveShadow",  Name = "ReceiveShadow"},
                          new Toggle(){ ParameterName = "BacklightBackfaceMask",   Name = "BackfaceMask"},
                    new RadialPuppet(){ ParameterName = "BacklightNormalStrength", Name = "NormalStrength"},
                    new RadialPuppet(){ ParameterName = "BacklightBorder",         Name = "Border"},
                    new RadialPuppet(){ ParameterName = "BacklightBlur",           Name = "Blur"},
                    new RadialPuppet(){ ParameterName = "BacklightDirectivity",    Name = "Directivity"},
                    new RadialPuppet(){ ParameterName = "BacklightViewStrength",   Name = "ViewStrength"},
                }
            }
            .ToMenuItem(root);

            new SubMenu()
            {
                Name = "Distance Fade",
                Children = new MenuTree[]
                {
                    new RadialPuppet(){ ParameterName = "DistanceFadeStart", Name = "Start", },
                    new RadialPuppet(){ ParameterName = "DistanceFadeEnd", Name = "End ", },
                    new RadialPuppet(){ ParameterName = "DistanceFadeStrength", Name = "Strength", },
                    new Toggle(){ ParameterName = "DistanceFadeBackfaceForceShadow", Name = "BackfaceForceShadow", },
                }
            }
            .ToMenuItem(root);

            foreach(var x in root.gameObject.GetComponentsInChildren<ModularAvatarMenuItem>())
            {
                if (x.Control.type == ExpressionMenuItemType.RadialPuppet)
                {
                    x.Control.parameter = new() { name = "ControledParameterIndex" };
                    x.Control.value = parameterIDTable[x.Control.subParameters[0].name];
                }
            }
        }

        var mapa = go.GetOrAddComponent<ModularAvatarParameters>();
        mapa.parameters = parameters;

        var mama_Wd = go.AddComponent<ModularAvatarMergeAnimator>();
        mama_Wd.animator = fx;
        mama_Wd.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        mama_Wd.matchAvatarWriteDefaults = false;
        mama_Wd.pathMode = MergeAnimatorPathMode.Absolute;

        var mama = go.AddComponent<ModularAvatarMergeAnimator>();
        mama.animator = parameterSync;
        mama.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        mama.matchAvatarWriteDefaults = true;
        mama.pathMode = MergeAnimatorPathMode.Absolute;

    }

    private static void SetMaterialParameters(Material[] materials, LilToonParameters parameters)
    {
        var dict = typeof(LilToonParameters).GetFields().Where(x => x.FieldType == typeof(float) || x.FieldType == typeof(bool)).ToDictionary(x => $"_{x.Name}", x => x.GetValue(parameters) switch
        {
            float value => value,
            bool value => value ? 1f : 0f,
            _ => 0f,
        });

        using var so = new SerializedObject(materials);
        so.maxArraySizeForMultiEditing = 512;
        using var savedProperties = so.FindProperty("m_SavedProperties");
        using var floats = savedProperties.FindPropertyRelative("m_Floats");
        using var colors = savedProperties.FindPropertyRelative("m_Colors");

        int remain = dict.Count;
        floats.NextVisible(true);
        do
        {
            if (floats.name == "data")
            {
                floats.NextVisible(true);
                var key = floats.stringValue;
                if (dict.TryGetValue(key, out var value))
                {
                    floats.NextVisible(false);
                    floats.floatValue = value;
                    remain--;
                }
                else
                {
                    floats.NextVisible(false);
                }
            }
        }
        while (remain > 0 && floats.NextVisible(false));

        colors.NextVisible(true);
        do
        {
            if (colors.name == "data")
            {
                colors.NextVisible(true);
                var key = colors.stringValue;
                if (key == "_BacklightColor")
                {
                    colors.NextVisible(false);
                    colors.colorValue = parameters.BacklightColor;
                    break;
                }
                else
                {
                    colors.NextVisible(false);
                }
            }
        }
        while (colors.NextVisible(false));

        so.ApplyModifiedProperties();
    }

    private static float ClampParameterValue(float value, float min, float max) => (Mathf.Clamp(value, min, max) - min) / max;

    private abstract class MenuTree
    {
        public virtual string Name { get; set; }

        public MenuTree[] Children { get; set; }

        public ModularAvatarMenuItem ToMenuItem(ModularAvatarMenuItem parent = null)
        {
            var go = new GameObject(Name);
            if (parent != null)
            {
                go.transform.parent = parent.transform;
            }

            var item = go.AddComponent<ModularAvatarMenuItem>();
            OnCreateMenuItem(item);

            if ((Children?.Length ?? 0) > 0)
            foreach (var x in Children)
                x.ToMenuItem(item);

            return item;
        }

        protected abstract void OnCreateMenuItem(ModularAvatarMenuItem item);
    }

    private sealed class RadialPuppet : MenuTree
    {
        public override string Name { get => base.Name ?? ParameterName; set => base.Name = value; }

        public string ParameterName { get; set; }

        protected override void OnCreateMenuItem(ModularAvatarMenuItem item)
        {
            item.Control.type = ExpressionMenuItemType.RadialPuppet;
            item.Control.subParameters = new VRCExpressionsMenu.Control.Parameter[] { new() { name = ParameterName } };
        }
    }

    private sealed class Toggle : MenuTree
    {
        public override string Name { get => base.Name ?? ParameterName; set => base.Name = value; }

        public string ParameterName { get; set; }

        protected override void OnCreateMenuItem(ModularAvatarMenuItem item)
        {
            item.Control.type = ExpressionMenuItemType.Toggle;
            item.Control.parameter = new() { name = ParameterName };
        }
    }

    private sealed class SubMenu : MenuTree
    {
        protected override void OnCreateMenuItem(ModularAvatarMenuItem item)
        {
            item.Control.type = ExpressionMenuItemType.SubMenu;
            item.MenuSource = SubmenuSource.Children;
        }
    }
}