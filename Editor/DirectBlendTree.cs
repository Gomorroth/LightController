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
        public string Name;

        public abstract void Apply(BlendTree destination);
        public abstract void AddParameterToController(AnimatorController destination);

        protected static bool TryAddParameter(AnimatorController controller, string name, float value)
        {
            if (controller.parameters.Any(p => p.name == name))
                return false;

            controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = value });
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

        public Toggle AddToggle(string parameterName = null)
        {
            var item = new Toggle(parameterName, _assetContainer);
            _items.Add(item);
            return item;
        }

        public MotionTimeToggle AddMotionTimeToggle(string parameterName = null)
        {
            var item = new MotionTimeToggle(parameterName, _assetContainer);
            _items.Add(item);
            return item;
        }

        public RadialPuppet AddRadialPuppet(string parameterName = null)
        {
            var item = new RadialPuppet(parameterName, _assetContainer);
            _items.Add(item);
            return item;
        }

        public DirectBlendTree AddDirectBlendTree()
        {
            var item = new DirectBlendTree(_assetContainer, DirectBlendParameterName);
            _items.Add(item);
            return item;
        }

        public BlendTree ToBlendTree()
        {
            var blendTree = new BlendTree();
            blendTree.name = Name;
            AssetDatabase.AddObjectToAsset(blendTree, _assetContainer);
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
            destination.AddLayer(layer);

            var state = stateMachine.AddState("Direct Blend Tree");
            state.motion = ToBlendTree();

            AddParameterToController(destination);

            return layer;
        }

        public override void Apply(BlendTree destination)
        {
            var blendTree = ToBlendTree();
            destination.AddChild(blendTree);
        }

        public override void AddParameterToController(AnimatorController destination)
        {
            TryAddParameter(destination, DirectBlendParameterName, 1);
            foreach (var item in _items)
            {
                item.AddParameterToController(destination);
            }
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
            public string ParameterName;
            protected Object AssetContainer;

            public ControlBase(string parameterName, Object assetContainer)
            {
                ParameterName = parameterName;
                AssetContainer = assetContainer;
            }

            public abstract IEnumerable<(Motion Motion, float Weight)> GetMotions();

            public override void Apply(BlendTree destination)
            {
                var blendTree = new BlendTree();
                blendTree.name = Name;
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
        }

        public abstract class MotionSeparatingControlBase : ControlBase
        {
            private static readonly ObjectReferenceKeyframe[] _singleKeyFrame = new ObjectReferenceKeyframe[1];

            public AnimationClip Motion;
            private Dictionary<float, AnimationClip> _separatedClips = new Dictionary<float, AnimationClip>();

            public MotionSeparatingControlBase(string parameterName, Object assetContainer) : base(parameterName, assetContainer)
            { }

            public override IEnumerable<(Motion Motion, float Weight)> GetMotions()
            {
                var separatedClips = _separatedClips;
                separatedClips.Clear();
                SeparateAnimationClips(Motion, AssetContainer, separatedClips);

                var endTime = separatedClips.Max(x => x.Key);

                return separatedClips.Select(x => (x.Value as Motion, x.Key / endTime));
            }

            public static void SeparateAnimationClips(AnimationClip clip, Object assetContainer, Dictionary<float, AnimationClip> destination)
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);

                foreach (var binding in bindings)
                {
                    // Editor Curve
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve != null)
                    {
                        foreach (var key in curve.keys)
                        {
                            var time = key.time;
                            var motion = GetOrAddSeparetedClip(time);

                            var singleCurve = AnimationCurve.Constant(time, time, key.value);
                            AnimationUtility.SetEditorCurve(motion, binding, singleCurve);
                        }
                    }

                    // Object Reference
                    var objectReferences = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    if (objectReferences != null)
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
                        AssetDatabase.AddObjectToAsset(motion, assetContainer);
                        destination.Add(time, motion);
                    }
                    return motion;
                }
            }
        }
    }

    partial class DirectBlendTree
    {
        public sealed class Toggle : ControlBase
        {
            public Motion OFF;
            public Motion ON;

            public Toggle(string parameterName, Object assetContainer) : base(parameterName, assetContainer)
            { }

            public override IEnumerable<(Motion Motion, float Weight)> GetMotions()
            {
                yield return (OFF, 0);
                yield return (ON, 1);
            }
        }
    }

    partial class DirectBlendTree
    {
        public sealed class MotionTimeToggle : MotionSeparatingControlBase
        {
            public MotionTimeToggle(string parameterName, Object assetContainer) : base(parameterName, assetContainer)
            { }
        }
    }

    partial class DirectBlendTree
    {
        public class RadialPuppet : MotionSeparatingControlBase
        {
            public RadialPuppet(string parameterName, Object assetContainer) : base(parameterName, assetContainer)
            { }
        }
    }
}
