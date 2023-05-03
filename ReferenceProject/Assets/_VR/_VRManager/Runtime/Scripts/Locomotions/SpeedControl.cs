using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.VRManager
{
    public class SpeedControl : MonoBehaviour
    {
        [SerializeField]
        InputActionReference m_InputActionReference;

        [SerializeField]
        float m_MinimumSpeed = 2f;

        [SerializeField]
        float m_MaximumSpeed = 4f;

        InputAction m_InputAction;

        public float Speed
        {
            get
            {
                var speedControlValue = m_InputAction.ReadValue<float>();
                return Mathf.Lerp(m_MinimumSpeed, m_MaximumSpeed, speedControlValue);
            }
        }

        void Awake()
        {
            m_InputAction = m_InputActionReference.action;
        }

        void OnEnable()
        {
            m_InputAction.Enable();
        }

        void OnDisable()
        {
            m_InputAction.Disable();
        }
    }
}
