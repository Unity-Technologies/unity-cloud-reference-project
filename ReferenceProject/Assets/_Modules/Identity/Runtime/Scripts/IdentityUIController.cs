using System;
using System.Threading;
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

        ICloudSession m_Session;

        bool m_IsRetrying;
        VisualElement m_LoadingIndicator;
        Button m_LoginLogoutButton;

        IAppMessaging m_AppMessaging;

        [Inject]
        public void Setup(ICloudSession session, IAppMessaging appMessaging)
        {
            m_Session = session;
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
                m_LoginLogoutButton.clicked -= OnLogoutClicked;
            }

            if (m_Session != null)
            {
                m_Session.SessionStateChanged -= OnAuthenticationStateChanged;
            }
        }

        public void InitUIToolkit(VisualElement rootVisualElement)
        {
            m_LoginLogoutButton = rootVisualElement.Q<Button>(m_LoginButtonElement);
            m_LoadingIndicator = rootVisualElement.Q<VisualElement>(m_IndicatorElement);

            Utils.SetVisible(m_LoadingIndicator, false);

            m_Session.SessionStateChanged += OnAuthenticationStateChanged;
            m_LoginLogoutButton.clicked += ApplyLogState;
        }

        void OnLogoutClicked()
        {
            ApplyLogState();
        }

        async void ApplyLogState()
        {
            try
            {
                var state = m_Session.State;

                switch (state)
                {
                    case SessionState.LoggingIn:
                    {
                        m_IsRetrying = true;
                        m_Session.CancelLogin();
                        break;
                    }

                    case SessionState.LoggedIn:
                    {
                        await m_Session.Logout();
                        break;
                    }

                    case SessionState.LoggedOut:
                    {
                        await m_Session.Login();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                m_AppMessaging.ShowException(e);
            }
        }

        void OnAuthenticationStateChanged(SessionState state)
        {
            switch (state)
            {
                case SessionState.LoggedIn:
                {
                    m_LoginLogoutButton.title = m_LogoutString;
                    m_LoginLogoutButton.SetEnabled(false);
                    Utils.SetVisible(m_LoadingIndicator, false);
                    LoggedIn?.Invoke();
                    break;
                }

                case SessionState.LoggingIn:
                {
                    m_LoginLogoutButton.title = m_RetryString;
                    Utils.SetVisible(m_LoadingIndicator, true);
                    break;
                }

                case SessionState.LoggedOut:
                {
                    if (m_IsRetrying)
                    {
                        m_IsRetrying = false;
                        ApplyLogState();
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
