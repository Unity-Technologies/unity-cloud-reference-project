using System;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
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
            parameterList.RegisterCallback<PointerCaptureEvent>(OnPointerCaptureEvent);
            parameterList.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOutEvent);
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
