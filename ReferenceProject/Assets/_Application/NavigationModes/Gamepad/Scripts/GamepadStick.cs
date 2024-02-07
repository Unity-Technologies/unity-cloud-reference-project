using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Application
{
    public class GamepadStick
    {
        readonly VisualElement m_GamepadBackground;
        readonly VisualElement m_GamepadHandle;
        Vector2 m_GamepadPointerDownPosition;
        Vector2 m_GamepadDelta; // Between -1 and 1
        Vector2 m_PreviousGamepadDelta = Vector2.zero;
        readonly bool m_IsRightStick;
        
        // InputAsset
        readonly Gamepad m_GamepadInput;

        public GamepadStick(VisualElement root, string backgroundName, string handleName, Gamepad gamepadInput, bool isRightStick)
        {
            m_GamepadBackground = root.Q(backgroundName);
            m_GamepadHandle = root.Q(handleName);
            m_GamepadHandle.RegisterCallback<PointerDownEvent>(OnPointerDown);
            m_GamepadHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            m_GamepadHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            m_GamepadInput = gamepadInput;
            m_IsRightStick = isRightStick;
        }
        
        void OnPointerDown(PointerDownEvent e)
        {
            m_GamepadHandle.CapturePointer(e.pointerId);
            m_GamepadPointerDownPosition = e.position;
        }

        void OnPointerUp(PointerUpEvent e)
        {
            m_GamepadHandle.ReleasePointer(e.pointerId);
            m_GamepadHandle.transform.position = Vector3.zero;
            m_GamepadDelta = Vector2.zero;
        }

        void OnPointerMove(PointerMoveEvent e)
        {
            if (!m_GamepadHandle.HasPointerCapture(e.pointerId))
                return;
            var pointerCurrentPosition = (Vector2) e.position;
            var pointerMaxDelta = (m_GamepadBackground.worldBound.size - m_GamepadHandle.worldBound.size) / 2;
            var pointerDelta = Clamp(pointerCurrentPosition - m_GamepadPointerDownPosition, -pointerMaxDelta, pointerMaxDelta);
            m_GamepadHandle.transform.position = pointerDelta;
            m_GamepadDelta = pointerDelta / pointerMaxDelta;
        }

        public void Update()
        {
            if (m_GamepadDelta != m_PreviousGamepadDelta && m_GamepadInput != null)
            {
                m_PreviousGamepadDelta = m_GamepadDelta;
                var value = new Vector2(m_GamepadDelta.x, -m_GamepadDelta.y);
                using (StateEvent.From(m_GamepadInput, out var eventPtr))
                {
                    if (m_IsRightStick)
                    {
                        m_GamepadInput.rightStick.WriteValueIntoEvent(value, eventPtr);
                    }
                    else
                    {
                        m_GamepadInput.leftStick.WriteValueIntoEvent(value, eventPtr);
                    }

                    UnityEngine.InputSystem.InputSystem.QueueEvent(eventPtr);
                }
            }
        }

        static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max) =>
            new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
    }
}
