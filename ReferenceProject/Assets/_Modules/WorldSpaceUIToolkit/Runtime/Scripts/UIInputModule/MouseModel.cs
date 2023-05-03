using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    /// <summary>
    ///     Represents the state of a single mouse button within the Unity UI system. Keeps track of various book-keeping
    ///     regarding clicks, drags, and presses.
    ///     Can be converted to and from PointerEventData for sending into Unity UI.
    /// </summary>
    public struct MouseButtonModel
    {
        internal struct ImplementationData
        {
            /// <summary>
            ///     Used to cache whether or not the current mouse button is being dragged.
            /// </summary>
            /// <seealso cref="PointerEventData.dragging" />
            public bool IsDragging { get; set; }

            /// <summary>
            ///     Used to cache the last time this button was pressed.
            /// </summary>
            /// <seealso cref="PointerEventData.clickTime" />
            public float PressedTime { get; set; }

            /// <summary>
            ///     The position on the screen that this button was last pressed.
            ///     In the same scale as <see cref="MouseModel.Position" />, and caches the same value as
            ///     <see cref="PointerEventData.pressPosition" />.
            /// </summary>
            /// <seealso cref="PointerEventData.pressPosition" />
            public Vector2 PressedPosition { get; set; }

            /// <summary>
            ///     The Raycast data from the time it was pressed.
            /// </summary>
            /// <seealso cref="PointerEventData.pointerPressRaycast" />
            public RaycastResult PressedRaycast { get; set; }

            /// <summary>
            ///     The last GameObject pressed on that can handle press or click events.
            /// </summary>
            /// <seealso cref="PointerEventData.pointerPress" />
            public GameObject PressedGameObject { get; set; }

            /// <summary>
            ///     The last GameObject pressed on regardless of whether it can handle events or not.
            /// </summary>
            /// <seealso cref="PointerEventData.rawPointerPress" />
            public GameObject PressedGameObjectRaw { get; set; }

            /// <summary>
            ///     The GameObject currently being dragged if any.
            /// </summary>
            /// <seealso cref="PointerEventData.pointerDrag" />
            public GameObject DraggedGameObject { get; set; }

            /// <summary>
            ///     Resets this object to it's default, unused state.
            /// </summary>
            public void Reset()
            {
                IsDragging = false;
                PressedTime = 0f;
                PressedPosition = Vector2.zero;
                PressedRaycast = new RaycastResult();
                PressedGameObject = PressedGameObjectRaw = DraggedGameObject = null;
            }
        }

        /// <summary>
        ///     Used to store the current binary state of the button. When set, will also track the changes between calls of
        ///     <see cref="OnFrameFinished" /> in <see cref="LastFrameDelta" />.
        /// </summary>
        public bool IsDown
        {
            get => m_IsDown;
            set
            {
                if (m_IsDown != value)
                {
                    m_IsDown = value;
                    LastFrameDelta |= value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released;
                }
            }
        }

        /// <summary>
        ///     A set of flags to identify the changes that have occurred between calls of <see cref="OnFrameFinished" />.
        /// </summary>
        internal ButtonDeltaState LastFrameDelta { get; private set; }

        /// <summary>
        ///     Resets this object to it's default, unused state.
        /// </summary>
        public void Reset()
        {
            LastFrameDelta = ButtonDeltaState.NoChange;
            m_IsDown = false;

            m_ImplementationData.Reset();
        }

        /// <summary>
        ///     Call this on each frame in order to reset properties that detect whether or not a certain condition was met this
        ///     frame.
        /// </summary>
        public void OnFrameFinished() => LastFrameDelta = ButtonDeltaState.NoChange;

        /// <summary>
        ///     Fills a <see cref="PointerEventData" /> with this mouse button's internally cached values.
        /// </summary>
        /// <param name="eventData">These objects are used to send data through the Unity UI (UGUI) system.</param>
        public void CopyTo(PointerEventData eventData)
        {
            eventData.dragging = m_ImplementationData.IsDragging;
            eventData.clickTime = m_ImplementationData.PressedTime;
            eventData.pressPosition = m_ImplementationData.PressedPosition;
            eventData.pointerPressRaycast = m_ImplementationData.PressedRaycast;
            eventData.pointerPress = m_ImplementationData.PressedGameObject;
            eventData.rawPointerPress = m_ImplementationData.PressedGameObjectRaw;
            eventData.pointerDrag = m_ImplementationData.DraggedGameObject;
        }

        /// <summary>
        ///     Fills this object with the values from a <see cref="PointerEventData" />.
        /// </summary>
        /// <param name="eventData">These objects are used to send data through the Unity UI (UGUI) system.</param>
        public void CopyFrom(PointerEventData eventData)
        {
            m_ImplementationData.IsDragging = eventData.dragging;
            m_ImplementationData.PressedTime = eventData.clickTime;
            m_ImplementationData.PressedPosition = eventData.pressPosition;
            m_ImplementationData.PressedRaycast = eventData.pointerPressRaycast;
            m_ImplementationData.PressedGameObject = eventData.pointerPress;
            m_ImplementationData.PressedGameObjectRaw = eventData.rawPointerPress;
            m_ImplementationData.DraggedGameObject = eventData.pointerDrag;
        }

        bool m_IsDown;
        ImplementationData m_ImplementationData;
    }

    struct MouseModel
    {
        internal struct InternalData
        {
            /// <summary>
            ///     This tracks the current GUI targets being hovered over.
            /// </summary>
            /// <seealso cref="PointerEventData.hovered" />
            public List<GameObject> HoverTargets { get; set; }

            /// <summary>
            ///     Tracks the current enter/exit target being hovered over at any given moment.
            /// </summary>
            /// <seealso cref="PointerEventData.pointerEnter" />
            public GameObject PointerTarget { get; set; }

            public void Reset()
            {
                PointerTarget = null;

                if (HoverTargets == null)
                    HoverTargets = new List<GameObject>();
                else
                    HoverTargets.Clear();
            }
        }

        /// <summary>
        ///     An Id representing a unique pointer.
        /// </summary>
        public int PointerId { get; }

        /// <summary>
        ///     A boolean value representing whether any mouse data has changed this frame, meaning that events should be
        ///     processed.
        /// </summary>
        /// <remarks>
        ///     This only checks for changes in mouse state (<see cref="Position" />, <see cref="LeftButton" />,
        ///     <see cref="RightButton" />, <see cref="MiddleButton" />, or <see cref="scrollPosition" />).
        /// </remarks>
        public bool ChangedThisFrame { get; private set; }

        Vector2 m_Position;

        public Vector2 Position
        {
            get => m_Position;
            set
            {
                if (m_Position != value)
                {
                    DeltaPosition = value - m_Position;
                    m_Position = value;
                    ChangedThisFrame = true;
                }
            }
        }

        /// <summary>
        ///     The pixel-space change in <see cref="Position" /> since the last call to <see cref="OnFrameFinished" />.
        /// </summary>
        public Vector2 DeltaPosition { get; private set; }

        Vector2 m_ScrollDelta;

        /// <summary>
        ///     The amount of scroll since the last call to <see cref="OnFrameFinished" />.
        /// </summary>
        public Vector2 ScrollDelta
        {
            get => m_ScrollDelta;
            set
            {
                if (m_ScrollDelta != value)
                {
                    m_ScrollDelta = value;
                    ChangedThisFrame = true;
                }
            }
        }

        MouseButtonModel m_LeftButton;

        /// <summary>
        ///     Cached data and button state representing a left mouse button on a mouse.
        ///     Used by Unity UI (UGUI) to keep track of persistent click, press, and drag states.
        /// </summary>
        public MouseButtonModel LeftButton
        {
            get => m_LeftButton;
            set
            {
                ChangedThisFrame |= (value.LastFrameDelta != ButtonDeltaState.NoChange);
                m_LeftButton = value;
            }
        }

        /// <summary>
        ///     Shorthand to set the pressed state of the left mouse button.
        /// </summary>
        public bool LeftButtonPressed
        {
            set
            {
                ChangedThisFrame |= m_LeftButton.IsDown != value;
                m_LeftButton.IsDown = value;
            }
        }

        MouseButtonModel m_RightButton;

        /// <summary>
        ///     Cached data and button state representing a right mouse button on a mouse.
        ///     Used by Unity UI (UGUI) to keep track of persistent click, press, and drag states.
        /// </summary>
        public MouseButtonModel RightButton
        {
            get => m_RightButton;
            set
            {
                ChangedThisFrame |= (value.LastFrameDelta != ButtonDeltaState.NoChange);
                m_RightButton = value;
            }
        }

        /// <summary>
        ///     Shorthand to set the pressed state of the right mouse button.
        /// </summary>
        public bool RightButtonPressed
        {
            set
            {
                ChangedThisFrame |= m_RightButton.IsDown != value;
                m_RightButton.IsDown = value;
            }
        }

        MouseButtonModel m_MiddleButton;

        /// <summary>
        ///     Cached data and button state representing a middle mouse button on a mouse.
        ///     Used by Unity UI (UGUI) to keep track of persistent click, press, and drag states.
        /// </summary>
        public MouseButtonModel MiddleButton
        {
            get => m_MiddleButton;
            set
            {
                ChangedThisFrame |= (value.LastFrameDelta != ButtonDeltaState.NoChange);
                m_MiddleButton = value;
            }
        }

        /// <summary>
        ///     Shorthand to set the pressed state of the middle mouse button.
        /// </summary>
        public bool MiddleButtonPressed
        {
            set
            {
                ChangedThisFrame |= m_MiddleButton.IsDown != value;
                m_MiddleButton.IsDown = value;
            }
        }

        InternalData m_InternalData;

        public MouseModel(int pointerId)
        {
            PointerId = pointerId;
            ChangedThisFrame = false;
            m_Position = Vector2.zero;
            DeltaPosition = Vector2.zero;
            m_ScrollDelta = Vector2.zero;

            m_LeftButton = new MouseButtonModel();
            m_RightButton = new MouseButtonModel();
            m_MiddleButton = new MouseButtonModel();
            m_LeftButton.Reset();
            m_RightButton.Reset();
            m_MiddleButton.Reset();

            m_InternalData = new InternalData();
            m_InternalData.Reset();
        }

        /// <summary>
        ///     Call this at the end of polling for per-frame changes.  This resets delta values, such as
        ///     <see cref="DeltaPosition" />, <see cref="ScrollDelta" />, and <see cref="MouseButtonModel.LastFrameDelta" />.
        /// </summary>
        public void OnFrameFinished()
        {
            ChangedThisFrame = false;
            DeltaPosition = Vector2.zero;
            m_ScrollDelta = Vector2.zero;
            m_LeftButton.OnFrameFinished();
            m_RightButton.OnFrameFinished();
            m_MiddleButton.OnFrameFinished();
        }

        public void CopyTo(PointerEventData eventData)
        {
            eventData.pointerId = PointerId;
            eventData.position = Position;
            eventData.delta = DeltaPosition;
            eventData.scrollDelta = ScrollDelta;

            eventData.pointerEnter = m_InternalData.PointerTarget;
            eventData.hovered.Clear();
            eventData.hovered.AddRange(m_InternalData.HoverTargets);

            // This is unset in legacy systems and can safely assumed to stay true.
            eventData.useDragThreshold = true;
        }

        public void CopyFrom(PointerEventData eventData)
        {
            var hoverTargets = m_InternalData.HoverTargets;
            m_InternalData.HoverTargets.Clear();
            m_InternalData.HoverTargets.AddRange(eventData.hovered);
            m_InternalData.HoverTargets = hoverTargets;
            m_InternalData.PointerTarget = eventData.pointerEnter;
        }
    }
}
