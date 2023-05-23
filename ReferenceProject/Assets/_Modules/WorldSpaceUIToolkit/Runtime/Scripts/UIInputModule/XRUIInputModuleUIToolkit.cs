using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TouchPhase = UnityEngine.TouchPhase;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    /// <summary>
    ///     Custom class for input modules that send UI input in XR. Adapted for UIToolkit runtime world space panels.
    /// </summary>
    public class XRUIInputModuleUIToolkit : UIInputModuleUIToolkit
    {
        [SerializeField]
        [Tooltip("If true, will forward 3D tracked device data to UI elements.")]
        bool m_EnableXRInput = true;

        [SerializeField]
        [Tooltip("If true, will forward 2D mouse data to UI elements.")]
        bool m_EnableMouseInput = true;

        [SerializeField]
        [Tooltip("If true, will forward 2D touch data to UI elements.")]
        bool m_EnableTouchInput = true;

        [FormerlySerializedAs("usePenPointerIdBase")]
        [SerializeField]
        protected bool UsePenPointerIdBase;

        readonly List<RegisteredInteractor> m_RegisteredInteractors = new List<RegisteredInteractor>();

        readonly List<RegisteredTouch> m_RegisteredTouches = new List<RegisteredTouch>();

        MouseModel m_Mouse;
        bool m_penBaseIdAssigned;

        int m_RollingInteractorIndex = PointerId.touchPointerIdBase;

        /// <summary>
        ///     If <see langword="true" />, will forward 3D tracked device data to UI elements.
        /// </summary>
        public bool EnableXRInput
        {
            get => m_EnableXRInput;
            set => m_EnableXRInput = value;
        }

        /// <summary>
        ///     If <see langword="true" />, will forward 2D mouse data to UI elements.
        /// </summary>
        public bool EnableMouseInput
        {
            get => m_EnableMouseInput;
            set => m_EnableMouseInput = value;
        }

        /// <summary>
        ///     If <see langword="true" />, will forward 2D touch data to UI elements.
        /// </summary>
        public bool EnableTouchInput
        {
            get => m_EnableTouchInput;
            set => m_EnableTouchInput = value;
        }

        /// <summary>
        ///     If <see langword="true" />, all input is unified to a single pointer.
        ///     This means that all input from all pointing devices (<see cref="Mouse" />,
        ///     <see cref="Pen" />, <see cref="Touchscreen" />, and <see cref="TrackedDevice" />) is routed into a single pointer
        ///     instance. There is only one position on screen which can be controlled from any of these devices.
        /// </summary>
        public bool SingleUnifiedPointer { get; set; }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            m_Mouse = new MouseModel(0);
        }

        public override int ConvertUIToolkitPointerId(PointerEventData sourcePointerData)
        {
            if (SingleUnifiedPointer)
            {
                return PointerId.touchPointerIdBase;
            }

            return sourcePointerData.pointerId;
        }

        /// <summary>
        ///     Register an <see cref="IUIInteractor" /> with the UI system.
        ///     Calling this will enable it to start interacting with UI.
        /// </summary>
        /// <param name="interactor">The <see cref="IUIInteractor" /> to use.</param>
        public void RegisterInteractor(IUIInteractor interactor)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].Interactor == interactor)
                    return;
            }

            if (UsePenPointerIdBase && !m_penBaseIdAssigned)
            {
                m_penBaseIdAssigned = true;
                m_RegisteredInteractors.Add(new RegisteredInteractor(interactor, PointerId.penPointerIdBase));
            }
            else
            {
                if (UsePenPointerIdBase && m_RollingInteractorIndex == PointerId.penPointerIdBase)
                    ++m_RollingInteractorIndex;

                m_RegisteredInteractors.Add(new RegisteredInteractor(interactor, m_RollingInteractorIndex++));
            }
        }

        /// <summary>
        ///     Unregisters an <see cref="IUIInteractor" /> with the UI system.
        ///     This cancels all UI Interaction and makes the <see cref="IUIInteractor" /> no longer able to affect UI.
        /// </summary>
        /// <param name="interactor">The <see cref="IUIInteractor" /> to stop using.</param>
        public void UnregisterInteractor(IUIInteractor interactor)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].Interactor == interactor)
                {
                    var registeredInteractor = m_RegisteredInteractors[i];
                    registeredInteractor.Interactor = null;
                    m_RegisteredInteractors[i] = registeredInteractor;
                    return;
                }
            }
        }

        /// <summary>
        ///     Gets an <see cref="IUIInteractor" /> from its corresponding Unity UI Pointer Id.
        ///     This can be used to identify individual Interactors from the underlying UI Events.
        /// </summary>
        /// <param name="pointerId">A unique integer representing an object that can point at UI.</param>
        /// <returns>
        ///     Returns the interactor associated with <paramref name="pointerId" />.
        ///     Returns <see langword="null" /> if no Interactor is associated (e.g. if it's a mouse event).
        /// </returns>
        public IUIInteractor GetInteractor(int pointerId)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].Model.pointerId == pointerId)
                {
                    return m_RegisteredInteractors[i].Interactor;
                }
            }

            return null;
        }

        /// <summary>Retrieves the UI Model for a selected <see cref="IUIInteractor" />.</summary>
        /// <param name="interactor">The <see cref="IUIInteractor" /> you want the model for.</param>
        /// <param name="model">The returned model that reflects the UI state of the <paramref name="interactor" />.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the model was able to retrieved. Otherwise, returns
        ///     <see langword="false" />.
        /// </returns>
        public bool GetTrackedDeviceModel(IUIInteractor interactor, out TrackedDeviceModel model)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].Interactor == interactor)
                {
                    model = m_RegisteredInteractors[i].Model;
                    return true;
                }
            }

            model = new TrackedDeviceModel(-1);
            return false;
        }

        /// <inheritdoc />
        protected override void DoProcess()
        {
            base.DoProcess();

            if (m_EnableXRInput)
            {
                for (var i = 0; i < m_RegisteredInteractors.Count; i++)
                {
                    var registeredInteractor = m_RegisteredInteractors[i];

                    // If device is removed, we send a default state to unclick any UI
                    if (registeredInteractor.Interactor == null)
                    {
                        registeredInteractor.Model.Reset(false);
                        ProcessTrackedDevice(ref registeredInteractor.Model, true);
                        m_RegisteredInteractors.RemoveAt(i--);
                    }
                    else
                    {
                        registeredInteractor.Interactor.UpdateUIModel(ref registeredInteractor.Model);
                        ProcessTrackedDevice(ref registeredInteractor.Model);
                        m_RegisteredInteractors[i] = registeredInteractor;
                    }
                    registeredInteractor.Model.OnFrameFinished();
                }
            }

            if (m_EnableMouseInput)
                ProcessMouse();

            if (m_EnableTouchInput)
                ProcessTouches();
        }

        void ProcessMouse()
        {
            if (Mouse.current != null)
            {
                // The Input System reports scroll in pixels, whereas the old Input class reported in lines.
                // Example, scrolling down by one notch of a mouse wheel for Input would be (0, -1),
                // but would be (0, -120) from Input System.
                // For consistency between the two Active Input Handling modes and with StandaloneInputModule,
                // scale the scroll value to the range expected by UI.
                const float kPixelsPerLine = 120f;
                m_Mouse.Position = Mouse.current.position.ReadValue();
                m_Mouse.ScrollDelta = Mouse.current.scroll.ReadValue() * (1 / kPixelsPerLine);
                m_Mouse.LeftButtonPressed = Mouse.current.leftButton.isPressed;
                m_Mouse.RightButtonPressed = Mouse.current.rightButton.isPressed;
                m_Mouse.MiddleButtonPressed = Mouse.current.middleButton.isPressed;

                ProcessMouse(ref m_Mouse);
            }
            else if (Input.mousePresent)
            {
                m_Mouse.Position = Input.mousePosition;
                m_Mouse.ScrollDelta = Input.mouseScrollDelta;
                m_Mouse.LeftButtonPressed = Input.GetMouseButton(0);
                m_Mouse.RightButtonPressed = Input.GetMouseButton(1);
                m_Mouse.MiddleButtonPressed = Input.GetMouseButton(2);

                ProcessMouse(ref m_Mouse);
            }
        }

        void EnsureTouchIsRegistered(Touch touch)
        {
            var registeredTouchIndex = m_RegisteredTouches.FindIndex(x => x.TouchId == touch.fingerId);

            if (registeredTouchIndex < 0)
            {
                // Not found, search empty pool
                var freeIndex = m_RegisteredTouches.FindIndex(x => !x.IsValid);
                if (freeIndex > 0)
                {
                    var pointerId = m_RegisteredTouches[freeIndex].Model.PointerId;
                    m_RegisteredTouches[freeIndex] = new RegisteredTouch(touch, pointerId);
                    registeredTouchIndex = freeIndex;
                }
                else
                {
                    // No Empty slots, add one
                    registeredTouchIndex = m_RegisteredTouches.Count;
                    m_RegisteredTouches.Add(new RegisteredTouch(touch, m_RollingInteractorIndex++));
                }
            }

            var registeredTouch = m_RegisteredTouches[registeredTouchIndex];
            registeredTouch.Model.SelectPhase = touch.phase;
            registeredTouch.Model.Position = touch.position;
            m_RegisteredTouches[registeredTouchIndex] = registeredTouch;
        }

        void ProcessTouches()
        {
            if (Input.touchCount <= 0)
                return;

            foreach (var touch in Input.touches)
            {
                EnsureTouchIsRegistered(touch);
            }

            for (var i = 0; i < m_RegisteredTouches.Count; i++)
            {
                var registeredTouch = m_RegisteredTouches[i];
                ProcessTouch(registeredTouch.Model);
                if (registeredTouch.Model.SelectPhase == (TouchPhase)UnityEngine.InputSystem.TouchPhase.Ended || registeredTouch.Model.SelectPhase == (TouchPhase)UnityEngine.InputSystem.TouchPhase.Canceled)
                    registeredTouch.IsValid = false;
                m_RegisteredTouches[i] = registeredTouch;
            }
        }

        struct RegisteredInteractor
        {
            public IUIInteractor Interactor;
            public TrackedDeviceModel Model;

            public RegisteredInteractor(IUIInteractor interactor, int deviceIndex)
            {
                Interactor = interactor;
                Model = new TrackedDeviceModel(deviceIndex);
            }
        }

        struct RegisteredTouch
        {
            public bool IsValid;
            public readonly int TouchId;
            public TouchModel Model;

            public RegisteredTouch(Touch touch, int deviceIndex)
            {
                TouchId = touch.fingerId;
                Model = new TouchModel(deviceIndex);
                IsValid = true;
            }
        }
    }
}
