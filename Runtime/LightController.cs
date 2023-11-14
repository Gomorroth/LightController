using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace gomoru.su.LightController
{
    [DisallowMultipleComponent]
    public sealed class LightController : MonoBehaviour, IEditorOnly
    {
        [HideInInspector]
        public List<string> ExpandedGroups;

        [SerializeField]
        public List<GameObject> Excludes = new List<GameObject>();

        private void OnEnable() { }
    }
}
