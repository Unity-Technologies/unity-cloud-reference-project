using System;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Metadata
{
    public class MetadataDisplayToolUIController : ToolUIController
    {
        [SerializeField]
        MetadataDisplayController m_Controller;

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            m_Controller.Initialize(rootVisualElement);

            return rootVisualElement;
        }

        protected override void RegisterCallbacks(VisualElement visualElement)
        {
            var searchInput = visualElement.Q("search-input");
            searchInput.RegisterCallback<FocusInEvent>(OnFocusIn);
            searchInput.RegisterCallback<FocusOutEvent>(OnFocusOut);

            var parameterList = visualElement.Q("ParameterList");
            parameterList.RegisterCallback<PointerEnterEvent>(OnPointerEntered);
            parameterList.RegisterCallback<PointerLeaveEvent>(OnPointerExited);
        }

        protected override void UnregisterCallbacks(VisualElement visualElement)
        {
            var searchInput = visualElement.Q("search-input");
            searchInput.UnregisterCallback<FocusInEvent>(OnFocusIn);
            searchInput.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            var parameterList = visualElement.Q("ParameterList");
            parameterList.UnregisterCallback<PointerEnterEvent>(OnPointerEntered);
            parameterList.UnregisterCallback<PointerLeaveEvent>(OnPointerExited);
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
