using System;
using Unity.ReferenceProject.Identity;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;

namespace Unity.ReferenceProject
{
    [Serializable]
    public sealed class LoginState : AppState
    {
        [SerializeField]
        AppState m_LoggedInState;

        [SerializeField]
        IdentityUIController m_IdentityUIController;

        protected override void EnterStateInternal()
        {
            m_IdentityUIController.LoggedIn += OnLoggedIn;
        }

        protected override void ExitStateInternal()
        {
            m_IdentityUIController.LoggedIn -= OnLoggedIn;
        }

        void OnLoggedIn()
        {
            AppStateController.PrepareTransition(m_LoggedInState).Apply();
        }
    }
}
