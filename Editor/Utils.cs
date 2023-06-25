using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace gomoru.su
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
    }
}
