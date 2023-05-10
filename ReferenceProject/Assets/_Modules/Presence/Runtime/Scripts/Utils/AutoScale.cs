using System;
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

        Transform m_Target;
        Vector3 m_OriginalScale;

        [Inject]
        void Setup(Camera streamingCamera)
        {
            m_Target = streamingCamera.transform;
        }

        void Awake()
        {
            m_OriginalScale = transform.localScale;
        }

        void LateUpdate()
        {
            var distance = Vector3.Distance(m_Target.position,transform.position);
            var scale = Mathf.Clamp(m_ScaleFactor * distance, m_MinScaleFactor, m_MaxScaleFactor);
            transform.localScale = m_OriginalScale * scale;
        }
    }
}
