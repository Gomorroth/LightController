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

        protected virtual void OnEnable() { }

        public abstract bool IsTargetMaterial(Material material);

        internal Parameter[] GetParameters()
        {
            return _GetParameters[this.GetType()](this);
        }

        internal static ImmutableDictionary<Type, Func<ShaderSettings, Parameter[]>> _GetParameters;
    }
}
