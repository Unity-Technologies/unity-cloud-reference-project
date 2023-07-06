using System;
using Unity.ReferenceProject.Identity;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VR
{
    [Serializable]
    public class LoginStateVR : AppStateWorldSpacePanel
    {
        [SerializeField]
        VisualTreeAsset m_Background;

        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [SerializeField]
        IdentityUIController m_IdentityUIController;

        protected override string PanelName => "LoginPanel";

        protected override Vector2 PanelSize => new Vector2(1366, 768);

        protected override void OnPanelBuilt(UIDocument document)
        {
            var appUIPanel = document.rootVisualElement.Q<Panel>();

            var background = m_Background.Instantiate();
            background.contentContainer.style.flexGrow = 1.0f;
            appUIPanel.Add(background);

            var loginVisualElement = m_VisualTreeAsset.Instantiate();
            loginVisualElement.contentContainer.style.flexGrow = 1.0f;
            loginVisualElement.contentContainer.AddToClassList("stretch");
            appUIPanel.Add(loginVisualElement);

            m_IdentityUIController.InitUIToolkit(loginVisualElement);
        }
    }
}