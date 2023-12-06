using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.MeasureTool
{
    /// <summary>
    /// A draggable button that can be used to move the reference point implemented using UIToolkit/AppUI
    /// </summary>
    public class DraggableButton : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<DraggableButton, UxmlTraits>
        {
            public override string uxmlName => "DraggableButton";
        }

        public class DragEvent : UnityEvent<Vector3> { }

        readonly DragEvent m_OnUp = new DragEvent();
        readonly DragEvent m_OnDrag = new DragEvent();
        readonly DragEvent m_OnDown = new DragEvent();

        public DragEvent onUp => m_OnUp;
        public DragEvent onDrag => m_OnDrag;
        public DragEvent onDown => m_OnDown;

        static class Keys
        {
            public const string DraggablePad = "DraggablePad";
        }

        /// <summary>
        /// Constructor for the draggable button, no callback setup
        /// </summary>
        public DraggableButton()
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>(Keys.DraggablePad);
            template.CloneTree(this);

            var draggableManipulator = new Draggable(OnClicked, OnDrag, OnUp, OnDown);
            this.AddManipulator(draggableManipulator);
        }

        /// <summary>
        /// Constructor for the draggable button that takes in the callbacks for the drag, down and up events 
        /// </summary>
        /// <param name="dragged"></param>
        /// <param name="down"></param>
        /// <param name="up"></param>
        public DraggableButton(VisualTreeAsset template, Action<Vector3> dragged, Action<Vector3> down, Action<Vector3> up)
        {
            template.CloneTree(this);

            var draggableManipulator = new Draggable(OnClicked, OnDrag, OnUp, OnDown);
            this.AddManipulator(draggableManipulator);

            m_OnDrag.AddListener(dragged.Invoke);
            m_OnDown.AddListener(down.Invoke);
            m_OnUp.AddListener(up.Invoke);
        }

        void OnClicked()
        {
            // Do nothing for now
        }

        void OnDrag(Draggable obj)
        {
            var position = Pointer.current.position.ReadValue();
            m_OnDrag?.Invoke(position);
        }

        void OnUp(Draggable obj)
        {
            var position = Pointer.current.position.ReadValue();
            m_OnUp?.Invoke(position);
        }

        void OnDown(Draggable obj)
        {
            var position = Pointer.current.position.ReadValue();
            m_OnDown?.Invoke(position);
        }
    }
}