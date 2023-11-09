using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace gomoru.su.LightController
{
    internal static partial class Utils
    {
        public static T AddTo<T>(this T item, ObjectCache cache) where T : Object
        {
            cache.Add(item);
            return item;
        }

        public static IEnumerable<GameObject> EnumerateUnderlyingObjects(this GameObject gameObject) => gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject);
    }
}