using System;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.WalkController
{
    [RequireComponent(typeof(CharacterController))]
    public class WalkModeConfigurator : MonoBehaviour
    {
        [SerializeField]
        Transform m_CameraRoot;

        Camera m_ViewCamera;

        [Inject]
        public void Setup(Camera viewCamera)
        {
            m_ViewCamera = viewCamera;

            // Preventing CharacterController from position reset
            var controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.position = viewCamera.transform.position + (transform.position - m_CameraRoot.position);
            controller.enabled = true;

            // Makes camera rotation the same
            var rotation = viewCamera.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
            m_CameraRoot.rotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
        }

        void LateUpdate()
        {
            if (m_ViewCamera)
            {
                m_ViewCamera.transform.position = m_CameraRoot.position;
                m_ViewCamera.transform.rotation = m_CameraRoot.rotation;
            }
        }
    }
}
