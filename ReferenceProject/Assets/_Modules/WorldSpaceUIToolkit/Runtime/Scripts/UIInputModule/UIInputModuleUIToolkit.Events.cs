using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    public abstract partial class UIInputModuleUIToolkit
    {
        /// <summary>
        ///     Calls the methods in its invocation list after the input module collects a list of type
        ///     <see cref="RaycastResult" />, but before the results are used.
        ///     Note that not all fields of the event data are still valid or up to date at this point in the UI event processing.
        ///     This event can be used to read, modify, or reorder results.
        ///     After the event, the first result in the list with a non-null GameObject will be used.
        /// </summary>
        public event Action<PointerEventData, List<RaycastResult>> FinalizeRaycastResults;

        /// <summary>
        ///     This occurs when a UI pointer enters an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> PointerEnter;

        /// <summary>
        ///     This occurs when a UI pointer exits an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> PointerExit;

        /// <summary>
        ///     This occurs when a select button down occurs while a UI pointer is hovering an element.
        ///     This event is executed using ExecuteEvents.ExecuteHierarchy when sent to the target element.
        /// </summary>
        public event Action<GameObject, PointerEventData> PointerDown;

        /// <summary>
        ///     This occurs when a select button up occurs while a UI pointer is hovering an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> PointerUp;

        /// <summary>
        ///     This occurs when a select button click occurs while a UI pointer is hovering an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> PointerClick;

        /// <summary>
        ///     This occurs when a potential drag occurs on an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> InitializePotentialDrag;

        /// <summary>
        ///     This occurs when a drag first occurs on an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> BeginDrag;

        /// <summary>
        ///     This occurs every frame while dragging an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> Drag;

        /// <summary>
        ///     This occurs on the last frame an element is dragged.
        /// </summary>
        public event Action<GameObject, PointerEventData> EndDrag;

        /// <summary>
        ///     This occurs when a dragged element is dropped on a drop handler.
        /// </summary>
        public event Action<GameObject, PointerEventData> Drop;

        /// <summary>
        ///     This occurs when an element is scrolled
        ///     This event is executed using ExecuteEvents.ExecuteHierarchy when sent to the target element.
        /// </summary>
        public event Action<GameObject, PointerEventData> Scroll;

        /// <summary>
        ///     This occurs on update for the currently selected object.
        /// </summary>
        public event Action<GameObject, BaseEventData> UpdateSelected;

        /// <summary>
        ///     This occurs when the move axis is activated.
        /// </summary>
        public event Action<GameObject, AxisEventData> Move;

        /// <summary>
        ///     This occurs when the submit button is pressed.
        /// </summary>
        public event Action<GameObject, BaseEventData> Submit;

        /// <summary>
        ///     This occurs when the cancel button is pressed.
        /// </summary>
        public event Action<GameObject, BaseEventData> Cancel;
    }
}
