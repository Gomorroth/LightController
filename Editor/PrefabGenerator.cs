using nadena.dev.modular_avatar.core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [InitializeOnLoad]
    internal static class PrefabGenerator
    {
        private const string EditorPrefsKey = "gomoru.su.LightController.generatedPrefabGUID";
        private const string PrefabPath = "Assets/LightController/LightController.prefab";

        static PrefabGenerator()
        {
            EditorApplication.delayCall += () =>
            {
                var guid = EditorPrefs.GetString(EditorPrefsKey, null);
                if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                {
                    var directory = Path.GetDirectoryName(PrefabPath);
                    if (!AssetDatabase.IsValidFolder(directory))
                    {
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(directory), Path.GetFileName(directory));
                    }
                    var prefab = new GameObject(Path.GetFileNameWithoutExtension(PrefabPath)) { hideFlags = HideFlags.HideInHierarchy }; 
                    var generator = prefab.AddComponent<LightController>();
                    generator.SaveParameters = false;
                    prefab.AddComponent<ModularAvatarMenuInstaller>();
                    PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
                    GameObject.DestroyImmediate(prefab);
                    EditorPrefs.SetString(EditorPrefsKey, AssetDatabase.AssetPathToGUID(PrefabPath));
                }
            };
        }
    }
}
