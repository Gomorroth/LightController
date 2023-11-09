using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gomoru.su.LightController
{
    partial class Utils
    {
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            var dummy = Unsafe.As<DummyList<T>>(list);
            return dummy.Items.AsSpan(0, dummy.Count);
        }

        private sealed class DummyList<T>
        {
            public T[] Items;
            public int Count;
        }
    }
}
