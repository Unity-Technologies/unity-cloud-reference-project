﻿using System;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.Dt.App.UI.Button;

namespace Unity.ReferenceProject.Tools
{
    public class Panel
    {
        readonly Divider m_Divider;
        readonly VisualElement m_Header;
        readonly Header m_Title;

        public Action OnCloseClicked;

        public Panel(VisualElement rootVisualElement, VisualElement contentVisualElement)
        {
            RootVisualElement = rootVisualElement;
            m_Header = rootVisualElement.Q<VisualElement>("header");
            Icon = rootVisualElement.Q<Image>("icon");
            m_Title = rootVisualElement.Q<Header>("title");
            CloseButton = rootVisualElement.Q<Button>("close-button");
            m_Divider = rootVisualElement.Q<Divider>("divider");

            var content = rootVisualElement.Q<VisualElement>("content");
            if (content != null)
            {
                content.contentContainer.Add(contentVisualElement);
            }
            else
            {
                RootVisualElement.contentContainer.Add(contentVisualElement);
            }

            if (CloseButton != null)
            {
                CloseButton.clicked += () => OnCloseClicked?.Invoke();
            }
        }

        public string Title
        {
            get => m_Title.text;
            set => m_Title.text = value;
        }

        public Image Icon { get; set; }
        public Button CloseButton { get; }
        public VisualElement RootVisualElement { get; }

        public bool Visible
        {
            get => RootVisualElement.style.display == DisplayStyle.Flex;
            set => RootVisualElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public bool HeaderVisible
        {
            get => m_Header.style.display == DisplayStyle.Flex;
            set
            {
                m_Header.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                m_Divider.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
