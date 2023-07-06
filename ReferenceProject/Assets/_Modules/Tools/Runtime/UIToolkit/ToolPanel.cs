using System;
using Unity.AppUI.Core;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.Tools
{
    public class ToolPanel : VisualElement, IDismissInvocator
    {
        readonly Button m_Button;
        readonly Divider m_Divider;
        readonly VisualElement m_Header;
        readonly Image m_Icon;
        readonly Heading m_Title;

        ToolPanel()
        {
            name = "panel";
            AddToClassList("tool-panel");

            m_Header = new VisualElement { name = "header" };
            m_Header.AddToClassList("tool-panel-header");
            Add(m_Header);

            m_Icon = new Image { name = "image" };
            m_Icon.AddToClassList("tool-panel-icon");
            m_Header.Add(m_Icon);

            m_Title = new Heading { name = "title" };
            m_Title.AddToClassList("tool-panel-title");
            m_Header.Add(m_Title);

            m_Button = new Button
            {
                name = "close-button",
                leadingIcon = "x"
            };
            m_Button.AddToClassList("tool-panel-dismiss-button");
            m_Button.clicked += OnCloseButtonClicked;
            m_Header.Add(m_Button);

            m_Divider = new Divider { name = "divider" };
            Add(m_Divider);

            SetVisibleHeader(false);
        }

        public event Action<DismissType> dismissRequested;

        public static ToolPanel Build(VisualElement contentView)
        {
            var panel = new ToolPanel();

            panel.Add(contentView);

            return panel;
        }

        public void SetVisible(bool value)
        {
            SetVisible(this, value);
        }

        public ToolPanel SetTitle(string title)
        {
            m_Title.text = title;

            if (!string.IsNullOrEmpty(title))
                SetVisibleHeader(true);

            return this;
        }

        public ToolPanel SetIcon(Sprite image)
        {
            m_Icon.sprite = image;

            if (image != null)
                SetVisibleHeader(true);

            return this;
        }

        public ToolPanel SetDismissable(bool dismissable)
        {
            if (!dismissable)
            {
                // Do not set m_Button.visible to true to avoid popover display issues.
                m_Button.visible = false;
            }
            else
            {
                SetVisibleHeader(true);
            }

            return this;
        }

        void OnCloseButtonClicked()
        {
            dismissRequested?.Invoke(DismissType.Manual);
        }

        void SetVisibleHeader(bool isVisible)
        {
            SetVisible(m_Header, isVisible);
            SetVisible(m_Divider, isVisible);
        }

        static void SetVisible(VisualElement element, bool value)
        {
            element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
