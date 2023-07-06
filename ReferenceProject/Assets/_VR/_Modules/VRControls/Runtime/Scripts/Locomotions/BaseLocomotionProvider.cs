using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VR.VRControls
{
    public abstract class BaseLocomotionProvider : LocomotionProvider
    {
        [SerializeField]
        InputActionReference m_InputActionReference;

        protected InputAction m_InputAction;

        protected override void Awake()
        {
            base.Awake();
            m_InputAction = m_InputActionReference.action;
            InitializeInputs();
        }

        void OnDestroy()
        {
            ResetInputs();
        }

        public void EnableInputs()
        {
            InitializeInputs();
        }

        public void DisableInputs()
        {
            ResetInputs();
        }

        protected virtual void InitializeInputs()
        {
            m_InputAction.Enable();
            m_InputAction.started += OnStarted;
            m_InputAction.performed += OnPerformed;
            m_InputAction.canceled += OnCanceled;
        }

        protected virtual void ResetInputs()
        {
            m_InputAction.Disable();
            m_InputAction.started -= OnStarted;
            m_InputAction.performed -= OnPerformed;
            m_InputAction.canceled -= OnCanceled;
        }

        protected virtual void OnStarted(InputAction.CallbackContext callbackContext) { }
        protected virtual void OnPerformed(InputAction.CallbackContext callbackContext) { }
        protected virtual void OnCanceled(InputAction.CallbackContext callbackContext) { }
    }
}
