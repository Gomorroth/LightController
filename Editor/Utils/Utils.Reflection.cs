using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace gomoru.su.LightController
{
    partial class Utils
    {
        private static readonly OpCode[] LdcI4s = new[] { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, };
        public static void LdcI4(this ILGenerator il, int value)
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
    }
}
