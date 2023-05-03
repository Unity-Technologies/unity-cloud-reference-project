using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools
{
    [CreateAssetMenu(menuName = "ReferenceProject/ToolUIMode/ToolUIModeSimple")]
    public class ToolUIModeSimple : ToolUIMode
    {
        public override IToolUIModeHandler CreateHandler()
        {
            return new ToolUIModeSimpleHandler();
        }
    }

    public class ToolUIModeSimpleHandler : ToolUIModeHandler
    {
        VisualElement m_RootVisualElement;

        protected override VisualElement CreateVisualTreeInternal()
        {
            m_RootVisualElement = new VisualElement { name = ToolUIController.DisplayName };
            return m_RootVisualElement;
        }

        protected override void OnToolOpenedInternal()
        {
            SetVisible(m_RootVisualElement, true);
        }

        protected override void OnToolClosedInternal()
        {
            SetVisible(m_RootVisualElement, false);
        }

        static void SetVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
