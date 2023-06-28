using System;
using UnityEngine;

namespace gomoru.su.LightController
{
    [Serializable]
    public struct LilToonLightParameters
    {
        [SerializeField, Range(0f, 1f)]
        public float LightMinLimit;

        [SerializeField, Range(0f, 10f)]
        public float LightMaxLimit;

        [SerializeField, Range(0f, 1f)]
        public float MonochromeLighting;

        [SerializeField, Range(0f, 1f)]
        public float ShadowEnvStrength;

        [SerializeField, Range(0f, 1f)]
        public float AsUnlit;

        [SerializeField, Range(0f, 1f)]
        public float VertexLightStrength;

        public static LilToonLightParameters Default => new LilToonLightParameters
        {
            LightMinLimit = 0.05f,
            LightMaxLimit = 1f,
            MonochromeLighting = 0f,
            ShadowEnvStrength = 0f,
            AsUnlit = 0f,
            VertexLightStrength = 0f,
        };
    }
}
