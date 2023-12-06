using System.Collections;
using System.Threading.Tasks;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Identity;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class LogoutState : AppState
    {
        [SerializeField]
        float m_MinimumDuration = 1.0f;

        [SerializeField]
        AppState m_LoginState;

        IAppMessaging m_AppMessaging;
        ICloudSession m_CloudSession;
        Toast m_LogoutToast;

        [Inject]
        void Setup(IAppMessaging appMessaging, ICloudSession session)
        {
            m_AppMessaging = appMessaging;
            m_CloudSession = session;
        }

        protected override void EnterStateInternal()
        {
            m_LogoutToast = m_AppMessaging.ShowInfo("@Identity:LoggingOut", true);
            m_LogoutToast.dismissed += OnToastDismissed;
            if(m_CloudSession.State == SessionState.LoggedOut)
            {
                StartCoroutine(Wait());
            }
            else
            {
                _ = LoggingOut();
            }
        }

        protected override void ExitStateInternal()
        {
            m_AppMessaging.ShowSuccess("@Identity:LogoutSuccess");
        }

        void OnToastDismissed(Toast toast, DismissType type)
        {
            m_LogoutToast = null;
        }

        async Task LoggingOut()
        {
            await m_CloudSession.Logout();
            StartCoroutine(Wait());
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(m_MinimumDuration);
            m_AppMessaging.DismissToast(m_LogoutToast);
            AppStateController.PrepareTransition(m_LoginState).Apply();
        }
    }
}
