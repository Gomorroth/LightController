using UnityEngine;

namespace gomoru.su.LightController
{
    internal static class AnimationUtils
    {
        private static readonly AnimationCurve _singleton = new AnimationCurve();
        private static readonly Keyframe[] _keyframes1 = new Keyframe[1];
        private static readonly Keyframe[] _keyframes2 = new Keyframe[2];

        public static AnimationCurve Constant(float value)
        {
            var curve = _singleton;
            var keys = _keyframes1;
            _ = keys.Length;
            keys[0] = new Keyframe(0, value);
            curve.keys = keys;
            return curve;
        }

        public static AnimationCurve Linear(float start, float end)
        {
            var curve = _singleton;
            var keys = _keyframes2;
            _ = keys.Length;
            float num = (end - start) / (1 / 60f);
            keys[0] = new Keyframe(0, start, 0, num);
            keys[1] = new Keyframe(1/60f, end, num, 0);
            curve.keys = keys;
            return curve;
        }
    }
}
