using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su.LightController.API
{
    public abstract partial class ShaderSettings : MonoBehaviour, IEditorOnly
    {
        public virtual string DisplayName => GetType().Name;

        protected virtual void OnEnable() { }
    }
}
