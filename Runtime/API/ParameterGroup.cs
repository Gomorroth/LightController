using System;
using UnityEngine;

namespace gomoru.su.LightController.API
{
    [Serializable]
    public abstract class ParameterGroup
    {
        public virtual bool UseGroupNameAsPrefix => false;

        public virtual string Name
        {
            get
            {
                var name = GetType().Name;
                if (name.EndsWith("Group"))
                {
                    name = name.Remove(name.Length - "Group".Length);
                }
                return name;
            }
        }

        [HideInInspector]
        public bool IsEnable = true;

        [HideInInspector]
        public bool IsSync = true;

        [HideInInspector]
        public bool IsSave = true;
    }

}
