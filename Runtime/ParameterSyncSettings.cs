using System;
using UnityEngine;

namespace gomoru.su.LightController
{
    [Serializable]
    public struct ParameterSyncSettings
    {
        [SerializeField]
        public bool Lighting;

        [SerializeField]
        public bool Backlight;

        [SerializeField]
        public bool DistanceFade;

        public static ParameterSyncSettings Default => new ParameterSyncSettings() { Lighting = true, Backlight = true, DistanceFade = true };
    }
}
