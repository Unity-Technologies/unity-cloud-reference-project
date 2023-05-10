using System;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField]
        Quaternion m_RotationOffset;
        
        Transform m_Target;

        [Inject]
        void Setup(Camera streamingCamera)
        {
            m_Target = streamingCamera.transform;
        }

        void LateUpdate()
        {
            var rotation = m_Target.transform.rotation * m_RotationOffset;
            transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        }
    }
}
