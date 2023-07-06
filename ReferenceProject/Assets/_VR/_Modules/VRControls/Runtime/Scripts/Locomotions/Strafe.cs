using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.VR.VRControls
{
    public class Strafe : BaseLocomotionProvider
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
                    var leftRightValue = m_InputAction.ReadValue<float>();
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
