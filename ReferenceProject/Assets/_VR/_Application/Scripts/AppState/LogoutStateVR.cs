using Unity.AppUI.UI;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class LogoutStateVR : AppStateWorldSpacePanel
    {
        [SerializeField]
        VisualTreeAsset m_Background;

        [SerializeField]
        VisualTreeAsset m_SplashScreen;

        [SerializeField]
        MenuType m_LogoutMenuType;

        PropertyValue<MenuType> m_MenuTypeProperty;

        protected override string PanelName => "Logout";

        protected override Vector2 PanelSize => new Vector2(1366, 768);


        [Inject]
        void Setup(PropertyValue<MenuType> menuType)
        {
            m_MenuTypeProperty = menuType;
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

        protected override void StateEntered()
        {
            m_MenuTypeProperty.SetValue(m_LogoutMenuType);
            base.StateEntered();
        }

        protected override void StateExited()
        {
            //Empty to keep panel alive as the login state need it
        }
    }
}
