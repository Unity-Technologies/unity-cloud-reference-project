using System;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.Identity
{
    public class ProfileToolUIController : ToolUIController
    {
        [SerializeField]
        AppState m_LogoutState;

        [Header("UXML")]
        [SerializeField]
        string m_UserNameHeaderElement = "profile-tool-username";
        
        [SerializeField]
        string m_LogoutButtonElement = "profile-tool-button";
        
        IAppStateController m_AppStateController;

        IUrlRedirectionAuthenticator m_Authenticator;
        IUserInfoProvider m_UserInfoProvider;
       
        Heading m_UserName;
        Button m_Button;

        [Inject]
        void Setup(IUrlRedirectionAuthenticator authenticator, IUserInfoProvider userInfoProvider, IAppStateController appStateController)
        {
            m_Authenticator = authenticator;
            m_UserInfoProvider = userInfoProvider;
            m_AppStateController = appStateController;
        }

        void OnEnable()
        {
            m_Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            OnAuthenticationStateChanged(m_Authenticator.AuthenticationState);
        }

        void OnDisable()
        {
            m_Authenticator.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);

            m_UserName = root.Q<Heading>(m_UserNameHeaderElement);
            m_Button = root.Q<Button>(m_LogoutButtonElement);
            
            m_Button.clickable.clicked += Logout;

            return root;
        }

        async void Logout()
        {
            await m_Authenticator.LogoutAsync();
            CloseSelf();

            m_AppStateController.PrepareTransition(m_LogoutState).Apply();
        }

        async void OnAuthenticationStateChanged(AuthenticationState obj)
        {
            if (obj == AuthenticationState.LoggedIn)
            {
                var userInfo = await m_UserInfoProvider.GetUserInfoAsync();

                if (m_UserName != null)
                    m_UserName.text = userInfo.Name;

                m_Button?.SetEnabled(true);
            }
            else
            {
                if (m_UserName != null)
                    m_UserName.text = "-";

                m_Button?.SetEnabled(false);
            }
        }
    }
}
