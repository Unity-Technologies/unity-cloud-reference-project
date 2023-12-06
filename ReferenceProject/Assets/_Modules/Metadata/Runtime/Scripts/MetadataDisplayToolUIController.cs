using Unity.ReferenceProject.Tools;
using Unity.ReferenceProject.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Metadata
{
    public class MetadataDisplayToolUIController : ToolUIController
    {
        [SerializeField]
        MetadataDisplayController m_Controller;
        IInputManager m_InputManager;

        [Inject]
        void Setup(IInputManager inputManager)
        {
            m_InputManager = inputManager;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            m_Controller.Initialize(rootVisualElement);

            return rootVisualElement;
        }

        protected override void RegisterCallbacks(VisualElement visualElement)
        {
            var searchInput = visualElement.Q("search-input");
            searchInput.RegisterCallback<FocusInEvent>(UIFocused);
            searchInput.RegisterCallback<FocusOutEvent>(UIUnFocused);
        }

        void UIFocused(FocusInEvent ev)
        {
            m_InputManager.IsUIFocused = true;
        }

        void UIUnFocused(FocusOutEvent ev)
        {
            m_InputManager.IsUIFocused = false;
        }

        protected override void UnregisterCallbacks(VisualElement visualElement)
        {
            var searchInput = visualElement.Q("search-input");
            searchInput.UnregisterCallback<FocusInEvent>(OnFocusIn);
            searchInput.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            var parameterList = visualElement.Q("ParameterList");
            parameterList.UnregisterCallback<PointerCaptureEvent>(OnPointerCaptureEvent);
            parameterList.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOutEvent);
        }

        public override void OnToolOpened()
        {
            if (m_Controller)
            {
                m_Controller.OpenTool();
            }
        }

        public override void OnToolClosed()
        {
            if (m_Controller)
            {
                m_Controller.CloseTool();
            }
        }
    }
}
