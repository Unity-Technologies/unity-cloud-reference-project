using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.VR.VRControls
{
    public class Turn : BaseLocomotionProvider
    {
        [SerializeField]
        float m_TurnSpeed = 2f;

        [SerializeField]
        bool m_Snap = true;

        [SerializeField]
        float m_SnapValue = 45f;

        Coroutine m_Coroutine;

        void OnDisable()
        {
            Cancel();
        }

        void Cancel()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        protected override void OnStarted(InputAction.CallbackContext callbackContext)
        {
            if (m_Coroutine == null && gameObject.activeInHierarchy)
            {
                m_Coroutine = StartCoroutine(OnLeftRight());
            }
        }

        protected override void OnCanceled(InputAction.CallbackContext callbackContext)
        {
            Cancel();
        }

        IEnumerator OnLeftRight()
        {
            while (true)
            {
                if (m_InputAction.IsPressed() && BeginLocomotion())
                {
                    var leftRightValue = m_InputAction.ReadValue<float>();
                    var xrOrigin = system.xrOrigin;

                    float rotation;
                    if (m_Snap)
                    {
                        var turnDirection = Mathf.Sign(leftRightValue);
                        rotation = m_SnapValue * turnDirection;
                    }
                    else
                    {
                        rotation = m_TurnSpeed * leftRightValue * Time.deltaTime;
                        Debug.Log(rotation);
                    }

                    xrOrigin.RotateAroundCameraUsingOriginUp(rotation);
                    EndLocomotion();

                    if (m_Snap)
                    {
                        m_Coroutine = null;
                        break;
                    }
                }

                yield return null;
            }
        }
    }
}
