﻿using System.Collections;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su
{
    [RequireComponent(typeof(ModularAvatarMenuInstaller))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class LightControllerGenerator : MonoBehaviour, IEditorOnly
    {
        [SerializeField, Range(1f, 10f)]
        public float LightMaxLimitMax = 1;

        [SerializeField]
        public bool UseMaterialPropertyAsDefault = true;

        [SerializeField]
        public LilToonLightParameters DefaultParameters = LilToonLightParameters.Default;

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
