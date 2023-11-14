using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace gomoru.su.LightController.API.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ParameterNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ParameterNameAttribute(string displayName)
        {
            Name = displayName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class VectorProxyAttribute : Attribute
    {
        public string TargetName { get; }
        public VectorField Field { get; }

        public VectorProxyAttribute(string targetName, VectorField field)
        {
            TargetName = targetName;
            Field = field;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class ColorControlAttribute : Attribute
    {
        public ColorControlAttribute(VectorField targetFields)
        {
            TargetFields = targetFields;
        }

        public VectorField TargetFields { get; }
    }

    [Flags]
    public enum VectorField
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        W = 1 << 3,

        R = 1 << 0,
        G = 1 << 1,
        B = 1 << 2,
        A = 1 << 3,

        XYZ = X | Y | Z,
        XYZW = XYZ | W,

        RGB = R | G | B,
        RGBA = RGB | A,
    }
}
