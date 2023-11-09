using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace gomoru.su.LightController
{
    partial class LightControllerCore
    {
        private const string MenuPath = "GameObject/ModularAvatar/Light Controller";
        private const string PrefabGUID = "6436c0c7a87bd344cba20929d9db49fe";

        [MenuItem(MenuPath, true, 200)]
        public static bool CanAppendPrefabToAvatars() => Selection.gameObjects.Any(ValidateCore);

        [MenuItem(MenuPath, false, 200)]
        public static void AppendPrefabToAvatars()
        {
            List<GameObject> objectToCreated = new List<GameObject>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(PrefabGUID));
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
