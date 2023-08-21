using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.UIInputBlocker
{
    public class UIRayInputBlocker : MonoBehaviour
    {
        [SerializeField]
        string m_BackgroundName;

        Camera m_Camera;
        IUIInputBlockerEventsDispatcher m_Dispatcher;

        InputSystemUIInputModule m_InputSystemUIInputModule;

        readonly float m_ClickThresholdDistance = 10;
        Vector2 m_StartClickPosition;

        [Inject]
        public void Setup(IUIInputBlockerEventsDispatcher dispatcher, Camera camera)
        {
            m_Dispatcher = dispatcher;
            m_Camera = camera;
        }

        void OnEnable()
        {
            m_InputSystemUIInputModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            if (m_InputSystemUIInputModule)
            {
                m_InputSystemUIInputModule.leftClick.action.performed += OnClickPerformed;
            }
            else
            {
                Debug.LogWarning(
                    $"Can't find {nameof(InputSystemUIInputModule)}. {nameof(UIRayInputBlocker)} has been disabled!");
                enabled = false;
            }
        }

        void OnDisable()
        {
            if (m_InputSystemUIInputModule)
                m_InputSystemUIInputModule.leftClick.action.performed -= OnClickPerformed;
        }

        void OnClickPerformed(InputAction.CallbackContext obj)
        {
            var currentPosition = m_InputSystemUIInputModule.point.action.ReadValue<Vector2>();
            
            // We perform check only when mouse button has been released
            if (obj.action.IsPressed())
            {
                m_StartClickPosition = currentPosition;
                return; 
            }
            
            if (Vector2.Distance(m_StartClickPosition, currentPosition) > m_ClickThresholdDistance)
            {
                return;
            }
            
            if (m_Dispatcher != null && m_Dispatcher.IsListenersAvailable)
                PerformCheck(m_Camera);
        }

        void PerformCheck(Camera camera)
        {
            if (!camera)
            {
                Debug.LogWarning($"Camera is missing! {nameof(UIRayInputBlocker)} has been stopped.");
                return;
            }

            var screenPosition = m_InputSystemUIInputModule.input.mousePosition;

            var eventSystem = EventSystem.current;

            if (eventSystem == null)
            {
                Debug.LogError($"Scene doesn't contain EventSystem! {nameof(UIRayInputBlocker)} can't perform check.");
                return;
            }

            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(
                new PointerEventData(EventSystem.current)
                    { position = screenPosition, pointerCurrentRaycast = new RaycastResult() }, results);

            if (ShouldRayBeDispatched(results, screenPosition))
            {
                DispatchRay(screenPosition, camera);
            }
        }

        bool ShouldRayBeDispatched(List<RaycastResult> results, Vector2 screenPosition)
        {
            PanelEventHandler panelEventHandler = null;
            foreach (var result in results)
            {
                panelEventHandler = result.gameObject.GetComponent<PanelEventHandler>();
                if (panelEventHandler != null)
                    break;
            }

            //Nothing was hit, we should pass
            if (panelEventHandler == null)
            {
                return true;
            }

            // Check if we hit background, then we should pass
            var localPanelPosition =
                RemapScreenPosToRectPos(screenPosition, panelEventHandler.panel.visualTree.contentRect);

            var pickResult = panelEventHandler.panel.Pick(localPanelPosition);

            if (pickResult != null && !string.IsNullOrEmpty(m_BackgroundName) &&
                pickResult.name.Equals(m_BackgroundName))
            {
                return true;
            }

            // We hit something, so block ray
            return false;
        }

        /// <summary>
        ///     Remaps screen position to rect position proportionally
        /// </summary>
        Vector2 RemapScreenPosToRectPos(Vector2 mousePos, Rect rect)
        {
            mousePos.y = Screen.height - mousePos.y;
            var x = Mathf.InverseLerp(0, Screen.width, mousePos.x);
            var y = Mathf.InverseLerp(0, Screen.height, mousePos.y);

            return new Vector2(rect.width * x, rect.height * y);
        }

        void DispatchRay(Vector2 screenPosition, Camera camera)
        {
            if (camera == null)
                return;

            var ray = camera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));

            m_Dispatcher?.DispatchRay(ray);
        }
    }
}
