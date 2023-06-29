using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace gomoru.su.LightController
{
    [InitializeOnLoad]
    internal static class LightController
    {
        private const string EditorPrefsKey = "gomoru.su.LightController.generatedPrefabGUID";
        static LightController()
        {
            EditorApplication.delayCall += () =>
            {
                var guid = EditorPrefs.GetString(EditorPrefsKey, null);
                if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/LightController"))
                    {
                        AssetDatabase.CreateFolder("Assets", "LightController");
                    }
                    var prefab = new GameObject("LightController");
                    prefab.AddComponent<LightControllerGenerator>();
                    prefab.AddComponent<ModularAvatarMenuInstaller>();
                    PrefabUtility.SaveAsPrefabAsset(prefab, "Assets/LightController/LightController.prefab");
                    GameObject.DestroyImmediate(prefab);
                    EditorPrefs.SetString(EditorPrefsKey, AssetDatabase.AssetPathToGUID("Assets/LightController/LightController.prefab"));
                }
            };
        }
    }
}
