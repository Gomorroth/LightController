using System;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace gomoru.su.LightController
{
    [Serializable]
    public abstract class Parameter
    {
        public bool Enable;
        public bool Sync;
        public bool Save;
    }

    [Serializable]
    public abstract class Parameter<T> : Parameter
    {
        public T Value;

        protected Parameter(T value, bool enable = true, bool sync = true, bool save = true)
        {
            Value = value;
            Enable = enable;
            Sync = sync;
            Save = save;
        }
    }

    [Serializable]
    public sealed class FloatParameter : Parameter<float>
    {
        public FloatParameter(float value) : base(value)
        { }

        public static implicit operator FloatParameter(float value) => new FloatParameter(value);
    }

    [Serializable]
    public sealed class BoolParameter : Parameter<bool>
    {
        public BoolParameter(bool value) : base(value)
        { }

        public static implicit operator BoolParameter(bool value) => new BoolParameter(value);
    }


    [Serializable]
    public sealed class ColorParameter : Parameter<Color>
    {
        public ColorParameter(Color value) : base(value)
        { }

        public static implicit operator ColorParameter(Color value) => new ColorParameter(value);
    }
}
