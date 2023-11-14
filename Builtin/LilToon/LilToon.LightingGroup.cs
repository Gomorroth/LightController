using gomoru.su.LightController.API;
using System;
using UnityEngine;

partial class LilToon
{
    [Serializable]
    public sealed class LightingGroup : ParameterGroup
    {
        public override string Name => "Lighting";

        [Range(0f, 1f)]
        [Tooltip("ates")]
        public FloatParameter LightMinLimit = 0.05f;

        [Range(0, 10f)]
        public FloatParameter LightMaxLimit = 1f;

        [Range(0f, 1f)]
        public FloatParameter MonochromeLighting = 0f;

        [Range(0f, 1f)]
        public FloatParameter ShadowEnvStrength = 0f;

        [Range(0f, 1f)]
        public FloatParameter AsUnlit = 0f;

        [Range(0f, 1f)]
        public FloatParameter VertexLightStrength = 0f;
    }
}