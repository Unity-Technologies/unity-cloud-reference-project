using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Manipulator which monitors Press, Hold and Release events in order to drag visuals.
    /// </summary>
    public class Draggable : Clickable
    {
        internal static readonly PropertyInfo handledByDraggableProp = typeof(PointerMoveEvent).GetProperty("isHandledByDraggable", BindingFlags.NonPublic | BindingFlags.Instance);

        readonly Action<Draggable> m_DownHandler;

        readonly Action<Draggable> m_DragHandler;

        readonly Action<Draggable> m_UpHandler;

        bool m_IsDown;

        Vector2 m_LastPos = Vector2.zero;

        /// <summary>
        /// Construct a Draggable manipulator.
        /// </summary>
        /// <param name="clickHandler">A callback invoked when a <see cref="ClickEvent"/> has been received.</param>
        /// <param name="dragHandler">A callback invoked during dragging state.</param>
        /// <param name="upHandler">A callback invoked when a <see cref="PointerUpEvent"/> has been received.</param>
        /// <param name="downHandler">A callback invoked when a <see cref="PointerDownEvent"/> has been received.</param>
        public Draggable(Action clickHandler, Action<Draggable> dragHandler, Action<Draggable> upHandler, Action<Draggable> downHandler = null)
            : base(clickHandler)
        {
            m_DragHandler = dragHandler;
            m_UpHandler = upHandler;
            m_DownHandler = downHandler;
        }

        /// <summary>
        /// The delta position between the last frame and the current one.
        /// </summary>
        internal Vector2 deltaPos { get; set; } = Vector2.zero;

        /// <summary>
        /// The local position received from the imGui native event.
        /// </summary>
        internal Vector2 localPosition { get; set; }

        /// <summary>
        /// The world position received from the imGui native event.
        /// </summary>
        internal Vector2 position { get; set; }

        /// <summary>
        /// Has the pointer moved since the last <see cref="PointerDownEvent"/>.
        /// </summary>
        internal bool hasMoved { get; set; }

        /// <summary>
        /// Cancel the drag operation.
        /// </summary>
        public void Cancel()
        {
            if (active)
                target?.Blur();
        }

        /// <summary>
        /// This method processes the down event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        /// <param name="pointerId"> The pointer id of the pointer.</param>
        protected override void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId)
        {
            deltaPos = Vector2.zero;
            this.localPosition = localPosition;
            position = (evt is PointerDownEvent e) ? e.position : ((MouseDownEvent)evt).mousePosition;
            m_LastPos = position;
            m_IsDown = true;
            hasMoved = false;

            m_DownHandler?.Invoke(this);
            base.ProcessDownEvent(evt, localPosition, pointerId);
        }

        /// <summary>
        /// This method processes the up event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        /// <param name="pointerId"> The pointer id of the pointer.</param>
        protected override void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId)
        {
            m_IsDown = false;
            deltaPos = Vector2.zero;
            this.localPosition = localPosition;
            position = (evt is PointerUpEvent e) ? e.position : ((MouseUpEvent)evt).mousePosition;

            m_UpHandler?.Invoke(this);
            base.ProcessUpEvent(evt, localPosition, pointerId);
        }

        /// <summary>
        /// This method processes the move event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        protected override void ProcessMoveEvent(EventBase evt, Vector2 localPosition)
        {
            if (m_IsDown)
            {
                this.localPosition = localPosition;
                position = (evt is PointerMoveEvent e) ? e.position : ((MouseMoveEvent)evt).mousePosition;
                deltaPos = position - m_LastPos;
                m_LastPos = position;

                if (evt is PointerMoveEvent pointerMoveEvent)
                {
                    if (pointerMoveEvent.pointerId != PointerId.mousePointerId)
                        handledByDraggableProp.SetValue(pointerMoveEvent, true);
                }

                m_DragHandler?.Invoke(this);
                hasMoved = true;
            }

            base.ProcessMoveEvent(evt, localPosition);
        }
    }
}
