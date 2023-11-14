using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using System;
using UnityEngine;

partial class LilToon
{
    [Serializable]
    public sealed class DistanceFadeGroup : ParameterGroup
    {
        public override bool UseGroupNameAsPrefix => true;

        public override string Name => "DistanceFade";

        [NonSerialized]
        public Vector4 DistanceFade = new Vector4(0.1f, 0.01f, 0, 0);

        [Range(0, 1)]
        [VectorProxy(nameof(DistanceFade), VectorField.X)]
        public FloatParameter Start = 0.1f;

        [Range(0, 1)]
        [VectorProxy(nameof(DistanceFade), VectorField.Y)]
        public FloatParameter End = 0.01f;

        [Range(0, 1)]
        [VectorProxy(nameof(DistanceFade), VectorField.Z)]
        public FloatParameter Strength = 0;

        [VectorProxy(nameof(DistanceFade), VectorField.W)]
        public BoolParameter BackfaceForceShadow = false;
    }
}