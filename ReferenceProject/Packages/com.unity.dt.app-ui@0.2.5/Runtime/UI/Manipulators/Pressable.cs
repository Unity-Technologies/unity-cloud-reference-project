using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Pressable Manipulator, used on <see cref="Button"/> elements.
    /// </summary>
    public class Pressable : Manipulator
    {
        public event Action<EventBase> pressed;
        
        public bool active { get; private set; }

        Event m_MoveEvent;
        
        Touch m_TouchMoveEvent;
        
        Event m_UpEvent;

        Touch m_TouchUpEvent;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public Pressable()
        {
            m_MoveEvent = new Event { type = EventType.MouseMove };
            m_TouchMoveEvent = new Touch { phase = TouchPhase.Moved };
            m_UpEvent = new Event { type = EventType.MouseUp };
            m_TouchUpEvent = new Touch { phase = TouchPhase.Ended };
        }

        /// <summary>
        /// Invoke the Pressed event.
        /// </summary>
        /// <param name="evt">The base event to use to invoke the press.</param>
        internal void InvokePressed(EventBase evt) => Invoke(evt);

        void Invoke(EventBase evt)
        {
            pressed?.Invoke(evt);
        }

        /// <summary>
        /// Simulate a single click on the target element.
        /// </summary>
        /// <param name="evt">The base event to use to invoke the click.</param>
        internal void SimulateSingleClickInternal(EventBase evt)
        {
            if (Clickable.pseudoStateProperty != null && target != null)
            {
                var pseudoStates = (PseudoStates)(int)Clickable.pseudoStateProperty.GetValue(target);
                Clickable.pseudoStateProperty.SetValue(target, (int)(pseudoStates | PseudoStates.Active));
                target.schedule
                    .Execute(() =>
                        Clickable.pseudoStateProperty.SetValue(target,
                        (int)((PseudoStates)(int)Clickable.pseudoStateProperty.GetValue(target) & ~PseudoStates.Active)))
                    .ExecuteLater(16L);
            }
            InvokePressed(evt);
        }

        /// <summary>
        /// Force the active pseudo state on the target element.
        /// </summary>
        public void ForceActivePseudoState()
        {
            if (Clickable.pseudoStateProperty != null && target != null)
            {
                var pseudoStates = (PseudoStates)(int)Clickable.pseudoStateProperty.GetValue(target);
                Clickable.pseudoStateProperty.SetValue(target, (int)(pseudoStates | PseudoStates.Active));
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }
        
        void OnPointerDown(PointerDownEvent evt)
        {
            Activate(evt.pointerId);
        }
        
        void OnPointerMove(PointerMoveEvent evt)
        {
            var parent = target?.parent;
            if (parent == null)
                return;
            
            m_MoveEvent.mousePosition = evt.originalMousePosition;
            m_MoveEvent.delta = evt.deltaPosition;
            m_MoveEvent.button = evt.button;
            m_MoveEvent.modifiers = evt.modifiers;
            m_MoveEvent.pressure = evt.pressure;
            m_MoveEvent.clickCount = evt.clickCount;

            m_TouchMoveEvent.fingerId = evt.pointerId - PointerId.touchPointerIdBase;
            m_TouchMoveEvent.position = evt.position;
            m_TouchMoveEvent.deltaPosition = evt.deltaPosition;
            m_TouchMoveEvent.deltaTime = evt.deltaTime;
            m_TouchMoveEvent.tapCount = evt.clickCount;
            m_TouchMoveEvent.pressure = evt.pressure;
            m_TouchMoveEvent.azimuthAngle = evt.azimuthAngle;
            m_TouchMoveEvent.altitudeAngle = evt.altitudeAngle;
            m_TouchMoveEvent.radius = evt.radius.x;
            m_TouchMoveEvent.radiusVariance = evt.radiusVariance.x;
            
            using var e = evt.pointerId == PointerId.mousePointerId ? 
                PointerMoveEvent.GetPooled(m_MoveEvent) : 
                PointerMoveEvent.GetPooled(m_TouchMoveEvent, evt.modifiers);
            e.target = parent;
            parent.SendEvent(e);
        }
                
        void OnPointerUp(PointerUpEvent evt)
        {
            if (!active)
                return;
            
            InvokePressed(evt);
            Deactivate(evt.pointerId);
            
            var parent = target?.parent;
            if (parent == null)
                return;
            
            m_UpEvent.mousePosition = evt.originalMousePosition;
            m_UpEvent.delta = evt.deltaPosition;
            m_UpEvent.button = evt.button;
            m_UpEvent.modifiers = evt.modifiers;
            m_UpEvent.pressure = evt.pressure;
            m_UpEvent.clickCount = evt.clickCount;
            
            m_TouchUpEvent.fingerId = evt.pointerId - PointerId.touchPointerIdBase;
            m_TouchUpEvent.position = evt.position;
            m_TouchUpEvent.deltaPosition = evt.deltaPosition;
            m_TouchUpEvent.deltaTime = evt.deltaTime;
            m_TouchUpEvent.tapCount = evt.clickCount;
            m_TouchUpEvent.pressure = evt.pressure;
            m_TouchUpEvent.azimuthAngle = evt.azimuthAngle;
            m_TouchUpEvent.altitudeAngle = evt.altitudeAngle;
            m_TouchUpEvent.radius = evt.radius.x;
            m_TouchUpEvent.radiusVariance = evt.radiusVariance.x;
            
            using var e = evt.pointerId == PointerId.mousePointerId ? 
                PointerUpEvent.GetPooled(m_UpEvent) : 
                PointerUpEvent.GetPooled(m_TouchUpEvent, evt.modifiers);
            e.target = parent;
            parent.SendEvent(e);
        }
        
        void OnPointerCancel(PointerCancelEvent evt)
        {
            Deactivate(evt.pointerId);
        }
        
        void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            Deactivate(evt.pointerId);
        }

        void Activate(int pointerId)
        {
            target.CapturePointer(pointerId);
            ForceActivePseudoState();
            target.AddToClassList(Styles.activeUssClassName);
            active = true;
        }
        
        void Deactivate(int pointerId)
        {
            if (!active)
                return;
            
            active = false;
            target.ReleasePointer(pointerId);
            var pseudoStates = (PseudoStates)(int)Clickable.pseudoStateProperty.GetValue(target);
            Clickable.pseudoStateProperty.SetValue(target, (int)(pseudoStates & ~PseudoStates.Active));
            target.RemoveFromClassList(Styles.activeUssClassName);
        }
    }
}
