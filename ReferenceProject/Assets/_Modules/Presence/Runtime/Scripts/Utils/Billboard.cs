using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField]
        Quaternion m_RotationOffset;

        ICameraProvider m_CameraProvider;

        [Inject]
        void Setup(ICameraProvider cameraProvider)
        {
            m_CameraProvider = cameraProvider;
        }

        void LateUpdate()
        {
            var rotation = m_CameraProvider.Camera.transform.rotation * m_RotationOffset;
            transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        }
    }
}
