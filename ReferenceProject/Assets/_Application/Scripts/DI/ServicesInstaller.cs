using System;
using System.ComponentModel;
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
        public override void InstallBindings()
        {
            // Analytics
            var assembly = Assembly.GetExecutingAssembly();
            Debug.Log($"Adding assembly to API Headers: {assembly.FullName}.");

            var playerSettings = UnityCloudPlayerSettings.Instance;
            var httpClient = new UnityHttpClient().WithApiSourceHeadersFromAssembly(assembly);

            var compositeAuthenticatorSettings = new CompositeAuthenticatorSettingsBuilder(httpClient, PlatformSupportFactory.GetAuthenticationPlatformSupport())
                .AddDefaultPersonalAccessTokenProvider()
                .AddDefaultPkceAuthenticator(playerSettings, playerSettings)
                .Build();

            var compositeAuthenticator = new CompositeAuthenticator(compositeAuthenticatorSettings);
            var cloudConfiguration = UnityRuntimeServiceHostConfigurationFactory.Create();
            var serviceHttpClient = new ServiceHttpClient(httpClient, compositeAuthenticator, playerSettings);

            var cloudWorkspaceRepository = new CloudWorkspaceRepository(serviceHttpClient, cloudConfiguration);
            
            Container.Bind(typeof(IAuthenticator), typeof(IUrlRedirectionAuthenticator), typeof(IAuthenticationStateProvider), typeof(IAccessTokenProvider))
                .FromInstance(compositeAuthenticator).AsSingle();
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

            var monitoringClient = new ServiceMessagingClient(WebSocketClientFactory.Create(), compositeAuthenticator, playerSettings);
            var joinerClient = new ServiceMessagingClient(WebSocketClientFactory.Create(), compositeAuthenticator, playerSettings);
            var presenceManager = new PresenceManager(monitoringClient, joinerClient, new Uri(cloudConfiguration.GetServiceAddress(ServiceProtocol.WebSocketSecure, "presence")));
            Container.Bind(typeof(IRoomProvider<Room>), typeof(ISessionProvider)).FromInstance(presenceManager).AsSingle();

            foreach (var logOutput in LogOutputs.Outputs)
            {
                logOutput.CurrentLevel = LogLevel.Warning;
            }
            
            var sceneWorkspaceProvider = new SceneWorkspaceProvider(cloudWorkspaceRepository);
            Container.Bind<SceneWorkspaceProvider>().FromInstance(sceneWorkspaceProvider).AsSingle();
        }
    }
}
