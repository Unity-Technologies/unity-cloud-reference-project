using System;
using Unity.Cloud.AppLinking.Runtime;
using Unity.Cloud.Common;
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
        
        IServiceHostResolver m_ServiceHostResolver;
        IAppMessaging m_AppMessaging;

        [Inject]
        public void Setup(IServiceHostResolver cloudConfiguration, IAppMessaging appMessaging)
        {
            m_ServiceHostResolver = cloudConfiguration;
            m_AppMessaging = appMessaging;
        }

        protected override void EnterStateInternal()
        {
            ValidateAppId();
        }

        void ValidateAppId()
        {
            var appId = UnityCloudPlayerSettings.Instance.AppId;

            var prefix = $"[{m_ServiceHostResolver?.GetResolvedEnvironment()}]";

            if (string.IsNullOrEmpty(appId))
            {
                m_AppMessaging.ShowWarning($"{prefix} Application Id was not set. (Unity Cloud App Registration)", true);
            }

            AppStateController.PrepareTransition(m_NextState).Apply();
        }
    }
}