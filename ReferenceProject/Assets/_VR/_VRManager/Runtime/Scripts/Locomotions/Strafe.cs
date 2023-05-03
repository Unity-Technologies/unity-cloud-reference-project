using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VRManager
{
    public class Strafe : LocomotionProvider
    {
        [SerializeField]
        InputActionReference m_LeftRight;

        [SerializeField]
        SpeedControl m_SpeedControl;

        InputAction m_LeftRightAction;
        Coroutine m_UpdateCoroutine;

        protected override void Awake()
        {
            base.Awake();

            m_LeftRightAction = m_LeftRight.action;
            m_LeftRightAction.performed += OnLeftRight;
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
            m_LeftRightAction.performed -= OnLeftRight;
        }

        void OnLeftRight(InputAction.CallbackContext callbackContext)
        {
            if (m_UpdateCoroutine == null)
            {
                m_UpdateCoroutine = StartCoroutine(UpdateCoroutine());
            }
        }

        IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                if (!m_LeftRightAction.IsInProgress())
                {
                    m_UpdateCoroutine = null;
                    yield break;
                }

                if (BeginLocomotion())
                {
                    var leftRightValue = m_LeftRightAction.ReadValue<float>();
                    var speed = m_SpeedControl.Speed;
                    var xrOrigin = system.xrOrigin;
                    var rightSource = xrOrigin.Camera.transform.right;

                    speed *= leftRightValue;
                    var motion = Time.deltaTime * speed * rightSource;
                    xrOrigin.transform.position += motion;
                    EndLocomotion();
                }

                yield return null;
            }
        }
    }
}
