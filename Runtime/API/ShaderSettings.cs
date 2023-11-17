using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using VRC.SDKBase;
using System.Reflection;
using System.Linq;
using System.Collections.Immutable;

[assembly:InternalsVisibleTo("gomoru.su.light-controller.editor")]

namespace gomoru.su.LightController.API
{
    public abstract partial class ShaderSettings : MonoBehaviour, IEditorOnly
    {
        public virtual string DisplayName => GetType().Name;
        public virtual string QualifiedName => GetType().FullName;

        protected virtual void OnEnable() { }

        public abstract bool IsTargetMaterial(Material material);

        public virtual void OnParameterPostProcess(string name, Parameter parameter, ref float min, ref float max) { }
    }
}
