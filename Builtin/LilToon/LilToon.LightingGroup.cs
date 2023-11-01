using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using System;
using UnityEngine;

partial class LilToon
{
    [Serializable]
    public sealed class LightingGroup : ParameterGroup
    {
        [Range(0f, 1f)]
        public FloatParameter LightMinLimit = 0.05f;

        [Range(0f, 10f)]
        [MinMax(0, nameof(LightMaxLimitMax))]
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