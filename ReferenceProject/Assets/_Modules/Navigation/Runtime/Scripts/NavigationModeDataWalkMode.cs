using System;
using UnityEngine;

namespace Unity.ReferenceProject.Navigation
{
    [CreateAssetMenu(menuName = "ReferenceProject/Navigation Manager/Navigation Mode Data WalkMode")]
    public class NavigationModeDataWalkMode : NavigationModeData
    {
        public override bool CheckDeviceCapability()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            return true;
#else 
            return false;
#endif
        }
        public override bool CheckDeviceAvailability()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            return true;
#else 
            return false;
#endif
        }
    }
}
