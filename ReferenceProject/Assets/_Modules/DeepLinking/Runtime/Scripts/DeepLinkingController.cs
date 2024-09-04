using System;
using System.Threading.Tasks;
using Unity.Cloud.AppLinking;
using Unity.Cloud.Common;
using Unity.Cloud.DeepLinking;
using Unity.ReferenceProject.Identity;
using UnityEngine;

namespace Unity.ReferenceProject.DeepLinking
{
    public interface IDeepLinkingController : IDisposable
    {
        event Action<AssetDescriptor, bool> DeepLinkConsumed;
        event Action<Exception> LinkConsumptionFailed;
        Task<Uri> GenerateUri(AssetDescriptor assetDescriptor);
        Task<bool> TryConsumeUri(string url);
        void ProcessQueryArguments();
    }

    public sealed class DeepLinkingController : IDeepLinkingController
    {
        readonly ICloudSession m_Session;
        readonly IDeepLinkProvider m_DeepLinkProvider;

        readonly IQueryArgumentsProcessor m_QueryArgumentsProcessor;
        readonly IUrlRedirectionInterceptor m_UrlRedirectionInterceptor;

        Uri m_ForwardedDeepLink;

        readonly DeepLinkData m_DeepLinkData;
        DeepLinkInfo m_CurrentDeepLinkInfo;

        public DeepLinkingController(IQueryArgumentsProcessor argumentsProcessor, IUrlRedirectionInterceptor interceptor,
            IDeepLinkProvider linkProvider, ICloudSession session, DeepLinkData deepLinkData)
        {
            m_QueryArgumentsProcessor = argumentsProcessor;
            m_UrlRedirectionInterceptor = interceptor;
            m_DeepLinkProvider = linkProvider;
            m_Session = session;

            m_UrlRedirectionInterceptor.DeepLinkForwarded += OnDeepLinkForwarded;

            m_DeepLinkData = deepLinkData;
            m_DeepLinkData.SetCameraReady += OnSetCameraReady;
        }

        public event Action<AssetDescriptor, bool> DeepLinkConsumed;
        public event Action<Exception> LinkConsumptionFailed;

        public void Dispose()
        {
            m_UrlRedirectionInterceptor.DeepLinkForwarded -= OnDeepLinkForwarded;
        }

        public async Task<Uri> GenerateUri(AssetDescriptor assetDescriptor)
        {
            return await m_DeepLinkProvider.CreateDeepLinkAsync(assetDescriptor);
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

        public void ProcessQueryArguments()
        {
            if (m_CurrentDeepLinkInfo != null)
            {
                if (!string.IsNullOrEmpty(m_CurrentDeepLinkInfo.QueryArguments))
                {
                    m_QueryArgumentsProcessor.Process(m_CurrentDeepLinkInfo);                    
                }
                m_CurrentDeepLinkInfo = null;
                m_DeepLinkData.DeepLinkIsProcessing = false;
            }
        }

        async Task<bool> TryConsumeUri(Uri uri)
        {
            if (uri == null)
                return false;

            var deepLinkInfo = await m_DeepLinkProvider.GetDeepLinkInfoAsync(uri);
            if (deepLinkInfo is { ResourceType: DeepLinkResourceType.Asset })
            {
                m_DeepLinkData.DeepLinkIsProcessing = true;
                m_DeepLinkData.SetDeepLinkCamera = true;

                var hasNewSceneState = !string.IsNullOrEmpty(deepLinkInfo.QueryArguments);
                var assetDescriptor = deepLinkInfo.ResourceId.ToAssetDescriptor();
                
                DeepLinkConsumed?.Invoke(assetDescriptor, hasNewSceneState);
                m_CurrentDeepLinkInfo = deepLinkInfo;
                return true;
            }

            return false;
        }

        void OnSetCameraReady()
        {
            ProcessQueryArguments();
        }

        async Task TryConsumeForwardedDeepLink()
        {
            if (m_ForwardedDeepLink == null)
            {
                return;
            }

            try
            {
                await TryConsumeUri(m_ForwardedDeepLink);
            }
            catch (Exception e)
            {
                LinkConsumptionFailed?.Invoke(e);
            }
            finally
            {
                m_ForwardedDeepLink = null;
            }
        }

        async void OnDeepLinkForwarded(Uri uri)
        {
            Debug.Log($"A link was received '{uri}'.");
            m_ForwardedDeepLink = uri;
            if (m_Session.State == SessionState.LoggedIn)
            {
                await TryConsumeForwardedDeepLink();                
            }
        }
    }
}
