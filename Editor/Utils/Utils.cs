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
        public static T AddTo<T>(this T item, ObjectContainer cache, bool forceFlush = false) where T : Object
        {
            cache.Add(item, forceFlush);
            return item;
        }

        public static IEnumerable<GameObject> EnumerateUnderlyingObjects(this GameObject gameObject) => gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject);

        public static T GetOrAddComponent<T>(this GameObject gameObject, Action<T> action) where T : Component
        {
            if (!(gameObject.GetComponent<T>() is T component))
            {
                component = gameObject.AddComponent<T>();
            }
            action(component);
            return component;
        }
    }
}