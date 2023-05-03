using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VRManager
{
    public class Fly : LocomotionProvider
    {
        [SerializeField]
        InputActionReference m_ForwardReverse;

        [SerializeField]
        SpeedControl m_SpeedControl;

        InputAction m_ForwardReverseAction;

        Coroutine m_UpdateCoroutine;

        protected override void Awake()
        {
            base.Awake();

            m_ForwardReverseAction = m_ForwardReverse.action;
            m_ForwardReverseAction.performed += OnForwardReverse;
        }

        void OnEnable()
        {
            m_ForwardReverseAction.Enable();
        }

        void OnDisable()
        {
            m_ForwardReverseAction.Disable();
        }

        void OnDestroy()
        {
            m_ForwardReverseAction.performed -= OnForwardReverse;
        }

        void OnForwardReverse(InputAction.CallbackContext callbackContext)
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
                if (!m_ForwardReverseAction.IsInProgress())
                {
                    m_UpdateCoroutine = null;
                    yield break;
                }

                if (BeginLocomotion())
                {
                    var forwardReverseValue = m_ForwardReverseAction.ReadValue<float>();
                    var speed = m_SpeedControl.Speed;
                    var xrOrigin = system.xrOrigin;
                    var forwardSource = xrOrigin.Camera.transform.forward;

                    speed *= forwardReverseValue;
                    var motion = Time.deltaTime * speed * forwardSource;
                    xrOrigin.transform.position += motion;
                    EndLocomotion();
                }

                yield return null;
            }
        }
    }
}
