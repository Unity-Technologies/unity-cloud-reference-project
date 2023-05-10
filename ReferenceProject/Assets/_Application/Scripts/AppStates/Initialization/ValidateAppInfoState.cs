using System;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    [Serializable]
    public sealed class ValidateAppInfoState : AppState
    {
        [SerializeField]
        AppState m_NextState;

        IAppInfoProvider m_AppInfoProvider;
        ServiceHostConfiguration m_CloudConfiguration;
        IAppMessaging m_AppMessaging;

        [Inject]
        public void Setup(IServiceHttpClient serviceHttpClient, ServiceHostConfiguration cloudConfiguration, IAppMessaging appMessaging)
        {
            m_AppInfoProvider = new AppInfoProvider(serviceHttpClient, cloudConfiguration);
            m_CloudConfiguration = cloudConfiguration;
            m_AppMessaging = appMessaging;
        }

        protected override void EnterStateInternal()
        {
            _ = ValidateAppId(m_AppInfoProvider);
        }

        async Task ValidateAppId(IAppInfoProvider appInfoProvider)
        {
            var appId = UnityCloudPlayerSettings.Instance.AppId;

            var prefix = $"[{m_CloudConfiguration?.ResolveEnvironment().environment}]";

            if (string.IsNullOrEmpty(appId))
            {
                m_AppMessaging.ShowWarning($"{prefix} Application Id was not set. (Unity Cloud App Registration)", true);
            }
            else
            {
                try
                {
                    var info = await appInfoProvider.GetAppInfoAsync(appId);

                    if (UnityCloudPlayerSettings.Instance.AppName != info.Name ||
                        UnityCloudPlayerSettings.Instance.AppDisplayName != info.DisplayName)
                    {
                        m_AppMessaging.ShowWarning($"{prefix} Application Id, Name or Display Name don't match Unity Cloud App Registration info.", true);
                    }
                }
                catch (NotFoundException)
                {
                    m_AppMessaging.ShowWarning($"{prefix} The provided Application Id was not found on the Unity Cloud Service.", true);
                }
                catch (Exception e)
                {
                    m_AppMessaging.ShowException(e);
                    m_AppMessaging.ShowError($"{prefix} An error occured when validating Application Info.", true);
                }
            }

            AppStateController.PrepareTransition(m_NextState).Apply();
        }
    }
}