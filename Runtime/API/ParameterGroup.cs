using System;

namespace gomoru.su.LightController.API
{
    [Serializable]
    public abstract class ParameterGroup
    {
        public virtual bool UseGroupNameAsPrefix => false;
        public virtual ReadOnlySpan<char> Name
        {
            get
            {
                var name = GetType().Name.AsSpan();
                if (name.EndsWith("Group".AsSpan()))
                {
                    name = name.Slice(0, name.Length - "Group".Length);
                }
                return name;
            }
        }
    }

}
