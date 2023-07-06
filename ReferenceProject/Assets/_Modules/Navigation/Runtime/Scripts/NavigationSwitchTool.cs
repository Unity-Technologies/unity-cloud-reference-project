using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Tools;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Navigation
{
    public class NavigationSwitchTool : ToolUIController
    {
        
        [SerializeField]
        VisualTreeAsset m_ButtonTemplate;
        
        readonly Dictionary<NavigationModeData, ActionButton> m_NavigationModes = new();
        INavigationManager m_NavigationManager;

        [Inject]
        void Setup(INavigationManager navigationManager)
        {
            m_NavigationManager = navigationManager;
        }

        void OnEnable()
        {
            m_NavigationManager.NavigationModeChanged -= OnNavigationModeChanged;
            m_NavigationManager.NavigationModeChanged += OnNavigationModeChanged;
        }

        void OnDisable()
        {
            m_NavigationManager.NavigationModeChanged -= OnNavigationModeChanged;
        }

        public override void OnToolOpened()
        {
            foreach (var (navigationMode, button) in m_NavigationModes)
            {
                button.SetEnabled(navigationMode.CheckDeviceAvailability());
                button.selected = navigationMode == m_NavigationManager.CurrentNavigationModeData;
            }
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);

            var navigationModes = m_NavigationManager.NavigationModes;

            for (var i = 0; i < navigationModes.Length; i++)
            {
                if (navigationModes[i] == null)
                    continue;

                if (navigationModes[i].CheckDeviceCapability())
                {
                    // Add Mode Button
                    var button = SetupVisualElement(navigationModes[i], i);
                    if (i == 0)
                    {
                        button.style.borderTopLeftRadius = button.style.borderTopRightRadius = 4;
                    }
                    else if (i == navigationModes.Length - 1)
                    {
                        button.style.borderBottomLeftRadius = button.style.borderBottomRightRadius = 4;
                    }

                    root.Add(button);
                }
            }

            return root;
        }

        VisualElement SetupVisualElement(NavigationModeData navigationMode, int idMode)
        {
            if (!navigationMode)
                return null;

            var buttonTemplate = m_ButtonTemplate.CloneTree();
            buttonTemplate.tooltip = navigationMode.ModeName;
            var actionButton = buttonTemplate.Q<ActionButton>("ActionButton");
            actionButton.label = navigationMode.ModeName;
            actionButton.icon = navigationMode.Icon.name;
            actionButton.clickable.clicked += () => OnModeClick(idMode);
            m_NavigationModes.Add(navigationMode, actionButton);

            return buttonTemplate;
        }

        void OnNavigationModeChanged()
        {
            var currentNavigationMode = m_NavigationManager.CurrentNavigationModeData;

            if (currentNavigationMode == null)
                return;

            Icon = currentNavigationMode.Icon;
        }

        void OnModeClick(int idMode)
        {
            m_NavigationManager.ChangeNavigationMode(idMode);
            CloseSelf();
        }
    }
}
