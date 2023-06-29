using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace gomoru.su.LightController
{
    [InitializeOnLoad]
    internal sealed class BuildManager : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => new nadena.dev.modular_avatar.core.editor.AvatarProcessor().callbackOrder - 100;

        static BuildManager()
        {
            RuntimeHelper.OnAwake = sender =>
            {
                var avatar = sender.gameObject.FindAvatarFromParent();
                if (avatar != null )
                {
                    Process(avatar.gameObject, sender);
                }
            };
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var generator = avatarGameObject.GetComponentInChildren<LightControllerGenerator>();
            if (generator != null)
            {
                Process(avatarGameObject, generator);
            }
            return true;
        }

        public static void Process(GameObject avatar, LightControllerGenerator generator)
        {
            generator.gameObject.SetActive(false);
            Generator.Generate(avatar, generator);
            AssetDatabase.SaveAssets();
            generator.gameObject.SetActive(true);
            GameObject.DestroyImmediate(generator.gameObject);
        }
    }
}
