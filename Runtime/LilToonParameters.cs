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
        [SerializeField, Range(0f, 1f)]
        public float LightMinLimit = 0.05f;

        [SerializeField, Range(0f, 10f)]
        public float LightMaxLimit = 1f;

        [SerializeField, Range(0f, 1f)]
        public float MonochromeLighting = 0f;

        [SerializeField, Range(0f, 1f)]
        public float ShadowEnvStrength = 0f;

        [SerializeField, Range(0f, 1f)]
        public float AsUnlit = 0f;

        [SerializeField, Range(0f, 1f)]
        public float VertexLightStrength = 0f;

        [SerializeField]
        public bool UseBacklight = false;

        [SerializeField, Range(0f, 1f)]
        public float BacklightStrength = 1;

        [SerializeField, Range(0f, 1f)]
        public float BacklightMainStrength = 0;

        [SerializeField]
        public bool BacklightReceiveShadow = true;

        [SerializeField]
        public bool BacklightBackfaceMask = true;

        [SerializeField, Range(0f, 1f)]
        public float BacklightNormalStrength = 1;

        [SerializeField, Range(0f, 1f)]
        public float BacklightBorder = 0.35f;

        [SerializeField, Range(0f, 1f)]
        public float BacklightBlur = 0.05f;

        [SerializeField, Range(0f, 20f)]
        public float BacklightDirectivity = 5;

        [SerializeField, Range(0f, 1f)]
        public float BacklightViewStrength = 1;


        public void SetValuesFromMaterial(Material material)
        {
            LightMinLimit = material.GetFloat($"_{nameof(LightMinLimit)}");
            LightMaxLimit = material.GetFloat($"_{nameof(LightMaxLimit)}");
            MonochromeLighting = material.GetFloat($"_{nameof(MonochromeLighting)}");
            ShadowEnvStrength = material.GetFloat($"_{nameof(ShadowEnvStrength)}");
            AsUnlit = material.GetFloat($"_{nameof(AsUnlit)}");
            VertexLightStrength = material.GetFloat($"_{nameof(VertexLightStrength)}");

            UseBacklight = material.GetInt($"_{nameof(UseBacklight)}") != 0;
            BacklightStrength = material.GetColor($"_BacklightColor").a;
            BacklightMainStrength = material.GetFloat($"_{nameof(BacklightMainStrength)}");
            BacklightReceiveShadow = material.GetInt($"_{nameof(BacklightReceiveShadow)}") != 0;
            BacklightBackfaceMask = material.GetInt($"_{nameof(BacklightBackfaceMask)}") != 0;
            BacklightNormalStrength = material.GetFloat($"_{nameof(BacklightNormalStrength)}");
            BacklightBorder = material.GetFloat($"_{nameof(BacklightBorder)}");
            BacklightBlur = material.GetFloat($"_{nameof(BacklightBlur)}");
            BacklightDirectivity = material.GetFloat($"_{nameof(BacklightDirectivity)}");
            BacklightViewStrength = material.GetFloat($"_{nameof(BacklightViewStrength)}");
        }
    }
}
