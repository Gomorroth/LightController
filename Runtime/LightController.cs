using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su.LightController
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class LightController : MonoBehaviour, IEditorOnly
    {
        [SerializeField, Range(1f, 10f)]
        [LimitParameter(nameof(LilToonParameters.LightMaxLimit))]
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
        [ConditionParameter(LilToonParameters.GroupName_DistanceFade)]
        public bool AddDistanceFadeControl = false;

        [SerializeField, Range(0, 1)]
        [LimitParameter(nameof(LilToonParameters.DistanceFadeEnd))]
        public float DistanceFadeEndMax = 1f;

        [SerializeField]
        public bool AddResetButton = false;

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
}
