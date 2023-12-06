using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.InputSystem;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Unity.ReferenceProject.WalkController
{
    [RequireComponent(typeof(WalkModeMoveController))]
    [RequireComponent(typeof(WalkModeCameraController))]
    public class WalkModeInputsController : NavigationMode
    {
        const string k_JumpAction = "Jump Action";
        const string k_MoveAction = "Walk Navigation Action";
        const string k_RunAction = "Run Action";
        const string k_TeleportAction = "Teleport Click Action";
        const string k_CameraRotateAction = "Camera Rotate Action";

        [SerializeField]
        InputActionAsset m_InputAsset;
        
        WalkModeCameraController m_WalkModeCameraController;
        WalkModeMoveController m_WalkModeMoveController;

        InputActionWrapper m_CameraRotateAction;
        InputActionWrapper m_JumpAction;
        InputActionWrapper m_MoveAction;
        InputActionWrapper m_RunAction;
        InputActionWrapper m_TeleportAction;

        InputScheme m_InputScheme;
        IInputManager m_InputManager;
        IObjectPicker m_ObjectPicker;

        bool m_CameraRotated;
        bool m_IsTeleporting;

        ICameraProvider m_CameraProvider;

        // Teleport
        [Tooltip("Offset from the contact point back towards the start position")]
        [SerializeField]
        float m_ArrivalOffsetRelative = 1f;

        [Tooltip("Offset along the surface normal of the selected object")]
        [SerializeField]
        float m_ArrivalOffsetNormal = 1f;

        [Tooltip("Fixed offset in world space")]
        [SerializeField]
        Vector3 m_ArrivalOffsetFixed = Vector3.zero;

        [Tooltip("Fixed time for the teleport animation")]
        [SerializeField]
        float m_LerpTime = 1f;

        [Tooltip("Teleport animation smooth curve")]
        [SerializeField]
        AnimationCurve m_SmoothCurve;        

        [Inject]
        void Setup(IObjectPicker objectPicker, IInputManager inputManager, ICameraProvider cameraProvider)
        {
            m_ObjectPicker = objectPicker;
            m_InputManager = inputManager;
            m_CameraProvider = cameraProvider;
        }

        void Awake()
        {
            m_WalkModeMoveController = GetComponent<WalkModeMoveController>();
            m_WalkModeCameraController = GetComponent<WalkModeCameraController>();
        }

        void Update()
        {
            UpdateTranslation();
            UpdateRotation();
        }

        void UpdateTranslation()
        {
            if (m_MoveAction.AttachedScheme.IsSchemeEligibleForInputs() &&
                m_MoveAction.AttachedScheme.IsActionEligibleForTrigger(m_MoveAction) &&
                m_MoveAction.InputAction.inProgress)
            {
                m_WalkModeMoveController.OnMoveInput(m_MoveAction.InputAction.ReadValue<Vector2>());
            }
        }

        void UpdateRotation()
        {
            if (m_CameraRotated)
            {
                m_WalkModeCameraController.OnViewInput(m_CameraRotateAction.InputAction.ReadValue<Vector2>());
            }

            bool isCursorLocked = m_CameraRotateAction.AttachedScheme.IsSchemeEligibleForInputs() &&
                m_CameraRotateAction.AttachedScheme.IsActionEligibleForTrigger(m_CameraRotateAction) &&
                m_CameraRotateAction.InputAction.controls[0].IsPressed();

            m_WalkModeCameraController.InternalLockUpdate(isCursorLocked);
        }

        void CameraRotated(InputAction.CallbackContext c)
        {
            m_CameraRotated = true;
        }

        void CameraStopRotation(InputAction.CallbackContext c)
        {
            m_CameraRotated = false;
        }

        void PickFromInput(InputAction.CallbackContext context)
        {
            if (m_IsTeleporting)
                return;

            _ = TeleportRaycast(Pointer.current.position.value);
        }

        void OnEnable()
        {
            InitializeInputs();
        }

        void OnDisable()
        {
            ResetInputs();
        }

        private void OnDestroy()
        {
            m_InputScheme.Dispose();
            m_InputScheme = null;
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
            m_InputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.WalkMode, InputSchemeCategory.Controller, m_InputAsset);
            m_InputScheme.SetEnable(true);

            m_MoveAction = m_InputScheme[k_MoveAction];
            m_MoveAction.OnPerformed += OnMoveKeyPressPerformed;
            m_MoveAction.InputAction.Reset();
            m_MoveAction.IsUISelectionCheckEnabled = true;
            m_MoveAction.IsEnabled = true;

            m_RunAction = m_InputScheme[k_RunAction];
            m_RunAction.InputAction.Reset();
            m_RunAction.OnPerformed += OnRunPerformed;
            m_RunAction.OnCanceled += OnRunCanceled;
            m_RunAction.IsUISelectionCheckEnabled = true;
            m_RunAction.IsEnabled = true;

            m_JumpAction = m_InputScheme[k_JumpAction];
            m_JumpAction.Reset();
            m_JumpAction.OnPerformed += OnJumpPerformed;
            m_JumpAction.IsUISelectionCheckEnabled = true;
            m_JumpAction.IsEnabled = true;

            m_CameraRotateAction = m_InputScheme[k_CameraRotateAction];
            m_CameraRotateAction.Reset();
            m_CameraRotateAction.IsEnabled = true;
            m_CameraRotateAction.IsUIPointerCheckEnabled = true;
            m_CameraRotateAction.OnPerformed += CameraRotated;
            m_CameraRotateAction.OnCanceled += CameraStopRotation;

            m_TeleportAction = m_InputScheme[k_TeleportAction];
            m_TeleportAction.OnPerformed += PickFromInput;
            m_TeleportAction.IsUIPointerCheckEnabled = true;
            m_TeleportAction.IsEnabled = true;
        }

        void ResetInputs()
        {
            m_InputScheme.SetEnable(false);
            m_MoveAction.IsEnabled = false;
            m_JumpAction.IsEnabled = false;
            m_CameraRotateAction.IsEnabled = false;
            m_RunAction.IsEnabled = false;
            m_TeleportAction.IsEnabled = false;

            m_CameraRotateAction.OnPerformed -= CameraRotated;
            m_CameraRotateAction.OnCanceled -= CameraStopRotation;
            m_RunAction.OnPerformed -= OnRunPerformed;
            m_RunAction.OnCanceled -= OnRunCanceled;
            m_JumpAction.OnPerformed -= OnJumpPerformed;
            m_TeleportAction.OnPerformed -= PickFromInput;
            m_MoveAction.OnPerformed -= OnMoveKeyPressPerformed;

            //Security to always keep cursor visible when exiting walkmode
            m_WalkModeCameraController.InternalLockUpdate(false);
        }

        void OnMoveKeyPressPerformed(InputAction.CallbackContext obj) => m_WalkModeMoveController.OnMoveInputPressed();
        
        void OnRunPerformed(InputAction.CallbackContext obj) => m_WalkModeMoveController.OnSprintInput(true);

        void OnRunCanceled(InputAction.CallbackContext obj) => m_WalkModeMoveController.OnSprintInput(false);

        void OnJumpPerformed(InputAction.CallbackContext obj) => m_WalkModeMoveController.OnJumpInput();

        public void EnableInputs()
        {
            InitializeInputs();
        }

        public void DisableInputs()
        {
            ResetInputs();
        }
        
        /// <summary>
        ///     The following code all relates to the Teleport feature
        /// </summary>
        public override void Teleport(Vector3 position, Vector3 eulerAngles)
        {
            m_MoveAction.Reset();
            m_RunAction.Reset();
            m_JumpAction.Reset();
            m_CameraRotateAction.Reset();
            
            m_WalkModeMoveController.OnHoverTeleportInput();
            m_WalkModeCameraController.ApplyNewPositionRotation(position, eulerAngles);
        }

        async Task TeleportRaycast(Vector2 mousePosition)
        {
            if (m_IsTeleporting)
                return;

            try
            {
                var ray = m_CameraProvider.Camera.ScreenPointToRay(mousePosition);
                var raycastResult = await m_ObjectPicker.RaycastAsync(ray);
                if (raycastResult.HasIntersected)
                {
                    m_IsTeleporting = true;
                    m_WalkModeMoveController.OnTeleportInput(m_IsTeleporting);

                    var currTransform = m_WalkModeCameraController.transform;
                    var normal = raycastResult.Normal;
                    var source = currTransform.position;
                    var target = raycastResult.Point +
                        m_ArrivalOffsetFixed +
                        m_ArrivalOffsetNormal * normal +
                        m_ArrivalOffsetRelative * (source - raycastResult.Point).normalized;
                   StartCoroutine(TeleportCoroutine(source, target, currTransform));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        IEnumerator TeleportCoroutine(Vector3 startPosition, Vector3 targetPosition, Transform currentTransform)
        {
            var startRotation = Quaternion.Euler(currentTransform.eulerAngles.x, currentTransform.eulerAngles.y, 0);
            var forward = targetPosition - startPosition;
            var targetRotation = Quaternion.LookRotation(forward);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            
            var t = 0f;
            while(t < m_LerpTime)
            {
                var evaluatedValue = m_SmoothCurve.Evaluate(t / m_LerpTime);
                var position = Vector3.Lerp(startPosition, targetPosition, evaluatedValue);
                var rotation = Quaternion.Lerp(startRotation, targetRotation, evaluatedValue);
                
                TeleportTo(position, rotation);
                t += Time.deltaTime;
                yield return null;
            }
            
            TeleportTo(targetPosition, targetRotation);
            m_IsTeleporting = false;
            m_WalkModeMoveController.OnTeleportInput(m_IsTeleporting);
        }

        void TeleportTo(Vector3 newPos, Quaternion newRot)
        { 
            m_MoveAction.Reset();
            m_RunAction.Reset();
            m_JumpAction.Reset();
            m_CameraRotateAction.Reset();
            
            m_WalkModeCameraController.ApplyNewPositionRotation(newPos, newRot.eulerAngles);
        }
    }
}
