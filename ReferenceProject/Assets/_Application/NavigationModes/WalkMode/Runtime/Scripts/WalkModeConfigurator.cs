using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.WalkController
{
    [RequireComponent(typeof(CharacterController))]
    public class WalkModeConfigurator : MonoBehaviour
    {
        ICameraProvider m_CameraProvider;
        CharacterController m_CharacterController;

        [Inject]
        public void Setup(ICameraProvider viewCamera)
        {
            m_CameraProvider = viewCamera;
        }

        void Awake()
        {
            // Preventing CharacterController from position reset
            m_CharacterController = GetComponent<CharacterController>();
            var cameraTransform = m_CameraProvider.Camera.transform;
            
            m_CharacterController.enabled = false;
            // Matching camera to the character's head, not its center
            transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y - m_CharacterController.height/2f, cameraTransform.position.z);
            m_CharacterController.enabled = true;

            // Makes camera rotation the same
            var rotation = cameraTransform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
        }

        void LateUpdate()
        {
            if (m_CameraProvider.Camera != null)
            {
                var newPosition = transform.position;
                newPosition = new Vector3(newPosition.x, newPosition.y + m_CharacterController.height/2f, newPosition.z); // Matching camera to the character's head
                m_CameraProvider.Camera.transform.SetPositionAndRotation(newPosition, transform.rotation);
            }
        }
    }
}
