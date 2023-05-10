using System;
using UnityEngine;

namespace Unity.ReferenceProject.Navigation
{
    [CreateAssetMenu(menuName = "ReferenceProject/Navigation Manager/Simple Navigation Mode Data")]
    public class NavigationModeDataSimple : NavigationModeData
    {
        public override bool CheckDeviceCapability() => true;
        public override bool CheckDeviceAvailability() => true;
    }
}
