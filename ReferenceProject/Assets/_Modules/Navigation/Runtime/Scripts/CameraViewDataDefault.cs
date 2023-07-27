using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.ReferenceProject.Navigation
{
    [CreateAssetMenu(menuName = "ReferenceProject/Navigation Manager/Camera View Data Default")]
    public class CameraViewDataDefault : CameraViewData
    {
        [SerializeField]
        bool m_UseDefaultView = true;
        
        public bool UseDefaultView => m_UseDefaultView;
    }
}
