using Unity.ReferenceProject.InputSystem;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasurementToolUIController : ToolUIController
    {
        [SerializeField]
        MeasurementToolController m_MeasurementToolController;

        [SerializeField]
        MeasureListUIController m_MeasureListUIController;

        IInputManager m_InputManager;
        IAppUnit m_AppUnit;

        [Inject]
        void Setup(IInputManager inputManager, IAppUnit appUnit)
        {
            m_InputManager = inputManager;
            m_AppUnit = appUnit;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            m_MeasurementToolController.InitializeUI(rootVisualElement);
            m_MeasureListUIController.InitializeUI(rootVisualElement);
            return rootVisualElement;
        }

        public override void OnToolOpened()
        {
            m_MeasureListUIController.OnToolOpen();
            m_MeasurementToolController.StartNewLine();

            var line = new MeasureLineData(m_AppUnit.GetSystemUnit(), true);
            m_MeasurementToolController.Add(line);
            m_MeasurementToolController.Edit(line);
        }

        public override void OnToolClosed()
        {
            m_MeasureListUIController.OnToolClose();
            m_MeasurementToolController.RemoveAll();
            m_MeasurementToolController.StopEditing();
        }

        protected override void RegisterCallbacks(VisualElement visualElement)
        {
            var lineName = visualElement.Q("line_creator_name_value");
            lineName.RegisterCallback<FocusInEvent>(UIFocused);
            lineName.RegisterCallback<FocusOutEvent>(UIUnFocused);
        }

        protected override void UnregisterCallbacks(VisualElement visualElement)
        {
            var lineName = visualElement.Q("line_creator_name_value");
            lineName.UnregisterCallback<FocusInEvent>(UIFocused);
            lineName.UnregisterCallback<FocusOutEvent>(UIUnFocused);
        }

        void UIFocused(FocusInEvent ev)
        {
            m_InputManager.IsUIFocused = true;
        }

        void UIUnFocused(FocusOutEvent ev)
        {
            m_InputManager.IsUIFocused = false;
        }
    }
}