using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using System;
using UnityEngine;

partial class LilToon
{
    [Serializable]
    public sealed class BacklightGroup : ParameterGroup
    {
        public override bool UseGroupNameAsPrefix => true;

        public override string Name => "Backlight";

        public ColorParameter Color = new Color(0.85f, 0.8f, 0.7f, 1.0f);

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