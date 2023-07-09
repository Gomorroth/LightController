using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace gomoru.su.LightController
{
    [InitializeOnLoad]
    internal sealed class BuildManager : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => new nadena.dev.modular_avatar.core.editor.AvatarProcessor().callbackOrder - 100;

        static BuildManager()
        {
            RuntimeHelper.OnAwake = generator =>
            {
                var avatar = generator.GetComponentInParent<VRCAvatarDescriptor>();
                if (avatar != null )
                {
                    generator.gameObject.SetActive(false);
                    Process(avatar.gameObject, generator);
                    GameObject.DestroyImmediate(generator);
                    generator.gameObject.SetActive(true);
                }
            };
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var generator = avatarGameObject.GetComponentInChildren<LightControllerGenerator>();
            if (generator != null)
            {
                Process(avatarGameObject, generator);
                GameObject.DestroyImmediate(generator);
            }
            return true;
        }

        public static void Process(GameObject avatar, LightControllerGenerator generator)
        {
            Generator.Generate(avatar, generator);
            AssetDatabase.SaveAssets();
        }
    }
}
