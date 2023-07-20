using System;
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
        public ParameterSyncSettings SyncSettings = ParameterSyncSettings.Default;

        [SerializeField]
        [ConditionParameter(LilToonParameters.GroupName_Lighting)]
        public bool AddLightingControl = true;

        [SerializeField]
        [ConditionParameter(LilToonParameters.GroupName_Backlight)]
        public bool AddBacklightControl = false;

        [SerializeField]
        public bool UseMaterialPropertyAsDefault = false;

        [SerializeField]
        public LilToonParameters DefaultParameters = new LilToonParameters();

#if UNITY_EDITOR

        [HideInInspector]
        public UnityEditor.Animations.AnimatorController FX;

#endif

        private void OnEnable() { }

        private void Awake()
        {
            if (enabled)
                RuntimeHelper.OnAwake?.Invoke(this);
            else
                Destroy(this);
        }
    }

    [Serializable]
    public struct ParameterSyncSettings
    {
        [SerializeField]
        public bool Lighting;

        [SerializeField]
        public bool Backlight;

        public static ParameterSyncSettings Default => new ParameterSyncSettings() { Lighting = true, Backlight = true };
    }
}
