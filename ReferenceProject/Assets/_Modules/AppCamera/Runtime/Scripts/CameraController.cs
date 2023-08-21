using System;
using System.Collections;
using Unity.ReferenceProject.Gestures;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.AppCamera
{
    public class CameraController : MonoBehaviour
    {
        public enum OrbitType
        {
            None = -1,
            WorldOrbit = 0,
            OrbitAtPoint = 1,
        }

        [SerializeField]
        CameraProxy m_Camera;

        [SerializeField]
        UINavigationControllerSettings m_UINavigationControllerSettings;
        float m_GestureCameraStartPosition;

        uint m_InputSkipper;
        Vector3 m_LastMovingAction;
        bool m_MoveEnabled = true;
        
        Coroutine m_PanGestureCoroutine;
        bool m_PanGestureInProgress;
        Coroutine m_ZoomGestureCoroutine;
        bool m_ZoomGestureInProgress;
        
        readonly DeltaCalculator m_PanDelta = new ();
        readonly DeltaCalculator m_WorldOrbitDelta = new ();
        readonly DeltaCalculator m_ZoomDelta = new ();

        public InputAction MovingAction { get; set; }

        public void Reset()
        {
            m_ZoomDelta.Reset();
            m_PanDelta.Reset();
            m_WorldOrbitDelta.Reset();
            m_InputSkipper = 0;
            m_ZoomGestureInProgress = false;
            m_PanGestureInProgress = false;
            m_GestureCameraStartPosition = 0;
            m_MoveEnabled = true;
            m_LastMovingAction = Vector3.zero;
        }

        void Update()
        {
            m_ZoomDelta.Reset();
            m_PanDelta.Reset();
            m_WorldOrbitDelta.Reset();
            m_InputSkipper++;
        }

        public void ResetTo(Transform newTransform)
        {
            Reset();
            m_Camera.TransformTo(newTransform);
        }

        public void ResetTo(Vector3 newPosition, Vector3 newEulerAngle, Vector3 newForward)
        {
            Reset();
            m_Camera.ApplyTransform(newPosition, newEulerAngle, newForward);
        }

        public void UpdateMovingAction()
        {
            if (MovingAction == null)
                return;

            if (Time.unscaledDeltaTime <= m_UINavigationControllerSettings.InputLagCutoffThreshold)
            {
                ToggleMovingAction(Time.unscaledDeltaTime <= m_UINavigationControllerSettings.InputLagSkipThreshold);

                if (!m_MoveEnabled)
                {
                    return;
                }

                var val = MovingAction.ReadValue<Vector3>();
                if (val != m_LastMovingAction)
                {
                    m_LastMovingAction = val;
                    m_Camera.MoveInLocalDirection(val, LookAtConstraint.Follow);
                }
            }
            else
            {
                MovingAction.Disable();
                m_Camera.ForceStop();
                m_LastMovingAction = Vector3.zero;
                m_Camera.MoveInLocalDirection(Vector3.zero, LookAtConstraint.Follow);
            }
        }

        void ToggleMovingAction(bool value)
        {
            switch (value)
            {
                case true when !MovingAction.enabled:
                    MovingAction.Enable();
                    break;
                case false when MovingAction.enabled:
                    MovingAction.Disable();
                    break;
            }
        }

        public void SetFocusOnPoint(Vector3 focusPoint)
        {
            m_Camera.FocusOnPoint(focusPoint);
        }

        void Zoom(Vector2 delta)
        {
            m_Camera.MoveOnLookAtAxis(delta.y);
        }

        void Pan(Vector2 delta)
        {
            m_Camera.Pan(delta);
        }

        void Orbit(Vector2 delta, OrbitType orbitType)
        {
            switch (orbitType)
            {
                case OrbitType.WorldOrbit:
                    m_Camera.Rotate(new Vector2(delta.y, delta.x));
                    break;

                case OrbitType.OrbitAtPoint:
                    m_Camera.OrbitAroundLookAt(new Vector2(delta.y, delta.x));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(orbitType));
            }
        }

        public void OnOrbit(InputAction.CallbackContext context)
        {
            if (m_ZoomGestureInProgress || m_PanGestureInProgress)
                return;
            
            var readValue = context.ReadValue<Vector2>();

            m_WorldOrbitDelta.SetNewFrameDelta(readValue);
            var delta = m_WorldOrbitDelta.delta;
            var worldVector = new Vector2(delta.x, -delta.y);
            Orbit(worldVector, OrbitType.OrbitAtPoint);
        }

        public void OnPanGesture(InputAction.CallbackContext context)
        {
            if (m_ZoomGestureInProgress || !CheckTreatInput(context))
                return;

            var interaction = context.interaction as TwoFingerDragGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                var dragGesture = interaction?.currentGesture as TwoFingerDragGesture;
                if (m_PanGestureCoroutine != null)
                    StopCoroutine(m_PanGestureCoroutine);
                m_PanGestureCoroutine = StartCoroutine(StopPanGesture());
                m_PanDelta.SetNewFrameDelta(dragGesture.Delta);
                var delta = dragGesture.Delta * -Vector2.one;
                Pan(delta);
            }
        }

        public void OnZoomGesture(InputAction.CallbackContext context)
        {
            if (m_PanGestureInProgress || !CheckTreatInput(context))
                return;

            var interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                var pinchGesture = interaction?.currentGesture as PinchGesture;
                if (m_ZoomGestureCoroutine != null)
                    StopCoroutine(m_ZoomGestureCoroutine);
                m_ZoomGestureCoroutine = StartCoroutine(StopZoomGesture());

                var distance = m_GestureCameraStartPosition - (m_GestureCameraStartPosition) * ((pinchGesture.gap - pinchGesture.startGap) / Screen.height);
                if (distance > m_GestureCameraStartPosition)
                {
                    // Double zoom out ratio
                    distance += distance - m_GestureCameraStartPosition;
                }

                m_Camera.SetDistanceFromLookAt(distance);
            }
        }

        IEnumerator StopZoomGesture()
        {
            yield return new WaitForSeconds(0.2f);
            OnZoomGestureFinished(null);
        }

        IEnumerator StopPanGesture()
        {
            yield return new WaitForSeconds(0.2f);
            OnPanGestureFinished(null);
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            if (CheckTreatInput(context))
            {
                m_ZoomDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                Zoom(m_ZoomDelta.delta);
            }
        }

        public void OnPan(InputAction.CallbackContext context)
        {
            if (CheckTreatInput(context))
            {
                m_PanDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
                var delta = m_PanDelta.delta * -Vector2.one;

                Pan(delta);
            }
        }

        public void OnPanStart(InputAction.CallbackContext context)
        {
            if (CheckTreatInput(context) && context.control.IsPressed())
            {
                var pos = Pointer.current.position.ReadValue();
                m_Camera.PanStart(pos);
            }
        }

        public void OnWorldOrbit(InputAction.CallbackContext context)
        {
            m_WorldOrbitDelta.SetNewFrameDelta(context.ReadValue<Vector2>());
            var delta = m_WorldOrbitDelta.delta;
            var worldVector = new Vector2(delta.x, -delta.y);

            Orbit(worldVector, OrbitType.WorldOrbit);
        }

        bool CheckTreatInput(double deltaTime)
        {
            return
                deltaTime <= m_UINavigationControllerSettings.InputLagSkipThreshold ||
                ((deltaTime <= m_UINavigationControllerSettings.InputLagCutoffThreshold) && (m_InputSkipper % m_UINavigationControllerSettings.InputLagSkipAmount == 0));
        }

        bool CheckTreatInput(InputAction.CallbackContext context)
        {
            var deltaTime = Time.realtimeSinceStartup - context.time;
            return CheckTreatInput(deltaTime);
        }

        public void OnZoomGestureStarted(InputAction.CallbackContext context)
        {
            if (m_PanGestureInProgress || m_ZoomGestureInProgress)
                return;

            var interaction = context.interaction as PinchGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                var pinchGesture = interaction?.currentGesture as PinchGesture;
                m_ZoomGestureInProgress = true;
                m_GestureCameraStartPosition = m_Camera.GetDistanceFromLookAt();
                pinchGesture.onFinished += OnZoomGestureFinished;
            }
        }

        void OnZoomGestureFinished(PinchGesture pinchGesture)
        {
            m_ZoomGestureInProgress = false;
        }

        public void OnPanGestureStarted(InputAction.CallbackContext context)
        {
            if (m_ZoomGestureInProgress || m_PanGestureInProgress)
                return;

            var interaction = context.interaction as TwoFingerDragGestureInteraction;
            if (interaction?.currentGesture != null)
            {
                var dragGesture = interaction?.currentGesture as TwoFingerDragGesture;
                m_PanGestureInProgress = true;
                dragGesture.onFinished += OnPanGestureFinished;

                var pos = Pointer.current.position.ReadValue();
                m_Camera.PanStart(pos);
            }
        }

        void OnPanGestureFinished(TwoFingerDragGesture dragGesture)
        {
            m_PanGestureInProgress = false;
        }

        public void ForceStop()
        {
            m_Camera.ForceStop();
        }
    }
}
