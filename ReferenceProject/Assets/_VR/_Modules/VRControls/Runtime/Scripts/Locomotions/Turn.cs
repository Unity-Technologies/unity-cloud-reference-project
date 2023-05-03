using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VR.VRControls
{
    public class Turn : LocomotionProvider
    {
        [SerializeField]
        InputActionReference m_LeftRight;

        [SerializeField]
        float m_TurnSpeed = 2f;

        [SerializeField]
        bool m_Snap = true;

        [SerializeField]
        float m_SnapValue = 45f;

        Coroutine m_Coroutine;

        InputAction m_LeftRightAction;

        protected override void Awake()
        {
            base.Awake();

            m_LeftRightAction = m_LeftRight.action;
            m_LeftRightAction.started += OnStarted;
            m_LeftRightAction.canceled += OnCancel;
        }

        void OnEnable()
        {
            m_LeftRightAction.Enable();
        }

        void OnDisable()
        {
            m_LeftRightAction.Disable();
        }

        void OnDestroy()
        {
            m_LeftRightAction.started -= OnStarted;
            m_LeftRightAction.canceled -= OnCancel;
        }

        void OnStarted(InputAction.CallbackContext obj)
        {
            if (m_Coroutine == null)
            {
                m_Coroutine = StartCoroutine(OnLeftRight());
            }
        }

        void OnCancel(InputAction.CallbackContext obj)
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        IEnumerator OnLeftRight()
        {
            while (true)
            {
                if (m_LeftRightAction.IsPressed() && BeginLocomotion())
                {
                    var leftRightValue = m_LeftRightAction.ReadValue<float>();
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
