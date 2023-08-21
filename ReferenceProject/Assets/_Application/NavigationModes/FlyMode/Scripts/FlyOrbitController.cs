using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.ReferenceProject.AppCamera;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.UIInputBlocker;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

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

        readonly Vector3 m_ResetCameraEuler = new(25, 0, 0);
        readonly Vector3 m_ResetCameraPosition = new(0, 5, -10);

        bool m_IsInputsEnabled;
        bool m_IsTeleporting;
        float m_LastClickTime;

        // Keyboard and Mouse
        static readonly string k_MovingAction = "Moving Action";
        static readonly string k_OrbitAction = "Orbit Action";
        static readonly string k_WorldOrbitAction = "WorldOrbit Action";
        static readonly string k_PanStart = "Pan Start";
        static readonly string k_PanAction = "Pan Action";
        static readonly string k_ZoomAction = "Zoom Action";
        
        // Touch Gestures
        static readonly string k_PanGestureAction = "Pan Gesture Action";
        static readonly string k_ZoomGestureAction = "Zoom Gesture Action";
        static readonly string k_OrbitGestureAction = "Orbit Gesture Action";
        
        static readonly float k_DoubleClickTime = 0.3f;

        Task m_PickTask;
        IObjectPicker m_ObjectPicker;
        IUIInputBlockerEvents m_ClickEventDispatcher;
        Camera m_StreamingCamera;

        [Inject]
        void Setup(IObjectPicker objectPicker, IUIInputBlockerEvents clickEventDispatcher, Camera streamingCamera)
        {
            m_ObjectPicker = objectPicker;
            m_ClickEventDispatcher = clickEventDispatcher;
            m_StreamingCamera = streamingCamera;
        }

        void Awake()
        {
            InitializeInputs();
        }

        void OnDestroy()
        {
            ResetInputs();
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

            m_IsInputsEnabled = true;

            // Keyboard and Mouse
            m_CameraController.MovingAction = m_InputActionAsset[k_MovingAction];
            m_InputActionAsset[k_OrbitAction].performed += m_CameraController.OnOrbit;
            m_InputActionAsset[k_WorldOrbitAction].performed += m_CameraController.OnWorldOrbit;
            m_InputActionAsset[k_PanStart].performed += m_CameraController.OnPanStart;
            m_InputActionAsset[k_PanAction].performed += m_CameraController.OnPan;
            m_InputActionAsset[k_ZoomAction].performed += m_CameraController.OnZoom;
            
            if (m_ClickEventDispatcher != null)
            {
                m_ClickEventDispatcher.OnDispatchRay += PerformPick;
            }

            m_InputActionAsset[k_OrbitAction].Enable();
            m_InputActionAsset[k_WorldOrbitAction].Enable();
            m_InputActionAsset[k_PanStart].Enable();
            m_InputActionAsset[k_PanAction].Enable();
            m_InputActionAsset[k_ZoomAction].Enable();

            // Touch Gestures
            m_InputActionAsset[k_PanGestureAction].started += m_CameraController.OnPanGestureStarted;
            m_InputActionAsset[k_PanGestureAction].performed += m_CameraController.OnPanGesture;
            m_InputActionAsset[k_ZoomGestureAction].started += m_CameraController.OnZoomGestureStarted;
            m_InputActionAsset[k_ZoomGestureAction].performed += m_CameraController.OnZoomGesture;
            m_InputActionAsset[k_OrbitGestureAction].performed += m_CameraController.OnOrbit;

            m_InputActionAsset[k_ZoomGestureAction].Enable();
            m_InputActionAsset[k_PanGestureAction].Enable();
            m_InputActionAsset[k_OrbitGestureAction].Enable();
        }

        void ResetInputs()
        {
            m_IsInputsEnabled = false;
            m_CameraController.ForceStop();

            // Keyboard and Mouse
            m_InputActionAsset[k_OrbitAction].Disable();
            m_InputActionAsset[k_WorldOrbitAction].Disable();
            m_InputActionAsset[k_PanStart].Disable();
            m_InputActionAsset[k_PanAction].Disable();
            m_InputActionAsset[k_ZoomAction].Disable();
            
            // Touch Gestures
            m_InputActionAsset[k_ZoomGestureAction].Disable();
            m_InputActionAsset[k_PanGestureAction].Disable();
            m_InputActionAsset[k_OrbitGestureAction].Disable();
            
            if (m_ClickEventDispatcher != null)
            {
                m_ClickEventDispatcher.OnDispatchRay -= PerformPick;
            }

            // Keyboard and Mouse
            m_CameraController.MovingAction = null;
            m_InputActionAsset[k_OrbitAction].performed -= m_CameraController.OnOrbit;
            m_InputActionAsset[k_WorldOrbitAction].performed -= m_CameraController.OnWorldOrbit;
            m_InputActionAsset[k_PanStart].performed -= m_CameraController.OnPanStart;
            m_InputActionAsset[k_PanAction].performed -= m_CameraController.OnPan;
            m_InputActionAsset[k_ZoomAction].performed -= m_CameraController.OnZoom;

            // Touch Gestures
            m_InputActionAsset[k_PanGestureAction].started -= m_CameraController.OnPanGestureStarted;
            m_InputActionAsset[k_PanGestureAction].performed -= m_CameraController.OnPanGesture;
            m_InputActionAsset[k_ZoomGestureAction].started -= m_CameraController.OnZoomGestureStarted;
            m_InputActionAsset[k_ZoomGestureAction].performed -= m_CameraController.OnZoomGesture;
            m_InputActionAsset[k_OrbitGestureAction].performed -= m_CameraController.OnOrbit;
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

            m_CameraController.UpdateMovingAction();
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

        void PerformPick(Ray ray)
        {
            if (m_IsTeleporting)
                return;

            if (Time.time - m_LastClickTime < k_DoubleClickTime)
            {
                if (m_PickTask?.IsCompleted ?? true)
                {
                    m_PickTask = PickFromRayAsync(ray);
                }
            }
            else
            {
                m_LastClickTime = Time.time;
            }
        }

        async Task PickFromRayAsync(Ray ray)
        {
            try
            {
                var raycastResult = await m_ObjectPicker.RaycastAsync(ray);
                if (raycastResult.HasIntersected)
                {
                    m_IsTeleporting = true;

                    var normal = Vector3.up; // Default to up until we have a hit normal
                    var source = m_StreamingCamera.transform.position;
                    var target = raycastResult.Point +
                        m_ArrivalOffsetFixed +
                        m_ArrivalOffsetNormal * normal +
                        m_ArrivalOffsetRelative * (source - raycastResult.Point).normalized;

                   StartCoroutine(TeleportCoroutine(source, target));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        IEnumerator TeleportCoroutine(Vector3 startPosition, Vector3 targetPosition)
        {
            var cameraTransform = m_StreamingCamera.transform;

            var startRotation = cameraTransform.rotation;
            var targetRotation = Quaternion.LookRotation(targetPosition - startPosition);

            var forwardVector = targetPosition + (targetPosition - startPosition).normalized;

            var t = 0f;
            while(t < m_LerpTime)
            {
                var evaluatedValue = m_SmoothCurve.Evaluate(t / m_LerpTime);
                var position = Vector3.Lerp(startPosition, targetPosition, evaluatedValue);
                var rotation = Quaternion.Lerp(startRotation, targetRotation, evaluatedValue);
        
                m_CameraController.ResetTo(position, rotation.eulerAngles, forwardVector);
        
                t += Time.deltaTime;
                yield return null;
            }

            m_CameraController.ResetTo(targetPosition, targetRotation.eulerAngles, forwardVector);

            m_IsTeleporting = false;
        }
    }
}
