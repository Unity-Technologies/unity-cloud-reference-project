using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.ReferenceProject.Navigation
{
    public class CameraViewData : ScriptableObject
    {
        [SerializeField]
        Sprite m_Icon;

        [SerializeField]
        string m_ViewName;

        [SerializeField]
        Vector3 m_AngleRotation;
        
        public Sprite Icon => m_Icon;
        public string ModeName => m_ViewName;
        public Vector3 Rotation => m_AngleRotation;
    }
}
