using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.ReferenceProject.AppCamera;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Unity.ReferenceProject.Application
{
    public class FlyOrbitController : NavigationMode
    {
        [SerializeField]
        CameraController m_CameraController;

        [SerializeField]
        InputActionAsset m_InputActionAsset;

        [Tooltip("Offset from the contact point back towards the start position")]
        [SerializeField]
        float m_ArrivalOffsetRelative = 1f;

        [Tooltip("Angle threshold for teleport to detect if the surface is a floor")]
        [SerializeField]
        float m_AngleThreshold = 10f;

        [Tooltip("Fixed time for the teleport animation")]
        [SerializeField]
        float m_LerpTime = 1f;

        [Tooltip("Teleport animation smooth curve")]
        [SerializeField]
        AnimationCurve m_SmoothCurve;

        readonly Vector3 m_ResetCameraEuler = new(25, 0, 0);
        readonly Vector3 m_ResetCameraPosition = new(0, 5, -10);

        bool m_IsInputsEnabled;
        bool m_IsTeleporting;
        bool m_IsValidStart;

        // Keyboard & Mouse & Gamepad
        static readonly string k_MovingAction = "Moving Action";
        static readonly string k_MovingActionGamepad = "Moving Action Gamepad";
        static readonly string k_WorldOrbitActionGamepad = "WorldOrbit Action Gamepad";
        static readonly string k_OrbitAction = "Orbit Action";
        static readonly string k_WorldOrbitAction = "WorldOrbit Action";
        static readonly string k_PanStart = "Pan Start";
        static readonly string k_PanAction = "Pan Action";
        static readonly string k_ZoomAction = "Zoom Action";
        static readonly string k_TeleportClickAction = "Teleport Click Action";

        // Touch Gestures
        static readonly string k_PanGestureAction = "Pan Gesture Action";
        static readonly string k_ZoomGestureAction = "Zoom Gesture Action";
        static readonly string k_OrbitGestureAction = "Orbit Gesture Action";
        
        IObjectPicker m_ObjectPicker;
        ICameraProvider m_CameraProvider;
        IInputManager m_InputManager;
        InputScheme m_InputScheme;

        [Inject]
        void Setup(IObjectPicker objectPicker, ICameraProvider cameraProvider, IInputManager inputManager)
        {
            m_ObjectPicker = objectPicker;
            m_CameraProvider = cameraProvider;
            m_InputManager = inputManager;
        }

        void Awake()
        {
            InitializeInputs();
        }

        void OnDestroy()
        {
            ResetInputs();
            m_InputScheme?.Dispose();
        }

        void Reset()
        {
            ResetInputs();
            ResetCameraTransform();
        }

        void Update()
        {
            UpdateMovement();
        }

        void InitializeInputs()
        {
            if (m_IsInputsEnabled)
                return;

            m_InputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.FlyOrbital, InputSchemeCategory.Controller, m_InputActionAsset);
            m_InputScheme.SetEnable(true);

            m_IsInputsEnabled = true;

            // Keyboard and Mouse
            m_CameraController.MovingAction = m_InputScheme[k_MovingAction];

            m_InputScheme[k_MovingAction].IsUISelectionCheckEnabled = true;
            m_InputScheme[k_MovingAction].IsEnabled = true;

            m_InputScheme[k_OrbitAction].OnStarted += StartCheck;
            m_InputScheme[k_OrbitAction].OnPerformed += TryOrbit;
            m_InputScheme[k_OrbitAction].IsDelayedInput = true;

            m_InputScheme[k_WorldOrbitAction].OnStarted += StartCheck;
            m_InputScheme[k_WorldOrbitAction].OnPerformed += TryWorldOrbit;
            m_InputScheme[k_WorldOrbitAction].IsDelayedInput = true;

            m_InputScheme[k_PanStart].OnStarted += StartCheck;
            m_InputScheme[k_PanStart].OnPerformed += TryPanStart;
            m_InputScheme[k_PanStart].IsUIPointerCheckEnabled = true;

            m_InputScheme[k_PanAction].OnPerformed += TryPan;
            m_InputScheme[k_PanAction].IsDelayedInput = true;

            m_InputScheme[k_ZoomAction].OnPerformed += TryZoom;
            m_InputScheme[k_ZoomAction].IsUIPointerCheckEnabled = true;

            m_InputScheme[k_TeleportClickAction].OnPerformed += PickFromInput;
            m_InputScheme[k_TeleportClickAction].IsUIPointerCheckEnabled = true;

            m_InputScheme[k_OrbitAction].IsEnabled = true;
            m_InputScheme[k_WorldOrbitAction].IsEnabled = true;
            m_InputScheme[k_PanStart].IsEnabled = true;
            m_InputScheme[k_PanAction].IsEnabled = true;
            m_InputScheme[k_ZoomAction].IsEnabled = true;
            m_InputScheme[k_TeleportClickAction].IsEnabled = true;

            // Touch Gestures
            m_InputScheme[k_PanGestureAction].OnStarted += StartCheck;
            m_InputScheme[k_PanGestureAction].OnPerformed += TryPanStartGesture;
            m_InputScheme[k_PanGestureAction].OnPerformed += TryPanGesture;

            m_InputScheme[k_ZoomGestureAction].OnPerformed += m_CameraController.OnZoomGestureStarted;
            m_InputScheme[k_ZoomGestureAction].OnPerformed += m_CameraController.OnZoomGesture;

            m_InputScheme[k_OrbitGestureAction].OnStarted += StartCheck;
            m_InputScheme[k_OrbitGestureAction].OnPerformed += TryOrbitGesture;
            m_InputScheme[k_OrbitGestureAction].IsDelayedInput = true;

            m_InputScheme[k_ZoomGestureAction].IsEnabled = true;
            m_InputScheme[k_PanGestureAction].IsEnabled = true;
            m_InputScheme[k_OrbitGestureAction].IsEnabled = true;

            // Gamepad
            m_CameraController.MovingActionGamepad = m_InputScheme[k_MovingActionGamepad];
            
            m_InputScheme[k_MovingActionGamepad].IsUISelectionCheckEnabled = true;
            m_InputScheme[k_MovingActionGamepad].IsEnabled = true;
            
            m_InputScheme[k_WorldOrbitActionGamepad].OnPerformed += (context) => { m_CameraController.OnWorldOrbit(context);};
            m_InputScheme[k_WorldOrbitActionGamepad].IsEnabled = true;
        }

        void ResetInputs()
        {
            m_IsInputsEnabled = false;
            m_CameraController.ForceStop();

            // Keyboard and Mouse
            m_InputScheme.SetEnable(false);

            // Keyboard and Mouse
            m_CameraController.MovingAction = null;
            
            // Mobile Gamepad
            m_CameraController.MovingActionGamepad = null;
        }

        void ResetCameraTransform()
        {
            // Example position, can be set to any desired Transform with overload method ResetTo(new Transform)
            m_CameraController.ResetTo(m_ResetCameraPosition, m_ResetCameraEuler, Vector3.forward);
        }

        void UpdateMovement()
        {
            // Ignore movement if asset is disabled
            if (!m_IsInputsEnabled)
                return;

            m_CameraController.UpdateMovingActions();
        }

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
            var rotation = Quaternion.Euler(eulerAngles);
            var lookAt = rotation * new Vector3(0.0f, 0.0f, (Vector3.zero - position).magnitude) + position;
            m_CameraController.ResetTo(position, eulerAngles, lookAt);
        }

        void PickFromInput(InputAction.CallbackContext context)
        {
            if (m_IsTeleporting)
                return;

            _ = TeleportRaycast(Pointer.current.position.value);
        }

        async Task TeleportRaycast(Vector2 screenPosition)
        {
            if (m_IsTeleporting)
                return;

            try
            {
                var currentCamera = m_CameraProvider.Camera;
                var ray = currentCamera.ScreenPointToRay(screenPosition);
                var raycastResult = await m_ObjectPicker.PickAsync(ray);
                if (raycastResult.HasIntersected)
                {
                    m_IsTeleporting = true;

                    StartCoroutine(TeleportCoroutine(
                        currentCamera.transform.position,
                        raycastResult.Point,
                        raycastResult.Normal.normalized));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        IEnumerator TeleportCoroutine(Vector3 startPosition, Vector3 targetPosition, Vector3 normal)
        {
            var forward = targetPosition - startPosition;
            forward.y = 0f;
            var offsetTargetPosition = targetPosition -
                m_ArrivalOffsetRelative * forward.normalized;
            offsetTargetPosition += Vector3.Angle(Vector3.up, normal) < m_AngleThreshold ? m_ArrivalOffsetRelative * Vector3.up : Vector3.zero;

            var cameraTransform = m_CameraProvider.Camera.transform;
            var startRotation = cameraTransform.rotation;
            Quaternion targetRotation;
            if(Vector3.Dot(forward, offsetTargetPosition-startPosition) < 0) // this happens when offset target position is behind the camera
            {
                targetRotation = Quaternion.LookRotation(startPosition - offsetTargetPosition);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(offsetTargetPosition - startPosition);
            }
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

            var t = 0f;
            while (t < m_LerpTime)
            {
                var evaluatedValue = m_SmoothCurve.Evaluate(t / m_LerpTime);
                var position = Vector3.Lerp(startPosition, offsetTargetPosition, evaluatedValue);
                var rotation = Quaternion.Lerp(startRotation, targetRotation, evaluatedValue);

                m_CameraController.ResetTo(position, rotation.eulerAngles, targetPosition);

                t += Time.deltaTime;
                yield return null;
            }

            m_CameraController.ResetTo(offsetTargetPosition, targetRotation.eulerAngles, targetPosition);

            m_IsTeleporting = false;
        }

        void StartCheck(InputAction.CallbackContext context)
        {
            m_IsValidStart = !EventSystem.current.IsPointerOverGameObject(-1);
        }

        void TryZoom(InputAction.CallbackContext context)
        {
            m_CameraController.OnZoom(context);
        }

        void TryPanStart(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnPanStart(context);
        }

        void TryPan(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnPan(context);
        }

        void TryOrbit(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnOrbit(context);
        }

        void TryWorldOrbit(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnWorldOrbit(context);
        }

        void TryPanStartGesture(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnPanGestureStarted(context);
        }

        void TryPanGesture(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnPanGesture(context);
        }

        void TryOrbitGesture(InputAction.CallbackContext context)
        {
            if (m_IsValidStart)
                m_CameraController.OnOrbit(context);
        }
    }
}
