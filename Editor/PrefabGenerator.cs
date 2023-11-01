using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace gomoru.su.LightController
{
    [InitializeOnLoad]
    internal static class PrefabGenerator
    {
        private const string PrefabGUIDKeyPrefix = "gomoru.su.light-controller.prefab.";
        private const string PrefabFolder = "Assets/LightController/";

        private static Dictionary<Type, string> QualifiedNameCache = new Dictionary<Type, string>();

        public static GameObject GetOrCreate(Type type)
        {
            if (!QualifiedNameCache.TryGetValue(type, out var name))
            {
                name = type.GetCustomAttribute<QualifiedNameAttribute>()?.QualifiedName ?? type.FullName;
                QualifiedNameCache.Add(type, name);
            }
            var key = $"{PrefabGUIDKeyPrefix}{name}";
            string guid = PlayerPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
            {
                var obj = new GameObject() { hideFlags = HideFlags.HideInHierarchy };
                try
                {
                    string displayName = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.Name;
                    obj.AddComponent(type);
                    if (!AssetDatabase.IsValidFolder(PrefabFolder))
                    {
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(PrefabFolder), Path.GetFileName(PrefabFolder));
                    }
                    obj = PrefabUtility.SaveAsPrefabAsset(obj, $"{PrefabFolder}{displayName}.prefab");
                    guid = AssetDatabase.AssetPathToGUID(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj));
                    PlayerPrefs.SetString(key, guid);
                }
                finally
                {
                    Object.DestroyImmediate(obj);
                }
            }
            return PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid))) as GameObject;
        }
    }
}
