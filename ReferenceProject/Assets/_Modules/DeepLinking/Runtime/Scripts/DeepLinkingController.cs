using System;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.DeepLinking;
using Unity.Cloud.Identity;
using Unity.Cloud.Storage;
using UnityEngine;

namespace Unity.ReferenceProject.DeepLinking
{
    public interface IDeepLinkingController : IDisposable
    {
        event Action<IScene, bool> DeepLinkConsumed;
        event Action<Exception> LinkConsumptionFailed;
        Task<Uri> GenerateUri(IScene scene);
        Task<bool> TryConsumeUri(string url);
    }

    public sealed class DeepLinkingController : IDeepLinkingController
    {
        readonly IAuthenticationStateProvider m_Authenticator;
        readonly IDeepLinkProvider m_DeepLinkProvider;

        readonly IQueryArgumentsProcessor m_QueryArgumentsProcessor;
        readonly ISceneProvider m_SceneProvider;
        readonly IUrlRedirectionInterceptor m_UrlRedirectionInterceptor;

        Uri m_ForwardedDeepLink;

        DeepLinkCameraInfo m_SetDeepLinkCamera;
        DeepLinkInfo m_CurrentDeepLinkInfo;

        public DeepLinkingController(IQueryArgumentsProcessor argumentsProcessor, IUrlRedirectionInterceptor interceptor,
            IDeepLinkProvider linkProvider, ISceneProvider sceneProvider, IAuthenticationStateProvider authenticator, DeepLinkCameraInfo deepLinkCameraInfo)
        {
            m_QueryArgumentsProcessor = argumentsProcessor;
            m_UrlRedirectionInterceptor = interceptor;
            m_DeepLinkProvider = linkProvider;
            m_SceneProvider = sceneProvider;
            m_Authenticator = authenticator;

            m_Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            m_UrlRedirectionInterceptor.DeepLinkForwarded += OnDeepLinkForwarded;

            m_SetDeepLinkCamera = deepLinkCameraInfo;
            m_SetDeepLinkCamera.SetCameraReady += OnSetCameraReady;
        }

        public event Action<IScene, bool> DeepLinkConsumed;
        public event Action<Exception> LinkConsumptionFailed;

        public void Dispose()
        {
            m_Authenticator.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            m_UrlRedirectionInterceptor.DeepLinkForwarded -= OnDeepLinkForwarded;
        }

        public async Task<Uri> GenerateUri(IScene scene)
        {
            return await m_DeepLinkProvider.CreateDeepLinkAsync(scene);
        }

        public async Task<bool> TryConsumeUri(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                try
                {
                    if (await TryConsumeUri(uri))
                    {
                        return true;   
                    }
                }
                catch (Exception e)
                {
                    LinkConsumptionFailed?.Invoke(e);
                }
            }
            return false;
        }

        async Task<bool> TryConsumeUri(Uri uri)
        {
            if (uri == null)
                return false;

            var deepLinkInfo = await m_DeepLinkProvider.GetDeepLinkInfoAsync(uri);
            if (deepLinkInfo is { ResourceType: DeepLinkResourceType.Scene })
            {
                var scene = await m_SceneProvider.GetSceneAsync(new SceneId(deepLinkInfo.ResourceId));
                m_SetDeepLinkCamera.SetDeepLinkCamera = true;
                // check if Query Arguments hold a different scene state. For now, just a null check instead of a full comparison of states
                var hasNewSceneState = !string.IsNullOrEmpty(deepLinkInfo.QueryArguments);
                DeepLinkConsumed?.Invoke(scene, hasNewSceneState);
            }
            
            m_CurrentDeepLinkInfo = deepLinkInfo;
            return true;
        }
        
        void OnSetCameraReady()
        {
            if (m_CurrentDeepLinkInfo != null && !string.IsNullOrEmpty(m_CurrentDeepLinkInfo.QueryArguments))
            {
                m_QueryArgumentsProcessor.Process(m_CurrentDeepLinkInfo);
            }
        }

        async Task TryConsumeForwardedDeepLink()
        {
            if (m_ForwardedDeepLink == null)
            {
                return;
            }
            
            try
            {
                if (await TryConsumeUri(m_ForwardedDeepLink))
                {
                    m_ForwardedDeepLink = null;
                }
            }
            catch (Exception e)
            {
                LinkConsumptionFailed?.Invoke(e);
            }
        }

        async void OnDeepLinkForwarded(Uri uri)
        {
            Debug.Log($"A link was received '{uri}'.");
            m_ForwardedDeepLink = uri;
            await TryConsumeForwardedDeepLink();
        }

        async void OnAuthenticationStateChanged(AuthenticationState state)
        {
            if (state == AuthenticationState.LoggedIn)
            {
                await TryConsumeForwardedDeepLink();
            }
        }
    }
}
