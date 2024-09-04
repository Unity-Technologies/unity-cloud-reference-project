using System;
using System.Threading.Tasks;
using Unity.Cloud.AppLinking;
using Unity.Cloud.Assets;
using Unity.ReferenceProject.AssetManager;
using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.DeepLinking;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public sealed class AssetListState : AppState
    {
        [SerializeField]
        AppState m_SceneSelectedState;
        
        IAssetListController m_AssetListController;
        IDeepLinkingController m_DeepLinkingController;
        IUrlRedirectionInterceptor m_UrlRedirectionInterceptor;

        Uri m_ForwardedDeepLink;
        PropertyValue<IAsset> m_IAsset;

        [Inject]
        void Setup(IAssetListController assetListController, IDeepLinkingController deepLinkingController, IUrlRedirectionInterceptor urlRedirectionInterceptor, AssetManagerStore assetManagerStore)
        {
            m_AssetListController = assetListController;
            m_DeepLinkingController = deepLinkingController;
            m_UrlRedirectionInterceptor = urlRedirectionInterceptor;

            m_IAsset = assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset));
        }

        void Awake()
        {
            m_AssetListController.AssetSelected += OnAssetSelected;
            m_UrlRedirectionInterceptor.DeepLinkForwarded += OnDeepLinkForwarded;
        }

        void OnDestroy()
        {
            m_AssetListController.AssetSelected -= OnAssetSelected;
            m_UrlRedirectionInterceptor.DeepLinkForwarded -= OnDeepLinkForwarded;
        }

        protected override async void EnterStateInternal()
        {
            if (!await TryConsumeForwardedDeepLink())
            {
                await m_AssetListController.Refresh();
            }
        }

        async Task<bool> TryConsumeForwardedDeepLink()
        {
            if (m_ForwardedDeepLink == null)
                return false;
            
            var deepLinkConsumptionSucceeded = await m_DeepLinkingController.TryConsumeUri(m_ForwardedDeepLink.AbsoluteUri);
            m_ForwardedDeepLink = null;
            
            return deepLinkConsumptionSucceeded;
        }

        void OnAssetSelected(IAsset asset)
        {
            AppStateController.PrepareTransition(m_SceneSelectedState).OnBeforeEnter(() => m_IAsset.SetValue(asset)).Apply();
        }
        
        void OnDeepLinkForwarded(Uri uri)
        {
            m_ForwardedDeepLink = uri;
        }
    }
}
