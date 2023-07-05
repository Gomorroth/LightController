using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su.LightController
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class LightControllerGenerator : MonoBehaviour, IEditorOnly
    {
        [SerializeField, Range(1f, 10f)]
        public float LightMaxLimitMax = 1;

        [SerializeField]
        public bool SaveParameters = true;

        [SerializeField]
        public bool UseMaterialPropertyAsDefault = false;

        [SerializeField]
        public LilToonParameters DefaultParameters = new LilToonParameters();

#if UNITY_EDITOR

        [HideInInspector]
        public UnityEditor.Animations.AnimatorController FX;

#endif

        private void Awake()
        {
            RuntimeHelper.OnAwake?.Invoke(this);
        }
    }
}
