﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace gomoru.su.LightController
{
    [Serializable]
    public sealed class LilToonParameters
    {
        public const string GroupName_Lighting = "Lighting";
        public const string GroupName_Backlight = "Backlight";
        public const string GroupName_DistanceFade = "DistanceFade";

        [HideInInspector, SerializeField]
        [Toggle, Name(MenuName = "Enable")]
        [GroupMaster, Group(GroupName_Lighting)]
        public bool UseLighting = false;

        [Header("Lighting")]
        [SerializeField, Range(0f, 1f)]
        [Name(MenuName = "Min")]
        [Group(GroupName_Lighting)]
        public float LightMinLimit = 0.05f;

        [SerializeField, Range(0f, 10f)]
        [Name(MenuName = "Max")]
        [Group(GroupName_Lighting)]
        public float LightMaxLimit = 1f;

        [SerializeField, Range(0f, 1f)]
        [Name(MenuName = "Monochrome")]
        [Group(GroupName_Lighting)]
        public float MonochromeLighting = 0f;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Lighting)]
        public float ShadowEnvStrength = 0f;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Lighting)]
        public float AsUnlit = 0f;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Lighting)]
        public float VertexLightStrength = 0f;


        [SerializeField, Header("Backlight")]
        [Toggle, Name(MenuName = "Enable")]
        [GroupMaster, Group(GroupName_Backlight)]
        public bool UseBacklight = false;

        [SerializeField]
        [Group(GroupName_Backlight), Name(MenuName = "Strength")]
        public Color BacklightColor = new Color(0.85f, 0.8f, 0.7f, 1.0f);

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Backlight)]
        public float BacklightMainStrength = 0;

        [SerializeField]
        [Group(GroupName_Backlight), Toggle]
        public bool BacklightReceiveShadow = true;

        [SerializeField]
        [Group(GroupName_Backlight), Toggle]
        public bool BacklightBackfaceMask = true;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Backlight)]
        public float BacklightNormalStrength = 1;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Backlight)]
        public float BacklightBorder = 0.35f;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Backlight)]
        public float BacklightBlur = 0.05f;

        [SerializeField, Range(0f, 20f)]
        [Group(GroupName_Backlight)]
        public float BacklightDirectivity = 5;

        [SerializeField, Range(0f, 1f)]
        [Group(GroupName_Backlight)]
        public float BacklightViewStrength = 1;


        [SerializeField, Header("DistanceFade")]
        [Toggle, Name(MenuName = "Enable")]
        [GroupMaster, Group(GroupName_DistanceFade)]
        public bool UseDistanceFade = false;

        [HideInInspector]
        [Group(GroupName_DistanceFade)]
        public Vector4 DistanceFade = new Vector4(0.1f, 0.01f, 0, 0);

        [SerializeField, Range(0, 1)]
        [VectorProxy(nameof(DistanceFade), 0)]
        [Group(GroupName_DistanceFade)]
        public float DistanceFadeStart = 0.1f;

        [SerializeField, Range(0, 1)]
        [VectorProxy(nameof(DistanceFade), 1)]
        [Group(GroupName_DistanceFade)]
        public float DistanceFadeEnd = 0.01f;

        [SerializeField, Range(0, 1)]
        [VectorProxy(nameof(DistanceFade), 2)]
        [Group(GroupName_DistanceFade)]
        public float DistanceFadeStrength = 0;

        [SerializeField, Toggle]
        [VectorProxy(nameof(DistanceFade), 3)]
        [Group(GroupName_DistanceFade)]
        public bool DistanceFadeBackfaceForceShadow = false;


        public void SetValuesFromMaterial(Material material)
        {
            LightMinLimit = material.GetFloat($"_{nameof(LightMinLimit)}");
            LightMaxLimit = material.GetFloat($"_{nameof(LightMaxLimit)}");
            MonochromeLighting = material.GetFloat($"_{nameof(MonochromeLighting)}");
            ShadowEnvStrength = material.GetFloat($"_{nameof(ShadowEnvStrength)}");
            AsUnlit = material.GetFloat($"_{nameof(AsUnlit)}");
            VertexLightStrength = material.GetFloat($"_{nameof(VertexLightStrength)}");

            UseBacklight = material.GetInt($"_{nameof(UseBacklight)}") != 0;
            BacklightColor = material.GetColor($"_{nameof(BacklightColor)}");
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
