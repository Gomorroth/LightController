using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace gomoru.su.LightController
{
    [Serializable]
    public sealed class LilToonParameters
    {
        [Header("Lighting")]

        [Range(0f, 1f)]
        public float LightMinLimit = 0.05f;
        [Range(0f, 1f)]
        public float LightMinLimitMin = 0;
        [Range(0f, 1f)]
        public float LightMinLimitMax = 1;

        [Range(0f, 10f)]
        public float LightMaxLimit = 1f;
        [Range(0f, 10f)]
        public float LightMaxLimitMin = 0;
        [Range(0f, 10f)]
        public float LightMaxLimitMax = 1;

        [Range(0f, 1f)]
        public float MonochromeLighting = 0f;

        [Range(0f, 1f)]
        public float ShadowEnvStrength = 0f;

        [Range(0f, 1f)]
        public float AsUnlit = 0f;

        [Range(0f, 1f)]
        public float VertexLightStrength = 0f;

        [Header("Backlight")]

        public bool UseBacklight = false;

        public Color BacklightColor = new Color(0.85f, 0.8f, 0.7f, 1.0f);

        [Range(0f, 1f)]
        public float BacklightMainStrength = 0;

        public bool BacklightReceiveShadow = true;

        public bool BacklightBackfaceMask = true;

        [Range(0f, 1f)]
        public float BacklightNormalStrength = 1;

        [Range(0f, 1f)]
        public float BacklightBorder = 0.35f;

        [Range(0f, 1f)]
        public float BacklightBlur = 0.05f;

        [Range(0f, 20f)]
        public float BacklightDirectivity = 5;

        [Range(0f, 1f)]
        public float BacklightViewStrength = 1;


        [Header("Distance Fade")]

        [Range(0, 1)]
        public float DistanceFadeStart = 0.1f;

        [Range(0, 1)]
        public float DistanceFadeEnd = 0.01f;

        [Range(0, 1)]
        public float DistanceFadeStrength = 0;

        public bool DistanceFadeBackfaceForceShadow = false;
    }
}
