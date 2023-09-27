using System;
using UnityEngine;

namespace gomoru.su.LightController
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class GroupMasterAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ConditionParameterAttribute : Attribute
    {
        public string Name { get; }

        public ConditionParameterAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class LimitParameterAttribute : Attribute
    {
        public string Name { get; }

        public LimitParameterAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ToggleAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NameAttribute : Attribute
    {
        public string Name { get; set; }
        public string MenuName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class GroupAttribute : PropertyAttribute
    {
        public string Group { get; set; }

        public GroupAttribute(string group)
        {
            Group = group;
            order = int.MinValue;
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

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class InternalPropertyAttribute : Attribute { }
}
