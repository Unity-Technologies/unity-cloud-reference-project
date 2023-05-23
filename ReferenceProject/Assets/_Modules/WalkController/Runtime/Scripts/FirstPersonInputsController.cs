using System;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.WalkController
{
    [RequireComponent(typeof(FirstPersonMoveController))]
    [RequireComponent(typeof(FirstPersonCameraController))]
    public class FirstPersonInputsController : NavigationMode
    {
        [SerializeField]
        InputActionAsset m_InputAsset;
        InputAction m_CameraRotateAction;
        FirstPersonCameraController m_FirstPersonCameraController;

        FirstPersonMoveController m_FirstPersonMoveController;
        InputAction m_JumpAction;

        InputAction m_MoveAction;
        InputAction m_RunAction;

        void Awake()
        {
            m_FirstPersonMoveController = GetComponent<FirstPersonMoveController>();
            m_FirstPersonCameraController = GetComponent<FirstPersonCameraController>();
        }

        void Update()
        {
            m_FirstPersonMoveController.OnMoveInput(m_MoveAction.ReadValue<Vector2>());

            if (m_CameraRotateAction.controls[0].IsPressed())
            {
                m_FirstPersonCameraController.OnViewInput(m_CameraRotateAction.ReadValue<Vector2>());
            }
        }

        void OnEnable()
        {
            InitializeInputs();
        }

        void OnDisable()
        {
            ResetInputs();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                return;
            }

            m_MoveAction.Reset();
            m_RunAction.Reset();
            m_JumpAction.Reset();
            m_CameraRotateAction.Reset();
        }

        void InitializeInputs()
        {
            m_MoveAction = m_InputAsset["Walk Navigation Action"];
            m_MoveAction.Reset();
            m_MoveAction.Enable();

            m_RunAction = m_InputAsset["Run Action"];
            m_RunAction.Reset();
            m_RunAction.performed += OnRunPerformed;
            m_RunAction.canceled += OnRunCanceled;
            m_RunAction.Enable();

            m_JumpAction = m_InputAsset["Jump Action"];
            m_JumpAction.Reset();
            m_JumpAction.performed += OnJumpPerformed;
            m_JumpAction.Enable();

            m_CameraRotateAction = m_InputAsset["Camera Control Action"];
            m_CameraRotateAction.Reset();
            m_CameraRotateAction.Enable();
        }

        void ResetInputs()
        {
            m_MoveAction.Disable();

            m_RunAction.performed -= OnRunPerformed;
            m_RunAction.canceled -= OnRunCanceled;
            m_RunAction.Disable();

            m_JumpAction.performed -= OnJumpPerformed;
            m_JumpAction.Disable();

            m_CameraRotateAction.Disable();
        }

        void OnRunPerformed(InputAction.CallbackContext obj) => m_FirstPersonMoveController.OnSprintInput(true);

        void OnRunCanceled(InputAction.CallbackContext obj) => m_FirstPersonMoveController.OnSprintInput(false);

        void OnJumpPerformed(InputAction.CallbackContext obj) => m_FirstPersonMoveController.OnJumpInput();

        public void EnableInputs()
        {
            InitializeInputs();
        }

        public void DisableInputs()
        {
            ResetInputs();
        }

        public override void Teleport(Vector3 position, Vector3 eulerAngles)
        {
            // Not supported yet
        }
    }
}
