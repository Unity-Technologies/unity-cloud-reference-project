using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.CustomKeyboard
{
    public class MockKeyboard : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        IKeyboardController m_KeyboardController;
        VisualElement m_KeyboardVisualElement;

        [Inject]
        void Setup(IKeyboardController keyboardController)
        {
            m_KeyboardController = keyboardController;
        }

        void Awake()
        {
            m_KeyboardVisualElement = m_KeyboardController.RootVisualElement;
            m_UIDocument.rootVisualElement.Add(m_KeyboardVisualElement);
            m_UIDocument.rootVisualElement.style.flexDirection = FlexDirection.Column;
            m_UIDocument.rootVisualElement.style.justifyContent = Justify.FlexEnd;
            Utils.SetVisible(m_KeyboardVisualElement, false);

            m_KeyboardController.KeyboardOpened += OnKeyboardOpen;
            m_KeyboardController.KeyboardClosed += OnKeyboardClose;
        }

        void OnKeyboardOpen()
        {
            Utils.SetVisible(m_KeyboardVisualElement, true);
        }

        void OnKeyboardClose()
        {
            Utils.SetVisible(m_KeyboardVisualElement, false);
        }
    }
}
