using System;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.VR
{
    public class ToolPanelVR : VisualElement
    {
        readonly Heading m_Title;
        readonly Image m_Icon;
        readonly VisualElement m_Header;
        readonly Divider m_Divider;
        readonly Button m_CloseButton;
        readonly ActionButton m_DockButton;

        ToolPanelVR()
        {
            name = "panel";
            AddToClassList("rigui-panel");

            m_Header = new VisualElement { name = "header" };
            m_Header.AddToClassList("tool-panel-header");
            Add(m_Header);

            m_Icon = new Image { name = "image" };
            m_Icon.AddToClassList("tool-panel-icon");
            m_Header.Add(m_Icon);

            m_Title = new Heading { name = "title" };
            m_Title.AddToClassList("tool-panel-title");
            m_Header.Add(m_Title);

            var headerButtons = new VisualElement();
            headerButtons.AddToClassList("rigui-panel-header-button");
            m_Header.Add(headerButtons);

            m_DockButton = new ActionButton
            {
                name = "dock-button",
                icon = "draghandle"
            };
            m_DockButton.AddToClassList("rigui-panel-pin-button");
            m_DockButton.clickable.clicked += OnDockButtonClicked;
            headerButtons.Add(m_DockButton);

            m_CloseButton = new Button
            {
                name = "close-button",
                leadingIcon = "x"
            };
            m_CloseButton.AddToClassList("tool-panel-dismiss-button");
            m_CloseButton.clicked += OnCloseButtonClicked;
            headerButtons.Add(m_CloseButton);

            m_Divider = new Divider { name = "divider" };
            Add(m_Divider);

            SetVisibleHeader(false);
        }

        public event Action DismissRequested;
        public event Action DockRequested;

        public static ToolPanelVR Build(VisualElement contentView)
        {
            var panel = new ToolPanelVR();

            panel.Add(contentView);

            return panel;
        }

        public ToolPanelVR SetVisible(bool value)
        {
            SetVisible(this, value);

            return this;
        }

        public ToolPanelVR SetTitle(string title)
        {
            m_Title.text = title;

            if (!string.IsNullOrEmpty(title))
                SetVisibleHeader(true);

            return this;
        }

        public ToolPanelVR SetIcon(Sprite image)
        {
            m_Icon.sprite = image;

            if (image != null)
                SetVisibleHeader(true);

            return this;
        }

        public ToolPanelVR SetDismissable(bool dismissable)
        {
            if (!dismissable)
            {
                // Do not set m_Button.visible to true to avoid popover display issues.
                m_CloseButton.visible = false;
            }
            else
            {
                SetVisibleHeader(true);
            }

            return this;
        }

        public ToolPanelVR SetDockable(bool dockable)
        {
            if (!dockable)
            {
                SetVisible(m_DockButton, false);
            }
            else
            {
                SetVisibleHeader(true);
            }

            return this;
        }

        public ToolPanelVR SetDocked(bool value)
        {
            m_DockButton.selected = !value;

            return this;
        }

        void OnCloseButtonClicked()
        {
            DismissRequested?.Invoke();
        }

        void OnDockButtonClicked()
        {
            DockRequested?.Invoke();
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
