using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace gomoru.su.LightController
{
    partial class LightControllerCore
    {
        const string MenuPath = "GameObject/ModularAvatar/Light Controller";

        [MenuItem(MenuPath, true, 200)]
        public static bool CanAppendPrefabToAvatars() => AssetGenerator.TryGetPrefabAsset(out _) && Selection.gameObjects.Any(ValidateCore);

        [MenuItem(MenuPath, false, 200)]
        public static void AppendPrefabToAvatars()
        {
            List<GameObject> objectToCreated = new List<GameObject>();
            AssetGenerator.TryGetPrefabAsset(out var prefab);
            foreach (var x in Selection.gameObjects)
            {
                if (!ValidateCore(x))
                    continue;

                var obj = PrefabUtility.InstantiatePrefab(prefab, x.transform) as GameObject;

                objectToCreated.Add(obj);
            }

            if (objectToCreated.Count == 0)
                return;

            EditorGUIUtility.PingObject(objectToCreated[0]);
            Selection.objects = objectToCreated.ToArray();
        }

        private static bool ValidateCore(GameObject obj) => obj != null && obj.GetComponent<VRCAvatarDescriptor>() != null && obj.GetComponentInChildren<LightController>() == null;
    }
}
