using System.Collections;
using System.Collections.Generic;
using Unity.ReferenceProject.AppCamera;
using UnityEngine;
using Unity.ReferenceProject.Navigation;

namespace Unity.ReferenceProject
{
    public class FollowNavigationMode : NavigationMode
    {
        public override void Teleport(Vector3 position, Vector3 eulerAngles) {}
    }
}
