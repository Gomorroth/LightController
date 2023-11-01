using System;
using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su.LightController.API
{
    public abstract class ShaderSettings : MonoBehaviour, IEditorOnly
    {
        protected virtual void OnEnable() { }
    }

    [Serializable]
    public abstract class ParameterGroup
    {

    }

}
