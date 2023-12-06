using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cloud.Identity;

namespace Unity.ReferenceProject.Identity
{
    public sealed class CloudSession : ICloudSession
    {
        UserData m_UserData = null;
        SessionState m_state = SessionState.LoggedOut;
        readonly HashSet<Func<Task>> m_loggingInCallbacks = new();
        readonly HashSet<Func<Task>> m_loggingOutCallbacks = new();
        readonly CompositeAuthenticator m_Authenticator;
        bool m_Initialized = false;

        public event Action<SessionState> SessionStateChanged;
        public SessionState State => m_state;
        public bool Initialized => m_Initialized;
        public IUserData UserData => m_UserData;

        public CloudSession(CompositeAuthenticator auth)
        {
            m_Authenticator = auth;
            m_Authenticator.AuthenticationStateChanged += AuthenticatonStateChanged;
        }

        public async Task Initialize()
        {
            await m_Authenticator.InitializeAsync();
            UpdateUserData();
            m_Initialized = true;
        }

        public void UpdateUserData()
        {
            if(m_UserData == null)
            {
                m_UserData = new UserData(m_Authenticator.GetUserInfo(AuthenticatedUserInfoClaims.Id), m_Authenticator.GetUserInfo(AuthenticatedUserInfoClaims.Name), UnityEngine.Color.grey);
            }
            else
            {
                m_UserData.Id = m_Authenticator.GetUserInfo(AuthenticatedUserInfoClaims.Id);
                m_UserData.Name = m_Authenticator.GetUserInfo(AuthenticatedUserInfoClaims.Name);
            }
        }

        public async Task<bool> Logout()
        {
            if (m_state != SessionState.LoggedIn || !m_Initialized)
                return false;

            await ChangeState(SessionState.LoggingOut);

            await m_Authenticator.LogoutAsync();

            m_UserData.Id = "";
            m_UserData.Name = "-";

            if (m_Authenticator.AuthenticationState == AuthenticationState.LoggedOut)
            {
                await ChangeState(SessionState.LoggedOut);
                return true;
            }
            else if(m_Authenticator.AuthenticationState == AuthenticationState.LoggedIn)
            {
                await ChangeState(SessionState.LoggedIn);
                return false;
            }

            return false;
        }

        void AuthenticatonStateChanged(AuthenticationState state)
        {
            switch (state)
            {
                case AuthenticationState.LoggedOut when m_state == SessionState.LoggedIn:
                    {
                        _ = ChangeState(SessionState.LoggedOut);
                    }break;
                case AuthenticationState.LoggedIn when m_state == SessionState.LoggedOut:
                    {
                        _ = FastLogIn();
                    }
                    break;
                default: return;
            }
        }

        async Task FastLogIn()
        {
            await ChangeState(SessionState.LoggingIn);
            UpdateUserData();
            await ChangeState(SessionState.LoggedIn);
        }

        public void CancelLogin()
        {
            if (m_state != SessionState.LoggingIn)
                return;

            m_Authenticator.CancelLogin();
        }

        public async Task<bool> Login()
        {
            if (m_state != SessionState.LoggedOut || !m_Initialized)
                return false;
            
            await ChangeState(SessionState.LoggingIn);

            if(m_Authenticator.AuthenticationState == AuthenticationState.LoggedOut)
            {
                await m_Authenticator.LoginAsync();
            }
            else
            {
                UnityEngine.Debug.Log("Already loggedin");
            }

            UpdateUserData();

            if (m_Authenticator.AuthenticationState == AuthenticationState.LoggedIn)
            {
                await ChangeState(SessionState.LoggedIn);
                return true;
            }
            else
            {
                await ChangeState(SessionState.LoggedOut);
                return false;
            }
        }

        public void RegisterLoggingOutCallback(Func<Task> callback)
        {
            m_loggingOutCallbacks.Add(callback);
        }

        public void UnRegisterLoggingOutCallback(Func<Task> callback)
        {
            m_loggingOutCallbacks.Remove(callback);
        }

        public void RegisterLoggedInCallback(Func<Task> callback)
        {
            m_loggingInCallbacks.Add(callback);
        }

        public void UnRegisterLoggedInCallback(Func<Task> callback)
        {
            m_loggingInCallbacks.Remove(callback);
        }

        async Task InvokeLoggingOut()
        {
            foreach (Func<Task> callback in m_loggingOutCallbacks)
            {
                await callback.Invoke();
            }
        }

        async Task InvokeLoggingIn()
        {
            foreach (Func<Task> callback in m_loggingInCallbacks)
            {
                await callback.Invoke();
            }
        }

        async Task ChangeState(SessionState state)
        {
            m_state = state;
            
            SessionStateChanged?.Invoke(m_state);

            switch (m_state)
            {
                case SessionState.LoggedIn:
                    await InvokeLoggingIn();
                    break;
                case SessionState.LoggingOut:
                    await InvokeLoggingOut();
                    break;
            }
        }
    }
}
