using System;
using UnityEngine;

namespace Unity.ReferenceProject.Navigation
{
    [CreateAssetMenu(menuName = "ReferenceProject/Navigation Manager/Navigation Mode Data WalkMode")]
    public class NavigationModeDataWalkMode : NavigationModeData
    {
        public override bool CheckDeviceCapability() => true;
        public override bool CheckDeviceAvailability() => true;
    }
}
