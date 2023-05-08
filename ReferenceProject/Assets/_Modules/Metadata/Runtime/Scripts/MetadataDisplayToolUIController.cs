using System;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Metadata
{
    public class MetadataDisplayToolUIController : ToolUIController
    {
        [SerializeField]
        MetadataDisplayController m_Controller;

        IAppMessaging m_AppMessaging;

        [Inject]
        void Setup(IAppMessaging appMessaging)
        {
            m_AppMessaging = appMessaging;
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
                m_AppMessaging.ShowDialog("Incomplete Feature", "Metadata Tool relies on com.unity.cloud.data-streaming's Object Picker, which is not implemented yet in current version. " +
                    "Selecting object will not work.", "Ok");
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
