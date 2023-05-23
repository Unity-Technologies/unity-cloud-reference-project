using System;
using UnityEngine;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    public class UIXRCursor : MonoBehaviour
    {
        [SerializeField]
        float m_MaxDistance = 5f;

        public float MaxDistance => m_MaxDistance;
        
        public Vector3 Position => transform.position;
        public Vector3 Direction => transform.forward;
    }
}
