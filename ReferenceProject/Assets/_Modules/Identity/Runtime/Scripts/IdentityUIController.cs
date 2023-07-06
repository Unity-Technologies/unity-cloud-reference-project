using System;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.Identity
{
    public class IdentityUIController : MonoBehaviour
    {
        [Header("UXML")]
        [SerializeField]
        string m_LoginButtonElement = "login-button";
        
        [SerializeField]
        string m_IndicatorElement = "loading-indicator";
        
        [Header("Localization")]
        [SerializeField]
        string m_LoginString = "@Identity:Login";
        
        [SerializeField]
        string m_LogoutString = "@Identity:Logout";
        
        [SerializeField]
        string m_RetryString = "@Identity:Retry";
        
        public event Action LoggedIn;
        public event Action LoggedOut;

        IUrlRedirectionAuthenticator m_Authenticator;

        AuthenticationState m_CurrentState;

        bool m_IsRetrying;
        VisualElement m_LoadingIndicator;
        Button m_LoginLogoutButton;

        IAppMessaging m_AppMessaging;

        [Inject]
        public void Setup(IUrlRedirectionAuthenticator authenticator, IAppMessaging appMessaging)
        {
            m_Authenticator = authenticator;
            m_AppMessaging = appMessaging;
        }

        void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                InitUIToolkit(uiDocument.rootVisualElement);
            }
        }

        void OnDestroy()
        {
            if (m_LoginLogoutButton != null)
            {
                m_LoginLogoutButton.clicked -= LoginLogout;
            }

            if (m_Authenticator != null)
            {
                m_Authenticator.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            }
        }

        public void InitUIToolkit(VisualElement rootVisualElement)
        {
            m_LoginLogoutButton = rootVisualElement.Q<Button>(m_LoginButtonElement);
            m_LoadingIndicator = rootVisualElement.Q<VisualElement>(m_IndicatorElement);

            Utils.SetVisible(m_LoadingIndicator, false);

            m_Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            m_LoginLogoutButton.clicked += LoginLogout;
        }

        async void LoginLogout()
        {
            try
            {
                var state = m_Authenticator.AuthenticationState;

                switch (state)
                {
                    case AuthenticationState.AwaitingLogin:
                    {
                        m_Authenticator.CancelLogin();
                        m_IsRetrying = true;
                        break;
                    }

                    case AuthenticationState.LoggedIn:
                    {
                        await m_Authenticator.LogoutAsync();
                        break;
                    }

                    case AuthenticationState.LoggedOut:
                    {
                        await m_Authenticator.LoginAsync();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                m_AppMessaging.ShowException(e);
            }
        }

        void OnAuthenticationStateChanged(AuthenticationState state)
        {
            switch (state)
            {
                case AuthenticationState.LoggedIn:
                {
                    m_LoginLogoutButton.title = m_LogoutString;
                    m_LoginLogoutButton.SetEnabled(false);
                    Utils.SetVisible(m_LoadingIndicator, false);
                    LoggedIn?.Invoke();
                    break;
                }

                case AuthenticationState.AwaitingLogin:
                {
                    m_LoginLogoutButton.title = m_RetryString;
                    Utils.SetVisible(m_LoadingIndicator, true);
                    break;
                }

                case AuthenticationState.LoggedOut:
                {
                    if (m_IsRetrying)
                    {
                        m_IsRetrying = false;
                        LoginLogout();
                        break;
                    }

                    m_LoginLogoutButton.title = m_LoginString;
                    Utils.SetVisible(m_LoadingIndicator, false);
                    LoggedOut?.Invoke();
                    break;
                }

                default:
                {
                    m_LoginLogoutButton.SetEnabled(true);
                    Utils.SetVisible(m_LoadingIndicator, false);
                    break;
                }
            }
        }
    }
}
