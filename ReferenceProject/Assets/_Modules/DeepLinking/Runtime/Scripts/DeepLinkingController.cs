using System;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.DeepLinking;
using Unity.ReferenceProject.Identity;
using UnityEngine;

namespace Unity.ReferenceProject.DeepLinking
{
    public interface IDeepLinkingController : IDisposable
    {
        event Action<DatasetDescriptor, bool> DeepLinkConsumed;
        event Action<Exception> LinkConsumptionFailed;
        Task<Uri> GenerateUri(AssetDescriptor assetDescriptor);
        Task<bool> TryConsumeUri(string url);
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

            m_Session.SessionStateChanged += OnAuthenticationStateChanged;
            m_UrlRedirectionInterceptor.DeepLinkForwarded += OnDeepLinkForwarded;

            m_DeepLinkData = deepLinkData;
            m_DeepLinkData.SetCameraReady += OnSetCameraReady;
        }

        public event Action<DatasetDescriptor, bool> DeepLinkConsumed;
        public event Action<Exception> LinkConsumptionFailed;

        public void Dispose()
        {
            m_Session.SessionStateChanged -= OnAuthenticationStateChanged;
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

        async Task<bool> TryConsumeUri(Uri uri)
        {
            if (uri == null)
                return false;

            m_DeepLinkData.DeepLinkIsProcessing = true;
            var deepLinkInfo = await m_DeepLinkProvider.GetDeepLinkInfoAsync(uri);
            if (deepLinkInfo is { ResourceType: DeepLinkResourceType.Dataset })
            {
                m_DeepLinkData.SetDeepLinkCamera = true;

                // Move to utils
                var splitId = deepLinkInfo.ResourceId.ToString().Split(',');

                var organizationId = new OrganizationId(splitId[0]);
                var projectId = new ProjectId(splitId[1]);
                var assetId = new AssetId(splitId[2]);
                var datasetId = new DatasetId(splitId[3]);

                var hasNewSceneState = !string.IsNullOrEmpty(deepLinkInfo.QueryArguments);
                if (hasNewSceneState)
                {
                    m_QueryArgumentsProcessor.Process(deepLinkInfo);
                }

                var datasetDescriptor = new DatasetDescriptor(new AssetDescriptor(new ProjectDescriptor(organizationId, projectId), assetId, new AssetVersion(0)), datasetId);

                DeepLinkConsumed?.Invoke(datasetDescriptor, hasNewSceneState);
                m_CurrentDeepLinkInfo = deepLinkInfo;

                return true;
            }

            return false;
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

        async void OnAuthenticationStateChanged(SessionState state)
        {
            if (state == SessionState.LoggedIn)
            {
                await TryConsumeForwardedDeepLink();
            }
        }
    }
}
