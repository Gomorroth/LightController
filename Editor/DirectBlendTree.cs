using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Animations;
using Object = UnityEngine.Object;
using UnityEditor;

namespace gomoru.su
{
    public sealed partial class DirectBlendTree : DirectBlendTree.IDirectBlendTreeItem
    {
        public static AnimatorControllerParameter DefaultDirectBlendTreeParameter => new AnimatorControllerParameter() { name  = "1", type = AnimatorControllerParameterType.Float, defaultFloat = 1 };

        private List<IDirectBlendTreeItem> _items;
        private Object _assetContainer;

        public string Name { get; set; }
        public string ParameterName { get; }

        public AnimatorControllerParameter DirectBlendParameter { get; }

        public DirectBlendTree(Object assetContainer, string parameterName = "1")
        {
            _items = new List<IDirectBlendTreeItem>();
            _assetContainer = assetContainer;
            ParameterName = parameterName;
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
            var item = new DirectBlendTree(_assetContainer, ParameterName);
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
                children[i].directBlendParameter = ParameterName;
            }
            blendTree.children = children;

            return blendTree;
        }

        public AnimatorControllerLayer ToAnimatorControllerLayer()
        {
            var layer = new AnimatorControllerLayer();
            layer.name = Name;
            var stateMachine = layer.stateMachine = new AnimatorStateMachine();
            AssetDatabase.AddObjectToAsset(stateMachine, _assetContainer);

            var state = stateMachine.AddState(Name ?? "Direct Blend Tree");
            state.motion = ToBlendTree();

            return layer;
        }

        public string[] GetParameters()
        {
            var list = new List<string>();
            (this as IDirectBlendTreeItem).GetParameters(list);
            return list.ToArray();
        }

        void IDirectBlendTreeItem.Apply(BlendTree destination)
        {
            var blendTree = ToBlendTree();
            destination.AddChild(blendTree);
        }

        void IDirectBlendTreeItem.GetParameters(List<string> destination)
        {
            destination.Add(ParameterName);
            foreach (var item in _items)
            {
                item.GetParameters(destination);
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
        private interface IDirectBlendTreeItem
        {
            void Apply(BlendTree destination);
            void GetParameters(List<string> destination);
        }
    }

    partial class DirectBlendTree
    {
        public abstract class ControlBase : IDirectBlendTreeItem
        {
            public string Name;
            public string ParameterName;
            protected Object AssetContainer;

            public ControlBase(string parameterName, Object assetContainer)
            {
                ParameterName = parameterName;
                AssetContainer = assetContainer;
            }

            public abstract IEnumerable<(Motion Motion, float Weight)> GetMotions();

            void IDirectBlendTreeItem.Apply(BlendTree destination)
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

            void IDirectBlendTreeItem.GetParameters(List<string> destination)
            {
                destination.Add(ParameterName);
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
