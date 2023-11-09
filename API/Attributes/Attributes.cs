using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace gomoru.su.LightController.API.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxAttribute : PropertyAttribute
    {
        public MinMaxAttribute(float min, float max) 
        {
            Min = min;
            Max = max;
        }
        public MinMaxAttribute(float min, string max)
        {
            Min = min;
            MaxLimitter = max;
        }
        public MinMaxAttribute(string min, float max)
        {
            MinLimitter = min;
            Max = max;
        }
        public MinMaxAttribute(string min, string max)
        {
            MinLimitter = min;
            MaxLimitter = max;
        }

        public float Min { get; }
        public float Max { get; }
        public string MinLimitter { get; }
        public string MaxLimitter { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
    public sealed class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public DisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
    public sealed class QualifiedNameAttribute : Attribute
    {
        public string QualifiedName { get; set; }
        public QualifiedNameAttribute(string qualifiedName)
        {
            QualifiedName = qualifiedName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class VectorProxyAttribute : Attribute
    {
        public string TargetName { get; set; }
        public int Index { get; set; }

        public VectorProxyAttribute(string targetName, int index)
        {
            TargetName = targetName;
            Index = index;
        }
    }
}
