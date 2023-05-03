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
        event Action<IScene> DeepLinkConsumed;
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

        public DeepLinkingController(IQueryArgumentsProcessor argumentsProcessor, IUrlRedirectionInterceptor interceptor,
            IDeepLinkProvider linkProvider, ISceneProvider sceneProvider, IAuthenticationStateProvider authenticator)
        {
            m_QueryArgumentsProcessor = argumentsProcessor;
            m_UrlRedirectionInterceptor = interceptor;
            m_DeepLinkProvider = linkProvider;
            m_SceneProvider = sceneProvider;
            m_Authenticator = authenticator;

            m_Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            m_UrlRedirectionInterceptor.DeepLinkForwarded += OnDeepLinkForwarded;
        }

        public event Action<IScene> DeepLinkConsumed;

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
                return await TryConsumeUri(uri);
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
                DeepLinkConsumed?.Invoke(scene);
            }

            m_QueryArgumentsProcessor.Process(deepLinkInfo);
            return true;
        }

        async Task TryConsumeForwardedDeepLink()
        {
            if (m_ForwardedDeepLink == null)
                return;

            if (await TryConsumeUri(m_ForwardedDeepLink))
            {
                m_ForwardedDeepLink = null;
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
