using System;
using UnityEngine;

namespace Unity.ReferenceProject.Navigation
{
    public abstract class NavigationMode : MonoBehaviour
    {
        public abstract void Teleport(Vector3 position, Vector3 eulerAngles);
    }
}
