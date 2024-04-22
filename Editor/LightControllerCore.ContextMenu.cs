using System.Collections.Generic;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace gomoru.su.LightController;

partial class LightControllerCore
{
    private const string MenuPath = "GameObject/ModularAvatar/Light Controller";
    private const string PrefabGUID = "84a28265b62aef84e8d298910a04005b";

    private static GameObject Prefab => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(PrefabGUID));

    [MenuItem(MenuPath, true, 200)]
    public static bool CanAppendPrefabToAvatars() => Prefab != null && Selection.gameObjects.Any(ValidateCore);

    [MenuItem(MenuPath, false, 200)]
    public static void AppendPrefabToAvatars()
    {
        List<GameObject> objectToCreated = new List<GameObject>();
        foreach (var x in Selection.gameObjects)
        {
            if (!ValidateCore(x))
                continue;

            var obj = PrefabUtility.InstantiatePrefab(Prefab, x.transform) as GameObject;

            objectToCreated.Add(obj);
        }

        if (objectToCreated.Count == 0)
            return;

        EditorGUIUtility.PingObject(objectToCreated[0]);
        Selection.objects = objectToCreated.ToArray();
    }

    private static bool ValidateCore(GameObject obj) => obj != null && obj.GetComponent<VRCAvatarDescriptor>() != null && obj.GetComponentInChildren<LightController>() == null;
}
