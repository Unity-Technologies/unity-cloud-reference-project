using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Tools;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Navigation
{
    public class NavigationSwitchTool : ToolUIController
    {
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

            var button = new ActionButton();
            button.style.justifyContent = Justify.SpaceBetween;
            button.label = navigationMode.ModeName;
            button.icon = "notnull";
            var icon = (Icon)button.hierarchy.ElementAt(0);
            icon.size = IconSize.M;
            icon.sprite = navigationMode.Icon;
            icon.style.marginRight = 12;
            button.quiet = true;
            button.style.borderTopWidth = button.style.borderBottomWidth = button.style.borderLeftWidth = button.style.borderRightWidth = 0;
            button.style.borderTopLeftRadius = button.style.borderTopRightRadius = button.style.borderBottomLeftRadius = button.style.borderBottomRightRadius = 0;
            button.clickable.clicked += () => OnModeClick(idMode);

            m_NavigationModes.Add(navigationMode, button);

            return button;
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
