using System;
using System.Collections;
using Unity.ReferenceProject.DeepLinking;
using Unity.ReferenceProject.Identity;
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

        ICloudSession m_Session;

        DeepLinkData m_DeepLinkData;

        [Inject]
        public void Setup(ICloudSession session, DeepLinkData deepLinkData)
        {
            m_Session = session;
            m_DeepLinkData = deepLinkData;
        }

        protected override void EnterStateInternal()
        {
            if (!m_Session.Initialized)
            {
                _ = m_Session.Initialize();
            }
            StartCoroutine(WaitForInitialization());
        }

        IEnumerator WaitForInitialization()
        {
            float time = Time.realtimeSinceStartup;
            while (!m_Session.Initialized)
            {
                yield return new WaitForEndOfFrame();
            }
            float elapsed = Time.realtimeSinceStartup - time;
            if (elapsed < m_MinimumDuration)
            {
                yield return new WaitForSeconds(m_MinimumDuration - elapsed);
            }
            CheckLoginState();
        }

        void CheckLoginState()
        {
            if (m_Session.State == SessionState.LoggedIn)
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
            else if (m_Session.State == SessionState.LoggedOut)
            {
                AppStateController.PrepareTransition(m_LoggedOutState).Apply();
            }
        }
    }
}
