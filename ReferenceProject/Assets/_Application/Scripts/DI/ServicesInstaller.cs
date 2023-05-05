using System;
using System.Reflection;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using Unity.Cloud.DeepLinking;
using Unity.Cloud.DeepLinking.Runtime;
using Unity.Cloud.Identity;
using Unity.Cloud.Identity.Runtime;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.Cloud.Storage;
using Unity.ReferenceProject.ScenesList;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class ServicesInstaller : MonoInstaller
    {
        [SerializeField]
        LogLevel m_ServicesLogLevel = LogLevel.Warning;
        
        CompositeAuthenticator m_CompositeAuthenticator;
        ServiceMessagingClient m_JoinerClient;
        ServiceMessagingClient m_MonitoringClient;

        public override void InstallBindings()
        {
            foreach (var logOutput in LogOutputs.Outputs)
            {
                logOutput.CurrentLevel = m_ServicesLogLevel;
            }
            
            // Analytics
            var assembly = Assembly.GetExecutingAssembly();
            Debug.Log($"Adding assembly to API Headers: {assembly.FullName}.");

            var playerSettings = UnityCloudPlayerSettings.Instance;
            var httpClient = new UnityHttpClient().WithApiSourceHeadersFromAssembly(assembly);

            var compositeAuthenticatorSettings = new CompositeAuthenticatorSettingsBuilder(httpClient, PlatformSupportFactory.GetAuthenticationPlatformSupport())
                .AddDefaultPersonalAccessTokenProvider()
                .AddDefaultPkceAuthenticator(playerSettings, playerSettings)
                .Build();

            m_CompositeAuthenticator = new CompositeAuthenticator(compositeAuthenticatorSettings);
            var cloudConfiguration = UnityRuntimeServiceHostConfigurationFactory.Create();
            var serviceHttpClient = new ServiceHttpClient(httpClient, m_CompositeAuthenticator, playerSettings);

            var cloudWorkspaceRepository = new CloudWorkspaceRepository(serviceHttpClient, cloudConfiguration);
            
            Container.Bind(typeof(IAuthenticator), typeof(IUrlRedirectionAuthenticator), typeof(IAuthenticationStateProvider), typeof(IAccessTokenProvider))
                .FromInstance(m_CompositeAuthenticator).AsSingle();
            Container.Bind<ServiceHostConfiguration>().FromInstance(cloudConfiguration).AsSingle();
            Container.Bind<IAppIdProvider>().FromInstance(playerSettings).AsSingle();
            Container.Bind<IServiceHttpClient>().FromInstance(serviceHttpClient).AsSingle();
            Container.Bind<IUserInfoProvider>().To<UserInfoProvider>().AsSingle();
            Container.Bind<IWorkspaceRepository>().FromInstance(cloudWorkspaceRepository).AsSingle();
            
            var queryArguments = new QueryArgumentsProcessor();
            var deepLinkProvider = new DeepLinkProvider(serviceHttpClient, queryArguments, cloudConfiguration,
                new UriActivationPlatformSupport());
            Container.Bind<IQueryArgumentsProcessor>().FromInstance(queryArguments).AsSingle();
            Container.Bind<IDeepLinkProvider>().FromInstance(deepLinkProvider).AsSingle();
            Container.Bind<IUrlRedirectionInterceptor>().FromInstance(UrlRedirectionInterceptor.GetInstance()).AsSingle();
            Container.Bind<ISceneProvider>().To<SceneProvider>().AsSingle();
            var clipboard = ClipboardFactory.Create();
            Container.Bind<IClipboard>().FromInstance(clipboard).AsSingle();

            m_MonitoringClient = new ServiceMessagingClient(WebSocketClientFactory.Create(), m_CompositeAuthenticator, playerSettings);
            m_JoinerClient = new ServiceMessagingClient(WebSocketClientFactory.Create(), m_CompositeAuthenticator, playerSettings);
            var presenceManager = new PresenceManager(m_MonitoringClient, m_JoinerClient, new Uri(cloudConfiguration.GetServiceAddress(ServiceProtocol.WebSocketSecure, "presence")));
            Container.Bind(typeof(IRoomProvider<Room>), typeof(ISessionProvider)).FromInstance(presenceManager).AsSingle();

            var sceneWorkspaceProvider = new SceneWorkspaceProvider(cloudWorkspaceRepository);
            Container.Bind<SceneWorkspaceProvider>().FromInstance(sceneWorkspaceProvider).AsSingle();
        }
        
        void OnDestroy()
        {
            m_CompositeAuthenticator.Dispose();
            m_MonitoringClient.Dispose();
            m_JoinerClient.Dispose();
        }
    }
}
