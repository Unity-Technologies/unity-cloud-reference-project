using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.VR.VRControls
{
    public class Fly : BaseLocomotionProvider
    {
        [SerializeField]
        SpeedControl m_SpeedControl;

        Coroutine m_UpdateCoroutine;

        protected override void OnPerformed(InputAction.CallbackContext callbackContext)
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
                if (!m_InputAction.IsInProgress())
                {
                    m_UpdateCoroutine = null;
                    yield break;
                }

                if (BeginLocomotion())
                {
                    var forwardReverseValue = m_InputAction.ReadValue<float>();
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
