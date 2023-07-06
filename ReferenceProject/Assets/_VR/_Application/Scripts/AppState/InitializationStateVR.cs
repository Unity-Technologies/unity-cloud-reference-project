using System;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VR
{
    public class InitializationStateVR : AppStateWorldSpacePanel
    {
        [SerializeField]
        VisualTreeAsset m_Background;

        [SerializeField]
        VisualTreeAsset m_SplashScreen;

        protected override string PanelName => "InitializationPanel";

        protected override Vector2 PanelSize => new Vector2(1366, 768);

        protected override void StateExited()
        {
            // Do not destroy the panel when exiting the state
        }

        protected override void OnPanelBuilt(UIDocument document)
        {
            var appUIPanel = document.rootVisualElement.Q<Panel>();

            var background = m_Background.Instantiate();
            background.contentContainer.style.flexGrow = 1.0f;
            appUIPanel.Add(background);

            var splashScreen = m_SplashScreen.Instantiate();
            splashScreen.contentContainer.style.flexGrow = 1.0f;
            splashScreen.contentContainer.AddToClassList("stretch");
            appUIPanel.Add(splashScreen);
        }
    }
}
