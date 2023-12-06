using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class AutoScale : MonoBehaviour
    {
        [SerializeField]
        float m_ScaleFactor = 1.0f;
        
        [SerializeField]
        float m_MinScaleFactor = 0.01f;
        
        [SerializeField]
        float m_MaxScaleFactor = 10.0f;

        ICameraProvider m_CameraProvider;
        Vector3 m_OriginalScale;

        [Inject]
        void Setup(ICameraProvider cameraProvider)
        {
            m_CameraProvider = cameraProvider;
        }

        void Awake()
        {
            m_OriginalScale = transform.localScale;
        }

        void LateUpdate()
        {
            var distance = Vector3.Distance(m_CameraProvider.Camera.transform.position,transform.position);
            var scale = Mathf.Clamp(m_ScaleFactor * distance, m_MinScaleFactor, m_MaxScaleFactor);
            transform.localScale = m_OriginalScale * scale;
        }
    }
}
