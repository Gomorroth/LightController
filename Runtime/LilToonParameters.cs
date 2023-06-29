using System;
using UnityEngine;

namespace gomoru.su.LightController
{
    [Serializable]
    public sealed class LilToonParameters
    {
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

        public void SetValuesFromMaterial(Material material)
        {
            LightMinLimit = material.GetFloat($"_{nameof(LightMinLimit)}");
            LightMaxLimit = material.GetFloat($"_{nameof(LightMaxLimit)}");
            MonochromeLighting = material.GetFloat($"_{nameof(MonochromeLighting)}");
            ShadowEnvStrength = material.GetFloat($"_{nameof(ShadowEnvStrength)}");
            AsUnlit = material.GetFloat($"_{nameof(AsUnlit)}");
            VertexLightStrength = material.GetFloat($"_{nameof(VertexLightStrength)}");
        }
    }
}
