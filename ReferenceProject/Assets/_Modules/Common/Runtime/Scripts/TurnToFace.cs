using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Common
{
    public class TurnToFace : MonoBehaviour
    {
        [SerializeField, Tooltip("Speed to turn")]
        float m_TurnToFaceSpeed = 5f;

        [SerializeField, Tooltip("Interpolate camera rotation")]
        bool m_InterpolateCameraRotation = true;

        [SerializeField, Tooltip("Local rotation offset")]
        Vector3 m_RotationOffset = Vector3.zero;

        [SerializeField, Tooltip("If enabled, ignore the x axis when rotating")]
        bool m_IgnoreX;

        [SerializeField, Tooltip("If enabled, ignore the y axis when rotating")]
        bool m_IgnoreY;

        [SerializeField, Tooltip("If enabled, ignore the z axis when rotating")]
        bool m_IgnoreZ;

        ICameraProvider m_CameraProvider;
        Vector3 m_lastCameraPosition;

        [Inject]
        public void Setup(ICameraProvider cameraProvider)
        {
            m_CameraProvider = cameraProvider;
        }

        void OnEnable()
        {
            m_lastCameraPosition = m_CameraProvider.Camera.transform.position;
            transform.rotation = GetTargetRotation();
        }

        void Update()
        {
            if (m_CameraProvider.Camera.transform == null)
                return;

            var currentCamPosition = m_CameraProvider.Camera.transform.position;
            var targetRotation = GetTargetRotation();

            // avoid interpolating when there are large jumps in camera position
            if (m_InterpolateCameraRotation && (currentCamPosition - m_lastCameraPosition).sqrMagnitude < 1)
            {
                var ease = 1f - Mathf.Exp(-m_TurnToFaceSpeed * Time.unscaledDeltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ease);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            m_lastCameraPosition = currentCamPosition;
        }

        Quaternion GetTargetRotation()
        {
            var cameraTransform = m_CameraProvider.Camera.transform;
            
            if (cameraTransform == null)
                return Quaternion.identity;
            
            var facePosition = cameraTransform.position;
            var forward = facePosition - transform.position;
            var up = cameraTransform.up;
            var targetRotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward, up) : Quaternion.identity;
            targetRotation *= Quaternion.Euler(m_RotationOffset);
            if (m_IgnoreX || m_IgnoreY || m_IgnoreZ)
            {
                var targetEuler = targetRotation.eulerAngles;
                var currentEuler = transform.rotation.eulerAngles;
                targetRotation = Quaternion.Euler
                (
                    m_IgnoreX ? currentEuler.x : targetEuler.x,
                    m_IgnoreY ? currentEuler.y : targetEuler.y,
                    m_IgnoreZ ? currentEuler.z : targetEuler.z
                );
            }

            return targetRotation;
        }
    }
}
