using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    struct TouchModel
    {
        internal struct ImplementationData
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
            ///     In the same scale as <see cref="TouchModel.Position" />, and caches the same value as
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

                if (HoverTargets == null)
                    HoverTargets = new List<GameObject>();
                else
                    HoverTargets.Clear();
            }
        }

        public int PointerId { get; }

        public TouchPhase SelectPhase
        {
            get => m_SelectPhase;
            set
            {
                if (m_SelectPhase != value)
                {
                    if (value == TouchPhase.Began)
                        SelectDelta |= ButtonDeltaState.Pressed;

                    if (value == TouchPhase.Ended || value == TouchPhase.Canceled)
                        SelectDelta |= ButtonDeltaState.Released;

                    m_SelectPhase = value;

                    ChangedThisFrame = true;
                }
            }
        }

        public ButtonDeltaState SelectDelta { get; private set; }

        public bool ChangedThisFrame { get; private set; }

        /// <summary>
        ///     The pixel-space position of the touch object.
        /// </summary>
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

        TouchPhase m_SelectPhase;
        Vector2 m_Position;
        ImplementationData m_ImplementationData;

        public TouchModel(int pointerId)
        {
            PointerId = pointerId;

            m_Position = DeltaPosition = Vector2.zero;

            m_SelectPhase = TouchPhase.Canceled;
            ChangedThisFrame = false;
            SelectDelta = ButtonDeltaState.NoChange;

            m_ImplementationData = new ImplementationData();
            m_ImplementationData.Reset();
        }

        public void Reset()
        {
            m_Position = DeltaPosition = Vector2.zero;
            ChangedThisFrame = false;
            SelectDelta = ButtonDeltaState.NoChange;
            m_ImplementationData.Reset();
        }

        public void OnFrameFinished()
        {
            DeltaPosition = Vector2.zero;
            SelectDelta = ButtonDeltaState.NoChange;
            ChangedThisFrame = false;
        }

        public void CopyTo(PointerEventData eventData)
        {
            eventData.pointerId = PointerId;
            eventData.position = Position;
            eventData.delta = ((SelectDelta & ButtonDeltaState.Pressed) != 0) ? Vector2.zero : DeltaPosition;

            eventData.pointerEnter = m_ImplementationData.PointerTarget;
            eventData.dragging = m_ImplementationData.IsDragging;
            eventData.clickTime = m_ImplementationData.PressedTime;
            eventData.pressPosition = m_ImplementationData.PressedPosition;
            eventData.pointerPressRaycast = m_ImplementationData.PressedRaycast;
            eventData.pointerPress = m_ImplementationData.PressedGameObject;
            eventData.rawPointerPress = m_ImplementationData.PressedGameObjectRaw;
            eventData.pointerDrag = m_ImplementationData.DraggedGameObject;

            eventData.hovered.Clear();
            eventData.hovered.AddRange(m_ImplementationData.HoverTargets);
        }

        public void CopyFrom(PointerEventData eventData)
        {
            m_ImplementationData.PointerTarget = eventData.pointerEnter;
            m_ImplementationData.IsDragging = eventData.dragging;
            m_ImplementationData.PressedTime = eventData.clickTime;
            m_ImplementationData.PressedPosition = eventData.pressPosition;
            m_ImplementationData.PressedRaycast = eventData.pointerPressRaycast;
            m_ImplementationData.PressedGameObject = eventData.pointerPress;
            m_ImplementationData.PressedGameObjectRaw = eventData.rawPointerPress;
            m_ImplementationData.DraggedGameObject = eventData.pointerDrag;

            m_ImplementationData.HoverTargets.Clear();
            m_ImplementationData.HoverTargets.AddRange(eventData.hovered);
        }
    }
}
