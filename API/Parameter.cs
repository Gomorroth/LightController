using System;

namespace gomoru.su.LightController.API
{
    [Serializable]
    public abstract class Parameter
    {
        public bool IsEnable = true; 
        public bool IsSync = true;
        public bool IsSave = true;

        private protected Parameter() { }
    }


    [Serializable]
    public class Parameter<T> : Parameter
    {
        public T Value;

        private protected Parameter() { }

        private Parameter(T value)
        {
            Value = value;
        }

        public static implicit operator Parameter<T>(T value) => new Parameter<T>(value);
    }

    [Serializable]
    public sealed class FloatParameter : Parameter<float>
    {
        public FloatParameter(float value)
        {
            Value = value;
        }

        public static implicit operator FloatParameter(float value) => new FloatParameter(value);
    }


    [Serializable]
    public sealed class BoolParameter : Parameter<bool>
    {
        public BoolParameter(bool value)
        {
            Value = value;
        }

        public static implicit operator BoolParameter(bool value) => new BoolParameter(value);
    }
}
