using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace gomoru.su.LightController
{
    internal static class Utils
    {
        private static MethodInfo _GetGeneratedAssetsFolder = typeof(nadena.dev.modular_avatar.core.editor.AvatarProcessor).Assembly.GetTypes().FirstOrDefault(x => x.Name == "Util")?.GetMethod(nameof(GetGeneratedAssetsFolder), BindingFlags.Static | BindingFlags.NonPublic);

        public static string GetGeneratedAssetsFolder()
        {
            var method = _GetGeneratedAssetsFolder;
            if (method != null)
                return method.Invoke(null, null) as string;

            var path = "Assets/_LightControllerTemporaryAssets";
            if (!AssetDatabase.IsValidFolder(path))
                path = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder("Assets/", "_LightControllerTemporaryAssets"));

            return path;
        }

        public static AnimatorController CreateTemporaryAsset()
        {
            var fx = new AnimatorController() { name = GUID.Generate().ToString() };
            AssetDatabase.CreateAsset(fx, System.IO.Path.Combine(Utils.GetGeneratedAssetsFolder(), $"{fx.name}.controller"));
            AssetDatabase.SaveAssets();
            return fx;
        }

        public static T GetOrAddComponent<T>(this GameObject obj, Action<T> action = null) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            action?.Invoke(component);
            return component;
        }

        public static T AddTo<T>(this T obj, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        public static T HideInHierarchy<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static string GetRelativePath(this Transform transform, Transform root, bool includeRelativeTo = false)
        {
            var buffer = _relativePathBuffer;
            if (buffer is null)
            {
                buffer = _relativePathBuffer = new string[128];
            }

            var t = transform;
            int idx = buffer.Length;
            while (t != null && t != root)
            {
                buffer[--idx] = t.name;
                t = t.parent;
            }
            if (includeRelativeTo && t != null && t == root)
            {
                buffer[--idx] = t.name;
            }

            return string.Join("/", buffer, idx, buffer.Length - idx);
        }

        private static string[] _relativePathBuffer;

        public static IEnumerable<string> EnumeratePropertyNames(this Shader shader) => Enumerable.Range(0, shader.GetPropertyCount()).Select(shader.GetPropertyName);

        private static AnimatorCondition[] _conditions1 = new AnimatorCondition[1];
        private static AnimatorCondition[] _conditions2 = new AnimatorCondition[2];

        public static AnimatorStateTransition AddTransition(this AnimatorState state, AnimatorState destination, AnimatorCondition condition)
        {
            _conditions1[0] = condition;
            return state.AddTransition(destination, _conditions1);
        }

        public static AnimatorStateTransition AddTransition(this AnimatorState state, AnimatorState destination, AnimatorCondition condition1, AnimatorCondition condition2)
        {
            _conditions2[0] = condition1;
            _conditions2[1] = condition2;
            return state.AddTransition(destination, _conditions2);
        }

        public static AnimatorStateTransition AddTransition(this AnimatorState state, AnimatorState destination, AnimatorCondition[] conditions)
        {
            var transition = new AnimatorStateTransition()
            {
                destinationState = destination,
                hasExitTime = false,
                duration = 0,
                hasFixedDuration = true,
                canTransitionToSelf = false,
                conditions = conditions,
            }.HideInHierarchy().AddTo(state);
            state.AddTransition(transition);
            return transition;
        }

        public static AnimatorController AddParameter<T>(this AnimatorController controller, string name, T defaultValue)
        {
            var param = new AnimatorControllerParameter()
            {
                name = name,

            };
            if (typeof(T) == typeof(float))
            {
                param.type = AnimatorControllerParameterType.Float;
                param.defaultFloat = (float)(object)defaultValue;
            }
            else if (typeof(T) == typeof(int))
            {
                param.type = AnimatorControllerParameterType.Int;
                param.defaultInt = (int)(object)defaultValue;
            }
            else if (typeof(T) == typeof(bool))
            {
                param.type = AnimatorControllerParameterType.Bool;
                param.defaultBool = (bool)(object)defaultValue;
            }
            else throw new ArgumentException(nameof(defaultValue));
            controller.AddParameter(param);
            return controller;
        }
    }
}
