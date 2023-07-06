using System;
using Unity.ReferenceProject.VR.RigUI;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class ThemeSettingVR : ThemeSetting
    {
        IPanelManager m_PanelManager;

        [Inject]
        void Setup(IPanelManager panelManager)
        {
            m_PanelManager = panelManager;
            m_PanelManager.OnPanelCreated += OnPanelCreated;
        }

        protected override void UpdateTheme()
        {
            var panels = m_PanelManager.Panels;

            foreach (var panel in panels)
            {
                SetPanelTheme(panel);
            }
        }

        void SetPanelTheme(PanelController panel)
        {
            var root = panel.Root;
            var appUIPanel = root.Q<Panel>();

            if (appUIPanel != null)
            {
                appUIPanel.theme = m_CurrentTheme;
            }
        }

        void OnPanelCreated(PanelController panel)
        {
           SetPanelTheme(panel);
        }
    }
}
