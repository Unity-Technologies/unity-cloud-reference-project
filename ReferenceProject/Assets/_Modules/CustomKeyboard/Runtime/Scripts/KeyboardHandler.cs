using System;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.CustomKeyboard
{
    public class KeyboardHandler : MonoBehaviour
    {
        [SerializeField]
        bool m_ForceMouseClick;

        IKeyboardController m_KeyboardController;
        Mouse m_Mouse;

        [Inject]
        void Setup(IKeyboardController keyboardController, Mouse mouse)
        {
            m_KeyboardController = keyboardController;
            m_Mouse = mouse;
        }

        void Start()
        {
            var toolUIController = GetComponent<ToolUIController>();
            if (toolUIController)
            {
                RegisterRootVisualElement(toolUIController.RootVisualElement);
            }
        }

        public void RegisterRootVisualElement(VisualElement rootVisualElement)
        {
            rootVisualElement.Query<TextField>().ForEach(textInput =>
            {
                RegisterTextField(textInput);
            });
        }

        void RegisterTextField(TextField textInput)
        {
            textInput.RegisterCallback<FocusEvent>(evt =>
            {
                m_KeyboardController.OpenKeyboard(textInput);
            });

            if (m_ForceMouseClick)
            {
                textInput.RegisterCallback<PointerDownEvent>(evt =>
                {
                    MouseClick(textInput, 1f);
                });

                textInput.RegisterCallback<PointerUpEvent>(evt =>
                {
                    MouseClick(textInput, 0f);
                });
            }
        }

        void MouseClick(TextField textInput, float value)
        {
            // This is necessary because moving cursor in text field only work with mouse events
            if (Utils.IsFocused(textInput) && m_Mouse != null)
            {
                using (StateEvent.From(m_Mouse, out var eventPtr))
                {
                    m_Mouse.leftButton.WriteValueIntoEvent(value, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                }
            }
        }
    }
}
