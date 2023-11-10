using UnityEditor;

namespace gomoru.su.LightController
{
    [InitializeOnLoad]
    internal static partial class RuntimeHelper
    {
        static RuntimeHelper()
        {
            EditorApplication.delayCall += () =>
            {
                ShaderSettings.Initialize();
            };
        }
    }
}
