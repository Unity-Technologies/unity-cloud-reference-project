using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VR.VRControls
{
    public class Vertical : LocomotionProvider
    {
        [SerializeField]
        InputActionReference m_Up;

        [SerializeField]
        InputActionReference m_Down;

        [SerializeField]
        SpeedControl m_SpeedControl;

        [SerializeField]
        bool m_Snap;

        [SerializeField]
        float m_SnapValue = 1f;

        Coroutine m_Coroutine;
        InputAction m_DownAction;

        InputAction m_UpAction;

        protected override void Awake()
        {
            base.Awake();

            m_UpAction = m_Up.action;
            m_DownAction = m_Down.action;
            m_UpAction.started += OnStarted;
            m_DownAction.started += OnStarted;
        }

        void OnEnable()
        {
            m_UpAction.Enable();
            m_DownAction.Enable();
        }

        void OnDisable()
        {
            m_UpAction.Disable();
            m_DownAction.Disable();
        }

        void OnDestroy()
        {
            m_UpAction.started -= OnStarted;
            m_DownAction.started -= OnStarted;
        }

        void OnStarted(InputAction.CallbackContext obj)
        {
            if (m_Coroutine == null)
            {
                m_Coroutine = StartCoroutine(OnUpDownCoroutine());
            }
        }

        bool CanLocomote()
        {
            return (m_UpAction.IsPressed() || m_DownAction.IsPressed()) && BeginLocomotion();
        }

        IEnumerator OnUpDownCoroutine()
        {
            while (CanLocomote())
            {
                var upDownValue = (m_UpAction.IsPressed() ? 1f : 0f) +
                    (m_DownAction.IsPressed() ? -1f : 0f);
                var speed = m_SpeedControl.Speed;
                var xrOrigin = system.xrOrigin;
                var upSource = system.xrOrigin.transform.up;
                Vector3 motion;

                if (m_Snap)
                {
                    motion = upDownValue * m_SnapValue * upSource;
                }
                else
                {
                    speed *= upDownValue;
                    motion = Time.deltaTime * speed * upSource;
                }

                xrOrigin.transform.position += motion;
                EndLocomotion();

                if (m_Snap)
                {
                    break;
                }

                yield return null;
            }
            m_Coroutine = null;
        }
    }
}
