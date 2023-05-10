using System;
using Unity.ReferenceProject.AppCamera;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.Application
{
    public class FlyOrbitController : NavigationMode
    {
        const string k_MovingAction = "Moving Action";
        const string k_OrbitAction = "Orbit Action";
        const string k_WorldOrbitAction = "WorldOrbit Action";
        const string k_PanStart = "Pan Start";
        const string k_PanAction = "Pan Action";
        const string k_ZoomAction = "Zoom Action";
        const string k_PanGestureAction = "Pan Gesture Action";
        const string k_ZoomGestureAction = "Zoom Gesture Action";

        [SerializeField]
        CameraController m_CameraController;

        [SerializeField]
        InputActionAsset m_InputActionAsset;
        readonly Vector3 m_ResetCameraEuler = new(25, 0, 0);

        readonly Vector3 m_ResetCameraPosition = new(0, 5, -10);

        bool m_IsInputsEnabled;

        void Awake()
        {
            InitializeInputs();
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

        void OnDestroy()
        {
            ResetInputs();
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

            m_InputActionAsset[k_ZoomGestureAction].Enable();
            m_InputActionAsset[k_PanGestureAction].Enable();
        }

        void ResetInputs()
        {
            m_IsInputsEnabled = false;

            m_InputActionAsset[k_OrbitAction].Disable();
            m_InputActionAsset[k_WorldOrbitAction].Disable();
            m_InputActionAsset[k_PanStart].Disable();
            m_InputActionAsset[k_PanAction].Disable();
            m_InputActionAsset[k_ZoomAction].Disable();
            m_InputActionAsset[k_ZoomGestureAction].Disable();
            m_InputActionAsset[k_PanGestureAction].Disable();

            m_CameraController.MovingAction = null;
            m_InputActionAsset[k_OrbitAction].performed -= m_CameraController.OnOrbit;
            m_InputActionAsset[k_WorldOrbitAction].performed -= m_CameraController.OnWorldOrbit;
            m_InputActionAsset[k_PanStart].performed -= m_CameraController.OnPanStart;
            m_InputActionAsset[k_PanAction].performed -= m_CameraController.OnPan;
            m_InputActionAsset[k_ZoomAction].performed -= m_CameraController.OnZoom;

            m_InputActionAsset[k_PanGestureAction].started -= m_CameraController.OnPanGestureStarted;
            m_InputActionAsset[k_PanGestureAction].performed -= m_CameraController.OnPanGesture;
            m_InputActionAsset[k_ZoomGestureAction].started -= m_CameraController.OnZoomGestureStarted;
            m_InputActionAsset[k_ZoomGestureAction].performed -= m_CameraController.OnZoomGesture;
        }

        void ResetCameraTransform()
        {
            // Example position, can be set to any desired Transform with overload method ResetTo(new Transform)
            m_CameraController.ResetTo(m_ResetCameraPosition, m_ResetCameraEuler, Vector3.forward);
        }

        void UpdateMovement()
        {
            // Ignore movement if asset is disabled
            if (!m_InputActionAsset.enabled)
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
            m_CameraController.ResetTo(position, eulerAngles, Vector3.forward);
        }
    }
}
