using System;
using System.Collections;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.DeepLinking;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    [Serializable]
    public sealed class WaitAuthenticationState : AppState
    {
        // Use this to prevent the initialization to be too fast and avoid a flashing Splash Screen
        [SerializeField]
        float m_MinimumDuration = 2.0f;

        [SerializeField]
        AppState m_LoggedInState;

        [SerializeField]
        AppState m_LoggedOutState;

        IAuthenticator m_Authenticator;

        bool m_TimerFinished;
        
        DeepLinkData m_DeepLinkData;

        [Inject]
        public void Setup(IAuthenticator authenticator, DeepLinkData deepLinkData)
        {
            m_Authenticator = authenticator;
            m_DeepLinkData = deepLinkData;
        }

        void Awake()
        {
            if (m_Authenticator.AuthenticationState != AuthenticationState.LoggedIn)
            {
                m_Authenticator.InitializeAsync();
            }
        }

        void Start()
        {
            StartCoroutine(StartTimer(m_MinimumDuration));
        }

        IEnumerator StartTimer(float duration)
        {
            m_TimerFinished = false;
            yield return new WaitForSeconds(duration);
            m_TimerFinished = true;
            CheckForStateChange();
        }

        protected override void EnterStateInternal()
        {
            m_Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            // Force update
            CheckForStateChange();
        }

        void CheckForStateChange()
        {
            OnAuthenticationStateChanged(m_Authenticator.AuthenticationState);
        }

        protected override void ExitStateInternal()
        {
            m_Authenticator.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        void OnAuthenticationStateChanged(AuthenticationState state)
        {
            if (!m_TimerFinished)
                return;

            if (state == AuthenticationState.LoggedIn)
            {
                if (m_DeepLinkData.DeepLinkIsProcessing)
                {
                    m_DeepLinkData.DeepLinkIsProcessing = false;
                }
                else
                {
                    AppStateController.PrepareTransition(m_LoggedInState).Apply();
                }
            }
            else if (state == AuthenticationState.LoggedOut)
            {
                AppStateController.PrepareTransition(m_LoggedOutState).Apply();
            }
        }
    }
}
