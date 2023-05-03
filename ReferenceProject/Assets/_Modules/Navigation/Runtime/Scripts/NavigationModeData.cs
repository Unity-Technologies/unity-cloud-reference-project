using System;
using UnityEngine;

namespace Unity.ReferenceProject.Navigation
{
    public abstract class NavigationModeData : ScriptableObject
    {
        [SerializeField]
        Sprite m_Icon;

        [SerializeField]
        string m_ModeName;

        [SerializeField]
        NavigationMode m_Prefab;

        public Sprite Icon => m_Icon;
        public string ModeName => m_ModeName;
        public NavigationMode Prefab => m_Prefab;

        public abstract bool CheckDeviceCapability();
        public abstract bool CheckDeviceAvailability();
    }
}
