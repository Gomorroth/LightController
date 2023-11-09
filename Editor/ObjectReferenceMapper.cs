using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;
using Factory = System.Func<UnityEngine.Component, UnityEngine.Object, UnityEngine.Object>;

namespace gomoru.su.LightController
{
    internal sealed class ObjectReferenceMapper
    {
        private readonly Dictionary<Type, List<object>> _factories = new Dictionary<Type, List<object>>();

        public ObjectReferenceMapper Register<T>(Func<Component, T, T> factory) where T : Object
        {
            if (!_factories.TryGetValue(typeof(T), out var list))
            {
                list = new List<object>();
                _factories[typeof(T)] = list;
            }
            list.Add(factory);
            return this;
        }

        public void Map(GameObject root)
        {
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                var so = new SerializedObject(component);
                bool enterChildren = true;
                var p = so.GetIterator();
                while (p.Next(enterChildren))
                {
                    if (p.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var value = p.objectReferenceValue;
                        if (value != null)
                        {
                            var type = value.GetType();
                            while (type != null)
                            {
                                if (_factories.TryGetValue(type, out var list))
                                {
                                    foreach (var factory in list)
                                    {
                                        value = Unsafe.As<Factory>(factory)(component, value);
                                        p.objectReferenceValue = value;
                                    }
                                    break;
                                }
                                type = type.BaseType;
                            }
                        }
                    }

                    switch (p.propertyType)
                    {
                        case SerializedPropertyType.String:
                        case SerializedPropertyType.Integer:
                        case SerializedPropertyType.Boolean:
                        case SerializedPropertyType.Float:
                        case SerializedPropertyType.Color:
                        case SerializedPropertyType.ObjectReference:
                        case SerializedPropertyType.LayerMask:
                        case SerializedPropertyType.Enum:
                        case SerializedPropertyType.Vector2:
                        case SerializedPropertyType.Vector3:
                        case SerializedPropertyType.Vector4:
                        case SerializedPropertyType.Rect:
                        case SerializedPropertyType.ArraySize:
                        case SerializedPropertyType.Character:
                        case SerializedPropertyType.AnimationCurve:
                        case SerializedPropertyType.Bounds:
                        case SerializedPropertyType.Gradient:
                        case SerializedPropertyType.Quaternion:
                        case SerializedPropertyType.FixedBufferSize:
                        case SerializedPropertyType.Vector2Int:
                        case SerializedPropertyType.Vector3Int:
                        case SerializedPropertyType.RectInt:
                        case SerializedPropertyType.BoundsInt:
                            enterChildren = false;
                            break;
                        case SerializedPropertyType.Generic:
                        case SerializedPropertyType.ExposedReference:
                        case SerializedPropertyType.ManagedReference:
                        default:
                            enterChildren = true;
                            break;
                    }
                    so.ApplyModifiedProperties();
                }
            }
        }
    }
}