using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Animations;
using Object = UnityEngine.Object;
using UnityEditor;

namespace gomoru.su
{
    public abstract class DirectBlendTreeItem
    {
        public abstract void Apply(BlendTree destination);
        public abstract void AddParameterToController(AnimatorController destination);

        protected static bool TryAddParameter(AnimatorController controller, string name, float value)
        {
            if (controller.parameters.Any(p => p.name == name))
                return false;

            controller.AddParameter(new AnimatorControllerParameter() { defaultFloat = value });
            return true;
        }
    }

    public sealed partial class DirectBlendTree : DirectBlendTreeItem
    {
        private readonly string DirectBlendParameterName;
        private List<DirectBlendTreeItem> _items;
        private Object _assetContainer;

        public DirectBlendTree(Object assetContainer, string directBlendParameterName = "1")
        {
            _items = new List<DirectBlendTreeItem>();
            _assetContainer = assetContainer;
            DirectBlendParameterName = directBlendParameterName;
        }

        public Toggle AddToggle() => new Toggle(_assetContainer);

        public MotionTimeToggle AddMotionTimeToggle() => new MotionTimeToggle(_assetContainer);

        public RadialPuppet AddRadialPuppet() => new RadialPuppet(_assetContainer);

        public DirectBlendTree AddDirectBlendTree() => new DirectBlendTree(_assetContainer, DirectBlendParameterName);

        public BlendTree ToBlendTree()
        {
            var blendTree = new BlendTree();
            blendTree.blendType = BlendTreeType.Direct;
            SetNormalizedBlendValues(blendTree, false);
            foreach (var item in _items)
            {
                item.Apply(blendTree);
            }

            var children = blendTree.children;
            for(int i = 0; i < children.Length; i++)
            {
                children[i].directBlendParameter = DirectBlendParameterName;
            }
            blendTree.children = children;

            return blendTree;
        }

        public AnimatorControllerLayer Apply(AnimatorController destination)
        {
            var layer = new AnimatorControllerLayer();
            var stateMachine = layer.stateMachine = new AnimatorStateMachine();
            AssetDatabase.AddObjectToAsset(stateMachine, destination);

            var state = stateMachine.AddState("Direct Blend Tree");
            state.motion = ToBlendTree();

            return layer;
        }

        public override void Apply(BlendTree destination)
        {
            var blendTree = ToBlendTree();
            destination.AddChild(blendTree);
        }

        public override void AddParameterToController(AnimatorController destination)
        {
            foreach (var item in _items)
            {
                item.AddParameterToController(destination);
            }

            TryAddParameter(destination, DirectBlendParameterName, 1);
        }

        private static void SetNormalizedBlendValues(BlendTree blendTree, bool value)
        {
            using (var so = new SerializedObject(blendTree))
            {
                so.FindProperty("m_NormalizedBlendValues").boolValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    partial class DirectBlendTree
    {
        public abstract class ControlBase : DirectBlendTreeItem
        {
            private static readonly ObjectReferenceKeyframe[] _singleKeyFrame = new ObjectReferenceKeyframe[1];

            public string ParameterName;
            protected Object AssetContainer;

            public ControlBase(Object assetContainer)
            {
                AssetContainer = assetContainer;
            }

            public abstract IEnumerable<(AnimationClip Motion, float Weight)> GetMotions();

            public override void Apply(BlendTree destination)
            {
                var blendTree = new BlendTree();
                foreach (var (motion, weight) in GetMotions().OrderBy(x => x.Weight))
                {
                    blendTree.AddChild(motion, weight);
                }

                blendTree.blendParameter = ParameterName;

                destination.AddChild(blendTree);
            }

            public override void AddParameterToController(AnimatorController destination)
            {
                TryAddParameter(destination, ParameterName, 0);
            }

            protected void SeparateAnimationClips(AnimationClip clip, Dictionary<float, AnimationClip> destination)
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);

                foreach (var binding in bindings)
                {
                    // Editor Curve
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        foreach (var key in curve.keys)
                        {
                            var time = key.time;
                            var motion = GetOrAddSeparetedClip(time);

                            var singleCurve = AnimationCurve.Constant(time, time, key.value);
                            AnimationUtility.SetEditorCurve(motion, binding, singleCurve);
                        }
                    }

                    // Object Reference
                    {
                        foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                        {
                            var time = key.time;
                            var motion = GetOrAddSeparetedClip(time);

                            var copiedKey = key;
                            copiedKey.time = 0;
                            _singleKeyFrame[0] = copiedKey;
                            AnimationUtility.SetObjectReferenceCurve(motion, binding, _singleKeyFrame);
                        }
                    }
                }


                AnimationClip GetOrAddSeparetedClip(float time)
                {
                    if (!destination.TryGetValue(time, out var motion))
                    {
                        motion = new AnimationClip() { name = $"{clip.name}.{time}" };
                        AssetDatabase.AddObjectToAsset(motion, AssetContainer);
                        destination.Add(time, motion);
                    }
                    return motion;
                }
            }
        }

        public abstract class MotionSeparatingControlBase : ControlBase
        {
            public AnimationClip Motion;
            private Dictionary<float, AnimationClip> _separatedClips = new Dictionary<float, AnimationClip>();

            public MotionSeparatingControlBase(Object assetContainer) : base(assetContainer)
            { }

            public override IEnumerable<(AnimationClip Motion, float Weight)> GetMotions()
            {
                var separatedClips = _separatedClips;
                separatedClips.Clear();
                SeparateAnimationClips(Motion, separatedClips);

                var endTime = separatedClips.Max(x => x.Key);

                return separatedClips.Select(x => (x.Value, x.Key / endTime));
            }
        }
    }

    partial class DirectBlendTree
    {
        public sealed class Toggle : ControlBase
        {
            public AnimationClip ON;
            public AnimationClip OFF;
            public Toggle(Object assetContainer) : base(assetContainer)
            { }

            public override IEnumerable<(AnimationClip Motion, float Weight)> GetMotions()
            {
                yield return (OFF, 0);
                yield return (OFF, 1);
            }
        }
    }

    partial class DirectBlendTree
    {
        public sealed class MotionTimeToggle : MotionSeparatingControlBase
        {
            public MotionTimeToggle(Object assetContainer) : base(assetContainer)
            { }
        }
    }

    partial class DirectBlendTree
    {
        public class RadialPuppet : MotionSeparatingControlBase
        {
            public RadialPuppet(Object assetContainer) : base(assetContainer)
            { }
        }
    }
}
