using System;
using UnityEngine;

namespace Unity.ReferenceProject.WalkController
{
    public class FirstPersonCameraController : MonoBehaviour
    {
        [SerializeField]
        Transform m_CameraRoot;

        [SerializeField]
        float m_XSensitivity = 2f;

        [SerializeField]
        float m_YSensitivity = 2f;

        [SerializeField]
        bool m_ClampVerticalRotation = true;

        [SerializeField]
        float m_MinimumX = -90F;

        [SerializeField]
        float m_MaximumX = 90F;

        [SerializeField]
        bool m_Smooth;

        [SerializeField]
        float m_SmoothTime = 5f;

        [SerializeField]
        bool m_LockCursor = true;
        Quaternion m_CameraTargetRot;

        Quaternion m_CharacterTargetRot;

        // null if there is no input. Any value when we received input.
        // This helps to determine if the mouse button is still pressed to hide or show the cursor.
        Vector2? m_MoveInput;

        void Update()
        {
            if (m_MoveInput != null)
            {
                var yRot = m_MoveInput.Value.x * m_XSensitivity;
                var xRot = m_MoveInput.Value.y * m_YSensitivity;
                m_MoveInput = null;

                m_CharacterTargetRot = transform.localRotation;
                m_CameraTargetRot = m_CameraRoot.localRotation;

                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

                if (m_ClampVerticalRotation)
                    m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
                if (m_Smooth)
                {
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, m_CharacterTargetRot,
                        m_SmoothTime * Time.deltaTime);
                    m_CameraRoot.localRotation = Quaternion.Slerp(m_CameraRoot.localRotation, m_CameraTargetRot,
                        m_SmoothTime * Time.deltaTime);
                }
                else
                {
                    transform.localRotation = m_CharacterTargetRot;
                    m_CameraRoot.localRotation = m_CameraTargetRot;
                }

                // if the user set "lockCursor" we check & properly lock the cursos
                if (m_LockCursor)
                    InternalLockUpdate(true);
            }
            else
            {
                // if the user set "lockCursor" we check & properly lock the cursos
                if (m_LockCursor)
                    InternalLockUpdate(false);
            }
        }

        void InternalLockUpdate(bool isLockCursor)
        {
            if (isLockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, m_MinimumX, m_MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

        public void OnViewInput(Vector2 moveInput)
        {
            m_MoveInput = moveInput;
        }
    }
}
