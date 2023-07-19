using System;

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
    public sealed class ToggleAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NameAttribute : Attribute
    {
        public string Name { get; set; }
        public string MenuName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class GroupAttribute : Attribute
    {
        public string Group { get; set; }
        public GroupAttribute(string group)
        {
            Group = group;
        }
    }
}
