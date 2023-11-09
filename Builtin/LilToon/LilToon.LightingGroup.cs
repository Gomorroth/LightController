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

    [Serializable]
    public sealed class BacklightGroup : ParameterGroup
    {
        public Color Color = new Color(0.85f, 0.8f, 0.7f, 1.0f);

        [Range(0f, 1f)]
        public FloatParameter MainStrength = 0;

        public BoolParameter ReceiveShadow = true;

        public BoolParameter BackfaceMask = true;

        [Range(0f, 1f)]
        public FloatParameter NormalStrength = 1;

        [Range(0f, 1f)]
        public FloatParameter Border = 0.35f;

        [Range(0f, 1f)]
        public FloatParameter Blur = 0.05f;

        [Range(0f, 20f)]
        public FloatParameter Directivity = 5;

        [Range(0f, 1f)]
        public FloatParameter ViewStrength = 1;
    }
}