using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using gomoru.su.LightController.API;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static VRC.SDKBase.VRCPlayerApi;
using Object = UnityEngine.Object;

namespace gomoru.su.LightController
{
    internal sealed partial class LightControllerCore
    {
        [MenuItem("Test/LightController")]
        public static void Test()
        {
            Run(Object.FindObjectOfType<LightController>(), null);
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

            var relation = new AnimationRelatedObjectManager(animations);

            using (var cache = new ObjectCache(assetContainer))
            {
                foreach (var shaderSetting in shaderSettings)
                {
                    foreach (var renderer in targetRenderers)
                    {
                        var materials = renderer.sharedMaterials;
                        bool flag = false;
                        foreach(var material in materials)
                        {
                            flag = shaderSetting.IsTargetMaterial(material);
                            if (flag)
                                break;
                        }
                        if (!flag)
                            continue;


                    }
                }
            }
        }
    }

    internal sealed class AnimationRelatedObjectManager
    {
        private readonly ImmutableDictionary<GameObject, Object[]> _dict;
        public AnimationRelatedObjectManager(IEnumerable<(GameObject Root, AnimationClip Motion)> animationClips)
        {
            _dict = animationClips
                .SelectMany(x => 
                    AnimationUtility.GetObjectReferenceCurveBindings(x.Motion)
                    .Select(binding => AnimationUtility.GetObjectReferenceValue(x.Root, binding, out var data) ? (x.Root.transform.Find(binding.path)?.gameObject, data) : default).Where(y => y.data != null))
                .GroupBy(x => x.gameObject, x => x.data)
                .ToImmutableDictionary(x => x.Key, x => x.ToArray());
        }
        public bool TryGetRelatedObjects(GameObject gameObject, out ReadOnlySpan<Object> result)
        {
            if (_dict.TryGetValue(gameObject, out var array))
            {
                result = array;
                return true;
            }
            result = default;
            return false;
        }
    }
}