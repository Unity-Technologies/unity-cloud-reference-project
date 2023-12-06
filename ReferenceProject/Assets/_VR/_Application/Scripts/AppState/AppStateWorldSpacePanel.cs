using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.CustomKeyboard;
using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public abstract class AppStateWorldSpacePanel : AppStateListener
    {
        IRigUIController m_RigUIController;
        IPanelManager m_PanelManager;
        IKeyboardController m_KeyboardController;
        Mouse m_Mouse;

        PanelController m_Panel;

        protected abstract string PanelName { get; }

        protected abstract Vector2 PanelSize { get; }

        [Inject]
        void Setup(IRigUIController rigUIController, IPanelManager panelManager, IKeyboardController keyboardController, Mouse mouse)
        {
            m_RigUIController = rigUIController;
            m_PanelManager = panelManager;
            m_KeyboardController = keyboardController;
            m_Mouse = mouse;
        }

        protected override void StateEntered()
        {
            var dockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(PanelSize);
            dockedPanel.name = PanelName;
            dockedPanel.DockPoint = m_RigUIController.DockPoint;
            dockedPanel.transform.localPosition += (PanelSize.y / 2f) / 1000f * Vector3.up + 0.01f * Vector3.forward;
            OnPanelBuilt(dockedPanel.UIDocument);
            m_Panel = dockedPanel;
        }

        protected override void StateExited()
        {
            if (m_Panel != null)
            {
                m_PanelManager.DestroyPanel(m_Panel);
            }
        }

        protected virtual void OnPanelBuilt(UIDocument document)
        {
            document.rootVisualElement.Query< TextField >().ForEach(textInput =>
            {
                textInput.RegisterCallback<FocusEvent>(evt =>
                {
                    m_KeyboardController.OpenKeyboard(textInput);
                });

                textInput.RegisterCallback<PointerDownEvent>(evt =>
                {
                    MouseClick(textInput, 1f);
                });

                textInput.RegisterCallback<PointerUpEvent>(evt =>
                {
                   MouseClick(textInput, 0f);
                });
            });
        }

        void MouseClick(TextField textInput, float value)
        {
            // This is necessary because moving cursor in text field only work with mouse events
            if (Utils.IsFocused(textInput))
            {
                using (StateEvent.From(m_Mouse, out var eventPtr))
                {
                    m_Mouse.leftButton.WriteValueIntoEvent(value, eventPtr);
                    UnityEngine.InputSystem.InputSystem.QueueEvent(eventPtr);
                }
            }
        }
    }
}
