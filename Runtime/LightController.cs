using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su.LightController
{
    [DisallowMultipleComponent]
    public sealed class LightController : MonoBehaviour, IEditorOnly
    {
        public LilToonParameters DefaultParameters = new();
        public List<GameObject> Excludes = new();

        public bool OverwriteMaterialSettings = false;

        private void OnEnable() { }
    }
}
