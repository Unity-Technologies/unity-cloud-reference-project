using System;
using System.Reflection;
using System.Text;
using Unity.Cloud.AppLinking;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using Unity.Cloud.DeepLinking;
using Unity.Cloud.AppLinking.Runtime;
using Unity.Cloud.Identity;
using Unity.Cloud.Identity.Runtime;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Identity;
using Unity.Services.Core;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class ServicesInstaller : MonoInstaller
    {
        [SerializeField]
        ServicesInitializer m_ServicesInitializer;
        
        [SerializeField]
        LogLevel m_ServicesLogLevel = LogLevel.Warning;
        
        CompositeAuthenticator m_CompositeAuthenticator;
        ServiceMessagingClient m_JoinerClient;
        ServiceMessagingClient m_MonitoringClient;

        public override void InstallBindings()
        {
            // Unity Services Initialization
            Container.Bind<InitializationOptions>().To<InitializationOptions>().AsSingle();
            Container.Bind<ServicesInitializer>().FromInstance(m_ServicesInitializer).AsSingle();
            
            foreach (var logOutput in LogOutputs.Outputs)
            {
                logOutput.CurrentLevel = m_ServicesLogLevel;
            }

            // Analytics
            var assembly = Assembly.GetExecutingAssembly();

            var playerSettings = UnityCloudPlayerSettings.Instance;
            var httpClient = new UnityHttpClient();
            var serviceHostResolver = UnityRuntimeServiceHostResolverFactory.Create();

            // TODO: update to use UnityServicesServiceHostResolver when identity is deployed to UCF
            var compositeAuthenticatorSettings = new CompositeAuthenticatorSettingsBuilder(httpClient, PlatformSupportFactory.GetAuthenticationPlatformSupport(), serviceHostResolver, playerSettings)
                .AddDefaultBrowserAuthenticatedAccessTokenProvider(playerSettings)
                .AddDefaultPkceAuthenticator(playerSettings)
                .Build();

            m_CompositeAuthenticator = new CompositeAuthenticator(compositeAuthenticatorSettings);

            ApiSourceVersion apiVersionOriginal = ApiSourceVersion.GetApiSourceVersionForAssembly(assembly);
            var serviceHttpClient = new ServiceHttpClient(httpClient, m_CompositeAuthenticator, playerSettings).WithApiSourceHeaders(apiVersionOriginal.Name, apiVersionOriginal.Version + GetFormattedPlatformStr());
            var organizationRepository = new AuthenticatorOrganizationRepository(serviceHttpClient, serviceHostResolver);
            var assetRepository = AssetRepositoryFactory.Create(serviceHttpClient, serviceHostResolver);

            Container.Bind<ICloudSession>().FromInstance(new CloudSession(m_CompositeAuthenticator));

            Container.Bind<IServiceHostResolver>().FromInstance(serviceHostResolver).AsSingle();
            Container.Bind<IAppIdProvider>().FromInstance(playerSettings).AsSingle();
            Container.Bind<IServiceHttpClient>().FromInstance(serviceHttpClient).AsSingle();
            Container.Bind<IAuthenticatedUserInfoProvider>().FromInstance(m_CompositeAuthenticator).AsSingle();
            Container.Bind<IServiceAuthorizer>().FromInstance(m_CompositeAuthenticator).AsSingle();
            Container.Bind<IOrganizationRepository>().FromInstance(organizationRepository).AsSingle();
            Container.Bind<IAssetRepository>().FromInstance(assetRepository).AsSingle();

            var queryArguments = new QueryArgumentsProcessor();
            var deepLinkProvider = new DeepLinkProvider(serviceHttpClient,
                queryArguments,
                serviceHostResolver,
                new UnityRuntimeUrlProcessor(),
                playerSettings
            );
            Container.Bind<IQueryArgumentsProcessor>().FromInstance(queryArguments).AsSingle();
            Container.Bind<IDeepLinkProvider>().FromInstance(deepLinkProvider).AsSingle();
            Container.Bind<IUrlRedirectionInterceptor>().FromInstance(UrlRedirectionInterceptor.GetInstance()).AsSingle();
            var clipboard = ClipboardFactory.Create();
            Container.Bind<IClipboard>().FromInstance(clipboard).AsSingle();

            var webSocketClientFactory = new WebSocketClientFactory();
            
            m_MonitoringClient = new ServiceMessagingClient(webSocketClientFactory.Create(), m_CompositeAuthenticator, playerSettings);
            m_JoinerClient = new ServiceMessagingClient(webSocketClientFactory.Create(), m_CompositeAuthenticator, playerSettings);
            var presenceManager = new PresenceManager(m_CompositeAuthenticator, serviceHttpClient, serviceHostResolver, playerSettings);
            Container.Bind(typeof(IPresenceManager), typeof(IRoomProvider<Room>), typeof(ISessionProvider)).FromInstance(presenceManager).AsSingle();
            Container.Bind<IPresentationService>().FromInstance(presenceManager.PresentationService).AsSingle();
#if USE_VIVOX
            var presenceVivoxService = new PresenceVivoxService(serviceHttpClient, presenceManager, serviceHostResolver);
            Container.Bind(typeof(IPresenceVivoxService), typeof(IPresenceVivoxServiceComponents)).FromInstance(presenceVivoxService).AsSingle();
#endif
        }

        static string GetFormattedPlatformStr()
        {
            /* Event Format is as follow :
                
            {project name}@{version}_|_{target platform}_|_{editor platform}

            project name : set by ApiSourceVersion.Name
            version : set by ApiSourceVersion.Version
            target platform : The Unity target build platform
            editor platform : The platfrom on which devs are using the editor.
                              In a case of build the value will always be "--".

            The -- ensures that they was no issue with request and make it easier to filter events.
             */

            StringBuilder str = new StringBuilder();
            str.Append("_|_");
            str.Append(UnityEngine.Application.platform);
            str.Append("_|_");
#if UNITY_EDITOR
            str.Append(Environment.OSVersion);
#else
            str.Append("--");
#endif

            return str.ToString();
        }

        void OnDestroy()
        {
            m_CompositeAuthenticator.Dispose();
            m_MonitoringClient.Dispose();
            m_JoinerClient.Dispose();
        }
    }
}
