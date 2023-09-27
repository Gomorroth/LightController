using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace gomoru.su.LightController
{
    internal sealed class ParameterControl
    {
        public string Name;
        public string Group = null;
        public bool IsMaster = false;
        public Func<LightController, bool> Condition = _ => true;
        public Action<(string Name, LightController Generator, LilToonParameters Parameters, List<Parameter> List)> Parameters;
        public Action<(string Path, Type Type, AnimationClip Default, AnimationClip Control, Material Material, LightController Generator, LilToonParameters Parameters)> SetAnimationCurves;
        public Action<(string Name, ParameterControl Self, List<VRCExpressionsMenu.Control> Controls)> CreateMenu;
        public Action<(LilToonParameters Parameters, Material Material)> GetValueFromMaterial;
        public AnimationClip Control;
        public AnimationClip Default;

        public struct Parameter
        {
            public string Name;
            public string Group;
            public float Value;
            public AnimatorControllerParameterType ParameterType;
            public bool BoolAsFloat;

            public ParameterConfig ToParameterConfig()
            {
                var result = new ParameterConfig()
                {
                    nameOrPrefix = Name,
                    defaultValue = Value,
                };
                switch (ParameterType)
                {
                    case AnimatorControllerParameterType.Float:
                        result.syncType = ParameterSyncType.Float;
                        break;
                    case AnimatorControllerParameterType.Int:
                        result.syncType = ParameterSyncType.Int;
                        break;
                    case AnimatorControllerParameterType.Bool:
                        result.syncType = ParameterSyncType.Bool;
                        break;
                }
                return result;
            }

            public AnimatorControllerParameter ToControllerParameter()
            {
                var result = new AnimatorControllerParameter()
                {
                    name = Name,
                    type = ParameterType == AnimatorControllerParameterType.Bool && BoolAsFloat ? AnimatorControllerParameterType.Float : ParameterType,
                };
                switch (result.type)
                {
                    case AnimatorControllerParameterType.Float:
                        result.defaultFloat = Value;
                        break;
                    case AnimatorControllerParameterType.Int:
                        result.defaultInt = (int)Value;
                        break;
                    case AnimatorControllerParameterType.Bool:
                        result.defaultBool = Value != 0;
                        break;
                }
                return result;
            }
        }
    }
}