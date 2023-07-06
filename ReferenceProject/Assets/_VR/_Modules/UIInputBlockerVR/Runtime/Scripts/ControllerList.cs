using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.VR.UIInputBlockerVR
{
    public interface IControllerList
    {
        List<GameObject> Controllers { get; }
    }

    public class ControllerList : IControllerList
    {
        public List<GameObject> Controllers { get; set; }
    }
}
