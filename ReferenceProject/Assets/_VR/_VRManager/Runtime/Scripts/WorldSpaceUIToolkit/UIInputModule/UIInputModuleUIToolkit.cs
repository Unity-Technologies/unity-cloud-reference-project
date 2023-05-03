using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.VRManager
{
    /// <summary>
    ///     Base class for input modules that send UI input.
    /// </summary>
    /// <remarks>
    ///     Multiple input modules may be placed on the same event system. In such a setup,
    ///     the modules will synchronize with each other.
    /// </remarks>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_UIInputModule)]
    public abstract partial class UIInputModuleUIToolkit : BaseInputModule
    {
        [SerializeField]
        [Tooltip("The maximum time (in seconds) between two mouse presses for it to be consecutive click.")]
        float m_ClickSpeed = 0.3f;

        [SerializeField]
        [Tooltip("The absolute value required by a move action on either axis required to trigger a move event.")]
        float m_MoveDeadzone = 0.6f;

        [SerializeField]
        [Tooltip("The Initial delay (in seconds) between an initial move action and a repeated move action.")]
        float m_RepeatDelay = 0.5f;

        [SerializeField]
        [Tooltip("The speed (in seconds) that the move action repeats itself once repeating.")]
        float m_RepeatRate = 0.1f;

        [SerializeField]
        [Tooltip("Scales the EventSystem.pixelDragThreshold, for tracked devices, to make selection easier.")]
        float m_TrackedDeviceDragThresholdMultiplier = 2f;

        AxisEventData m_CachedAxisEvent;
        PointerEventData m_CachedPointerEvent;
        TrackedDeviceEventData m_CachedTrackedDeviceEventData;

        /// <summary>
        ///     The maximum time (in seconds) between two mouse presses for it to be consecutive click.
        /// </summary>
        public float ClickSpeed
        {
            get => m_ClickSpeed;
            set => m_ClickSpeed = value;
        }

        /// <summary>
        ///     The absolute value required by a move action on either axis required to trigger a move event.
        /// </summary>
        public float MoveDeadzone
        {
            get => m_MoveDeadzone;
            set => m_MoveDeadzone = value;
        }

        /// <summary>
        ///     The Initial delay (in seconds) between an initial move action and a repeated move action.
        /// </summary>
        public float RepeatDelay
        {
            get => m_RepeatDelay;
            set => m_RepeatDelay = value;
        }

        /// <summary>
        ///     The speed (in seconds) that the move action repeats itself once repeating.
        /// </summary>
        public float RepeatRate
        {
            get => m_RepeatRate;
            set => m_RepeatRate = value;
        }

        /// <summary>
        ///     Scales the <see cref="EventSystem.pixelDragThreshold" />, for tracked devices, to make selection easier.
        /// </summary>
        public float TrackedDeviceDragThresholdMultiplier
        {
            get => m_TrackedDeviceDragThresholdMultiplier;
            set => m_TrackedDeviceDragThresholdMultiplier = value;
        }

        /// <summary>
        ///     The <see cref="Camera" /> that is used to perform 2D raycasts when determining the screen space location of a
        ///     tracked device cursor.
        /// </summary>
        public Camera UICamera { get; set; }

        /// <summary>
        ///     See <see cref="MonoBehaviour" />.
        /// </summary>
        /// <remarks>
        ///     Processing is postponed from earlier in the frame (<see cref="EventSystem" /> has a
        ///     script execution order of <c>-1000</c>) until this Update to allow other systems to
        ///     update the poses that will be used to generate the raycasts used by this input module.
        ///     <br />
        ///     For Ray Interactor, it must wait until after the Controller pose updates and Locomotion
        ///     moves the Rig in order to generate the current sample points used to create the rays used
        ///     for this frame. Those positions will be determined during <see cref="DoProcess" />.
        ///     Ray Interactor needs the UI raycasts to be completed by the time <see cref="XRInteractionManager" />
        ///     calls into <see cref="XRBaseInteractor.GetValidTargets" /> since that is dependent on
        ///     whether a UI hit was closer than a 3D hit. This processing must therefore be done
        ///     between Locomotion and <see cref="XRBaseInteractor.ProcessInteractor" /> to minimize latency.
        /// </remarks>
        protected virtual void Update()
        {
            // Check to make sure that Process should still be called.
            // It would likely cause unexpected results if processing was done
            // when this module is no longer the current one.
            if (eventSystem.IsActive() && eventSystem.currentInputModule == this && eventSystem == EventSystem.current)
            {
                DoProcess();
            }
        }

        /// <summary>
        ///     Process the current tick for the module.
        /// </summary>
        /// <remarks>
        ///     Executed once per Update call. Override for custom processing.
        /// </remarks>
        /// <seealso cref="Process" />
        protected virtual void DoProcess()
        {
            SendUpdateEventToSelectedObject();
        }

        /// <inheritdoc />
        public override void Process()
        {
            // Postpone processing until later in the frame
        }

        /// <summary>
        ///     Sends an update event to the currently selected object.
        /// </summary>
        /// <returns>Returns whether the update event was used by the selected object.</returns>
        protected bool SendUpdateEventToSelectedObject()
        {
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (selectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            UpdateSelected?.Invoke(selectedGameObject, data);
            ExecuteEvents.Execute(selectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        RaycastResult PerformRaycast(PointerEventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            eventSystem.RaycastAll(eventData, m_RaycastResultCache);
            FinalizeRaycastResults?.Invoke(eventData, m_RaycastResultCache);
            var result = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            return result;
        }

        /// <summary>
        ///     Takes an existing <see cref="MouseModel" /> and dispatches all relevant changes through the event system.
        ///     It also updates the internal data of the <see cref="MouseModel" />.
        /// </summary>
        /// <param name="mouseState">The mouse state you want to forward into the UI Event System.</param>
        internal void ProcessMouse(ref MouseModel mouseState)
        {
            if (!mouseState.ChangedThisFrame)
                return;

            var eventData = GetOrCreateCachedPointerEvent();
            eventData.Reset();

            mouseState.CopyTo(eventData);

            eventData.pointerCurrentRaycast = PerformRaycast(eventData);

            // Left Mouse Button
            // The left mouse button is 'dominant' and we want to also process hover and scroll events as if the occurred during the left click.
            var buttonState = mouseState.LeftButton;
            buttonState.CopyTo(eventData);
            ProcessMouseButton(buttonState.LastFrameDelta, eventData);

            ProcessMouseMovement(eventData);
            ProcessMouseScroll(eventData);

            mouseState.CopyFrom(eventData);

            ProcessMouseButtonDrag(eventData);

            buttonState.CopyFrom(eventData);
            mouseState.LeftButton = buttonState;

            // Right Mouse Button
            buttonState = mouseState.RightButton;
            buttonState.CopyTo(eventData);

            ProcessMouseButton(buttonState.LastFrameDelta, eventData);
            ProcessMouseButtonDrag(eventData);

            buttonState.CopyFrom(eventData);
            mouseState.RightButton = buttonState;

            // Middle Mouse Button
            buttonState = mouseState.MiddleButton;
            buttonState.CopyTo(eventData);

            ProcessMouseButton(buttonState.LastFrameDelta, eventData);
            ProcessMouseButtonDrag(eventData);

            buttonState.CopyFrom(eventData);
            mouseState.MiddleButton = buttonState;

            mouseState.OnFrameFinished();
        }

        void ProcessMouseMovement(PointerEventData eventData)
        {
            var currentPointerTarget = eventData.pointerCurrentRaycast.gameObject;

            foreach (var hovered in eventData.hovered)
            {
                ExecuteEvents.Execute(hovered, eventData, ExecuteEvents.pointerMoveHandler);
            }

            // If we have no target or pointerEnter has been deleted,
            // we just send exit events to anything we are tracking
            // and then exit.
            if (currentPointerTarget == null || eventData.pointerEnter == null)
            {
                foreach (var hovered in eventData.hovered)
                {
                    PointerExit?.Invoke(hovered, eventData);
                    ExecuteEvents.Execute(hovered, eventData, ExecuteEvents.pointerExitHandler);
                }

                eventData.hovered.Clear();

                if (currentPointerTarget == null)
                {
                    eventData.pointerEnter = null;
                    return;
                }
            }

            if (eventData.pointerEnter == currentPointerTarget)
                return;

            var commonRoot = FindCommonRoot(eventData.pointerEnter, currentPointerTarget);

            // We walk up the tree until a common root and the last entered and current entered object is found.
            // Then send exit and enter events up to, but not including, the common root.
            if (eventData.pointerEnter != null)
            {
                var target = eventData.pointerEnter.transform;

                while (target != null)
                {
                    if (commonRoot != null && commonRoot.transform == target)
                        break;

                    var targetGameObject = target.gameObject;
                    PointerExit?.Invoke(targetGameObject, eventData);
                    ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerExitHandler);

                    eventData.hovered.Remove(targetGameObject);

                    target = target.parent;
                }
            }

            eventData.pointerEnter = currentPointerTarget;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse -- Could be null if it was destroyed immediately after executing above
            if (currentPointerTarget != null)
            {
                var target = currentPointerTarget.transform;

                while (target != null && target.gameObject != commonRoot)
                {
                    var targetGameObject = target.gameObject;
                    PointerEnter?.Invoke(targetGameObject, eventData);
                    ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerEnterHandler);

                    eventData.hovered.Add(targetGameObject);

                    target = target.parent;
                }
            }
        }

        void ProcessMouseButton(ButtonDeltaState mouseButtonChanges, PointerEventData eventData)
        {
            var hoverTarget = eventData.pointerCurrentRaycast.gameObject;

            if ((mouseButtonChanges & ButtonDeltaState.Pressed) != 0)
            {
                eventData.eligibleForClick = true;
                eventData.delta = Vector2.zero;
                eventData.dragging = false;
                eventData.pressPosition = eventData.position;
                eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

                var selectHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(hoverTarget);

                // If we have clicked something new, deselect the old thing
                // and leave 'selection handling' up to the press event.
                if (selectHandler != eventSystem.currentSelectedGameObject)
                    eventSystem.SetSelectedGameObject(null, eventData);

                // search for the control that will receive the press.
                // if we can't find a press handler set the press
                // handler to be what would receive a click.

                PointerDown?.Invoke(hoverTarget, eventData);
                var newPressed = ExecuteEvents.ExecuteHierarchy(hoverTarget, eventData, ExecuteEvents.pointerDownHandler);

                // We didn't find a press handler, so we search for a click handler.
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoverTarget);

                var time = Time.unscaledTime;

                if (newPressed == eventData.lastPress && ((time - eventData.clickTime) < m_ClickSpeed))
                    ++eventData.clickCount;
                else
                    eventData.clickCount = 1;

                eventData.clickTime = time;

                eventData.pointerPress = newPressed;
                eventData.rawPointerPress = hoverTarget;

                // Save the drag handler for drag events during this mouse down.
                var dragObject = ExecuteEvents.GetEventHandler<IDragHandler>(hoverTarget);
                eventData.pointerDrag = dragObject;

                if (dragObject != null)
                {
                    InitializePotentialDrag?.Invoke(dragObject, eventData);
                    ExecuteEvents.Execute(dragObject, eventData, ExecuteEvents.initializePotentialDrag);
                }
            }

            if ((mouseButtonChanges & ButtonDeltaState.Released) != 0)
            {
                var target = eventData.pointerPress;
                PointerUp?.Invoke(target, eventData);
                ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);

                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoverTarget);
                var pointerDrag = eventData.pointerDrag;
                if (target == pointerUpHandler && eventData.eligibleForClick)
                {
                    PointerClick?.Invoke(target, eventData);
                    ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
                }
                else if (eventData.dragging && pointerDrag != null)
                {
                    Drop?.Invoke(hoverTarget, eventData);
                    ExecuteEvents.ExecuteHierarchy(hoverTarget, eventData, ExecuteEvents.dropHandler);

                    EndDrag?.Invoke(pointerDrag, eventData);
                    ExecuteEvents.Execute(pointerDrag, eventData, ExecuteEvents.endDragHandler);
                }

                eventData.eligibleForClick = eventData.dragging = false;
                eventData.pointerPress = eventData.rawPointerPress = eventData.pointerDrag = null;
            }
        }

        void ProcessMouseButtonDrag(PointerEventData eventData, float pixelDragThresholdMultiplier = 1.0f)
        {
            if (!eventData.IsPointerMoving() ||
                Cursor.lockState == CursorLockMode.Locked ||
                eventData.pointerDrag == null)
            {
                return;
            }

            if (!eventData.dragging)
            {
                if ((eventData.pressPosition - eventData.position).sqrMagnitude >= ((eventSystem.pixelDragThreshold * eventSystem.pixelDragThreshold) * pixelDragThresholdMultiplier))
                {
                    var target = eventData.pointerDrag;
                    BeginDrag?.Invoke(target, eventData);
                    ExecuteEvents.Execute(target, eventData, ExecuteEvents.beginDragHandler);
                    eventData.dragging = true;
                }
            }

            if (eventData.dragging)
            {
                // If we moved from our initial press object, process an up for that object.
                var target = eventData.pointerPress;
                if (target != eventData.pointerDrag)
                {
                    PointerUp?.Invoke(target, eventData);
                    ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);

                    eventData.eligibleForClick = false;
                    eventData.pointerPress = null;
                    eventData.rawPointerPress = null;
                }

                Drag?.Invoke(eventData.pointerDrag, eventData);
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
            }
        }

        void ProcessMouseScroll(PointerEventData eventData)
        {
            var scrollDelta = eventData.scrollDelta;
            if (!Mathf.Approximately(scrollDelta.sqrMagnitude, 0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerEnter);
                Scroll?.Invoke(scrollHandler, eventData);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, eventData, ExecuteEvents.scrollHandler);
            }
        }

        internal void ProcessTouch(ref TouchModel touchState)
        {
            if (!touchState.ChangedThisFrame)
                return;

            var eventData = GetOrCreateCachedPointerEvent();
            eventData.Reset();

            touchState.CopyTo(eventData);

            eventData.pointerCurrentRaycast = (touchState.SelectPhase == TouchPhase.Canceled) ? new RaycastResult() : PerformRaycast(eventData);
            eventData.button = PointerEventData.InputButton.Left;

            ProcessMouseButton(touchState.SelectDelta, eventData);
            ProcessMouseMovement(eventData);
            ProcessMouseButtonDrag(eventData);

            touchState.CopyFrom(eventData);

            touchState.OnFrameFinished();
        }

        internal void ProcessTrackedDevice(ref TrackedDeviceModel deviceState, bool force = false)
        {
            if (!deviceState.changedThisFrame && !force)
                return;

            var eventData = GetOrCreateCachedTrackedDeviceEvent();
            eventData.Reset();
            deviceState.CopyTo(eventData);

            eventData.button = PointerEventData.InputButton.Left;

            // Demolish the screen position so we don't trigger any hits from a GraphicRaycaster component on a Canvas.
            // The position value is not used by the TrackedDeviceGraphicRaycaster.
            // Restore the original value after the Raycast is complete.
            var savedPosition = eventData.position;
            eventData.position = new Vector2(float.MinValue, float.MinValue);
            eventData.pointerCurrentRaycast = PerformRaycast(eventData);
            eventData.position = savedPosition;

            // Get associated camera, or main-tagged camera, or camera from raycast, and if *nothing* exists, then abort processing this frame.
            // ReSharper disable once LocalVariableHidesMember
            var camera = UICamera != null ? UICamera : (UICamera = Camera.main);
            if (camera == null)
            {
                var module = eventData.pointerCurrentRaycast.module;
                if (module != null)
                    camera = module.eventCamera;
            }

            if (camera != null)
            {
                Vector2 screenPosition;
                if (eventData.pointerCurrentRaycast.isValid)
                {
                    screenPosition = camera.WorldToScreenPoint(eventData.pointerCurrentRaycast.worldPosition);
                }
                else
                {
                    var endPosition = eventData.rayPoints.Count > 0 ? eventData.rayPoints[eventData.rayPoints.Count - 1] : Vector3.zero;
                    screenPosition = camera.WorldToScreenPoint(endPosition);
                    eventData.position = screenPosition;
                }

                var thisFrameDelta = screenPosition - eventData.position;
                eventData.position = screenPosition;
                eventData.delta = thisFrameDelta;

                ProcessMouseButton(deviceState.selectDelta, eventData);
                ProcessMouseMovement(eventData);
                ProcessMouseScroll(eventData);
                ProcessMouseButtonDrag(eventData, m_TrackedDeviceDragThresholdMultiplier);

                deviceState.CopyFrom(eventData);
            }

            deviceState.OnFrameFinished();
        }

        // TODO Update UIInputModule to make use of unused ProcessJoystick method
        /// <summary>
        ///     Takes an existing JoystickModel and dispatches all relevant changes through the event system.
        ///     It also updates the internal data of the JoystickModel.
        /// </summary>
        /// <param name="joystickState">The joystick state you want to forward into the UI Event System</param>
        internal void ProcessJoystick(ref JoystickModel joystickState)
        {
            var implementationData = joystickState.ImplementationDataValue;

            var usedSelectionChange = false;
            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (selectedGameObject != null)
            {
                var data = GetBaseEventData();
                UpdateSelected?.Invoke(selectedGameObject, data);
                ExecuteEvents.Execute(selectedGameObject, data, ExecuteEvents.updateSelectedHandler);
                usedSelectionChange = data.used;
            }

            // Don't send move events if disabled in the EventSystem.
            if (!eventSystem.sendNavigationEvents)
                return;

            var movement = joystickState.Move;
            if (!usedSelectionChange && (!Mathf.Approximately(movement.x, 0f) || !Mathf.Approximately(movement.y, 0f)))
            {
                var time = Time.unscaledTime;

                var moveVector = joystickState.Move;

                var moveDirection = MoveDirection.None;
                if (moveVector.sqrMagnitude > m_MoveDeadzone * m_MoveDeadzone)
                {
                    if (Mathf.Abs(moveVector.x) > Mathf.Abs(moveVector.y))
                        moveDirection = (moveVector.x > 0) ? MoveDirection.Right : MoveDirection.Left;
                    else
                        moveDirection = (moveVector.y > 0) ? MoveDirection.Up : MoveDirection.Down;
                }

                if (moveDirection != implementationData.LastMoveDirection)
                {
                    implementationData.ConsecutiveMoveCount = 0;
                }

                if (moveDirection != MoveDirection.None)
                {
                    var allow = true;
                    if (implementationData.ConsecutiveMoveCount != 0)
                    {
                        if (implementationData.ConsecutiveMoveCount > 1)
                            allow = (time > (implementationData.LastMoveTime + m_RepeatRate));
                        else
                            allow = (time > (implementationData.LastMoveTime + m_RepeatDelay));
                    }

                    if (allow)
                    {
                        var eventData = GetOrCreateCachedAxisEvent();
                        eventData.Reset();

                        eventData.moveVector = moveVector;
                        eventData.moveDir = moveDirection;

                        Move?.Invoke(selectedGameObject, eventData);
                        ExecuteEvents.Execute(selectedGameObject, eventData, ExecuteEvents.moveHandler);
                        usedSelectionChange = eventData.used;

                        implementationData.ConsecutiveMoveCount++;
                        implementationData.LastMoveTime = time;
                        implementationData.LastMoveDirection = moveDirection;
                    }
                }
                else
                    implementationData.ConsecutiveMoveCount = 0;
            }
            else
                implementationData.ConsecutiveMoveCount = 0;

            if (!usedSelectionChange)
            {
                if (selectedGameObject != null)
                {
                    var data = GetBaseEventData();
                    if ((joystickState.SubmitButtonDelta & ButtonDeltaState.Pressed) != 0)
                    {
                        Submit?.Invoke(selectedGameObject, data);
                        ExecuteEvents.Execute(selectedGameObject, data, ExecuteEvents.submitHandler);
                    }

                    if (!data.used && (joystickState.CancelButtonDelta & ButtonDeltaState.Pressed) != 0)
                    {
                        Cancel?.Invoke(selectedGameObject, data);
                        ExecuteEvents.Execute(selectedGameObject, data, ExecuteEvents.cancelHandler);
                    }
                }
            }

            joystickState.ImplementationDataValue = implementationData;
            joystickState.OnFrameFinished();
        }

        PointerEventData GetOrCreateCachedPointerEvent()
        {
            var result = m_CachedPointerEvent;
            if (result == null)
            {
                result = new PointerEventData(eventSystem);
                m_CachedPointerEvent = result;
            }

            return result;
        }

        TrackedDeviceEventData GetOrCreateCachedTrackedDeviceEvent()
        {
            var result = m_CachedTrackedDeviceEventData;
            if (result == null)
            {
                result = new TrackedDeviceEventData(eventSystem);
                m_CachedTrackedDeviceEventData = result;
            }

            return result;
        }

        AxisEventData GetOrCreateCachedAxisEvent()
        {
            var result = m_CachedAxisEvent;
            if (result == null)
            {
                result = new AxisEventData(eventSystem);
                m_CachedAxisEvent = result;
            }

            return result;
        }
    }
}
