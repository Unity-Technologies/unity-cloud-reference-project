using System;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools
{
    public interface IToolUIModeHandler
    {
        bool IsOpened { get; }
        bool KeepOpened { get; set; }
        public VisualElement CreateVisualTree(ActionButton button, ToolUIController toolUIController);

        public void OpenTool();

        public void CloseTool();

        public ActionButton GetButton();
    }

    public abstract class ToolUIMode : ScriptableObject
    {
        public abstract IToolUIModeHandler CreateHandler();
    }

    public abstract class ToolUIModeHandler : IToolUIModeHandler
    {
        protected ActionButton Button { private set; get; }

        protected ToolUIController ToolUIController { private set; get; }

        public VisualElement CreateVisualTree(ActionButton button, ToolUIController toolUIController)
        {
            Button = button;
            ToolUIController = toolUIController;
            ToolUIController.SetCloseAction(CloseTool);
            ToolUIController.ButtonStateChanged += OnButtonStateChanged;

            return CreateVisualTreeInternal();
        }

        public bool IsOpened => Button.selected;
        public bool KeepOpened { get; set; }

        public void OpenTool()
        {
            Button.selected = true;
            OnToolOpenedInternal();
            ToolUIController.InvokeToolOpened();
        }

        public void CloseTool()
        {
            Button.selected = false;
            OnToolClosedInternal();
            ToolUIController.InvokeToolClosed();
        }

        void OnButtonStateChanged(ToolUIController.ToolState state)
        {
            switch (state)
            {
                case ToolUIController.ToolState.Active:
                    Button.style.display = DisplayStyle.Flex;
                    Button.SetEnabled(true);
                    break;
                
                case ToolUIController.ToolState.Inactive:
                    Button.style.display = DisplayStyle.Flex;
                    Button.SetEnabled(false);
                    break;
                
                case ToolUIController.ToolState.Hidden:
                    Button.style.display = DisplayStyle.None;
                    break;
            }
            
            if (state != ToolUIController.ToolState.Active)
            {
                CloseTool();
            }
        }

        protected abstract VisualElement CreateVisualTreeInternal();

        protected abstract void OnToolOpenedInternal();

        protected abstract void OnToolClosedInternal();
        
        public ActionButton GetButton()
        {
            return Button;
        }
    }
}
