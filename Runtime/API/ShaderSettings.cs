using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using VRC.SDKBase;
using System.Reflection;
using System.Linq;

[assembly:InternalsVisibleTo("gomoru.su.light-controller.editor")]

namespace gomoru.su.LightController.API
{
    public abstract partial class ShaderSettings : MonoBehaviour, IEditorOnly
    {
        public virtual string DisplayName => GetType().Name;

        protected virtual void OnEnable() { }

        public abstract bool IsTargetMaterial(Material material);

        internal virtual Parameter[] EnumerateParameters()
        {
            if (_InternalEnumerateParameters == null)
            {
                _InternalEnumerateParameters = CreateEnumerateParameters(this.GetType());
            }
            return _InternalEnumerateParameters();
        }

        private static Func<Parameter[]> CreateEnumerateParameters(Type type)
        {
            var method = new DynamicMethod("", typeof(Parameter[]), Type.EmptyTypes);
            var il = method.GetILGenerator();

            // TODO

            return method.CreateDelegate(typeof(Func<Parameter[]>)) as Func<Parameter[]>;
        }

        private static readonly OpCode[] LdcI4s = new[] { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, };
        private static void LdcI4(ILGenerator il, int value)
        {
            if ((uint)value < LdcI4s.Length)
            {
                il.Emit(LdcI4s[value]);
            }
            else if ((uint)value <= byte.MaxValue)
            {
                il.Emit(OpCodes.Ldc_I4_S, value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }

        private Func<Parameter[]> _InternalEnumerateParameters;
    }
}
