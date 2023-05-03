using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools
{
    [CreateAssetMenu(menuName = "ReferenceProject/ToolUIMode/ToolUIModePanel")]
    public class ToolUIModePanel : ToolUIMode
    {
        public List<StyleSheet> AdditionalStyles;

        public string PanelStyleClass;
        [FormerlySerializedAs("PanelHeader")]
        public bool DisplayTitle;
        [FormerlySerializedAs("PanelHeaderIcon")]
        public bool DisplayIcon;
        [FormerlySerializedAs("PanelHeaderCloseButton")]
        public bool DisplayCloseButton;

        public override IToolUIModeHandler CreateHandler()
        {
            return new ToolUIModePanelHandler(this);
        }
    }

    class ToolUIModePanelHandler : ToolUIModeHandler
    {
        readonly List<StyleSheet> m_AdditionalStyles;
        readonly bool m_DisplayCloseButton;
        readonly bool m_DisplayIcon;
        readonly bool m_DisplayTitle;

        readonly string m_StyleClass;

        public ToolUIModePanelHandler(ToolUIModePanel toolUIModePanel)
        {
            m_AdditionalStyles = toolUIModePanel.AdditionalStyles;
            m_StyleClass = toolUIModePanel.PanelStyleClass;
            m_DisplayTitle = toolUIModePanel.DisplayTitle;
            m_DisplayIcon = toolUIModePanel.DisplayIcon;
            m_DisplayCloseButton = toolUIModePanel.DisplayCloseButton;
        }

        protected ToolPanel Panel { private set; get; }

        protected override VisualElement CreateVisualTreeInternal()
        {
            Panel = ToolPanel.Build(ToolUIController.RootVisualElement)
                .SetTitle(m_DisplayTitle ? ToolUIController.DisplayName : null)
                .SetIcon(m_DisplayIcon ? ToolUIController.Icon : null)
                .SetDismissable(m_DisplayCloseButton);

            Panel.SetVisible(false);

            foreach (var style in m_AdditionalStyles)
            {
                Panel.styleSheets.Add(style);
            }

            if (!string.IsNullOrWhiteSpace(m_StyleClass))
            {
                Panel.AddToClassList(m_StyleClass);
            }

            Panel.dismissRequested += _ => CloseTool();

            return Panel;
        }

        protected override void OnToolOpenedInternal()
        {
            Panel.SetVisible(true);
        }

        protected override void OnToolClosedInternal()
        {
            Panel.SetVisible(false);
        }
    }
}
