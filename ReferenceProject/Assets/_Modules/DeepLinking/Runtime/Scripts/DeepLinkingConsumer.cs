using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.AssetManager;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Unity.ReferenceProject.DeepLinking
{
    public class DeepLinkData
    {
        public bool SetDeepLinkCamera { get; set; }
        public Action SetCameraReady { get; set; }
        public bool DeepLinkIsProcessing { get; set; }
        public bool EnableSwitchToDesktop { get; set; }
    }

    public class DeepLinkingConsumer : MonoBehaviour
    {
        [SerializeField]
        AppState m_NextState;

        [Header("Localization")]
        [SerializeField]
        string m_OpenLinkSuccessString = "@DeepLinking:OpenLinkSuccess";

        IAppStateController m_AppStateController;
        PropertyValue<IAsset> m_SelectedAsset;
        IAppMessaging m_AppMessaging;
        IDeepLinkingController m_DeepLinkingController;

        IOrganizationRepository m_OrganizationRepository;
        IAssetRepository m_AssetRepository;
        IAssetListController m_AssetListController;

        [Inject]
        public void Setup(IDeepLinkingController deepLinkingController, IAppStateController appStateController,
            IOrganizationRepository organizationRepository, IAssetRepository assetRepository, IAssetListController assetListController,
            AssetManagerStore assetManagerStore, IAppMessaging appMessaging)
        {
            m_DeepLinkingController = deepLinkingController;
            m_AppStateController = appStateController;
            m_OrganizationRepository = organizationRepository;
            m_AssetRepository = assetRepository;
            m_AssetListController = assetListController;
            m_SelectedAsset = assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset));
            m_AppMessaging = appMessaging;
        }

        public void Awake()
        {
            m_DeepLinkingController.DeepLinkConsumed += OnDeepLinkConsumed;
        }

        public void OnDestroy()
        {
            m_DeepLinkingController.DeepLinkConsumed -= OnDeepLinkConsumed;
        }

        async void OnDeepLinkConsumed(AssetDescriptor assetDescriptor, bool haSceneState)
        {
            await ProcessDescriptor(assetDescriptor, haSceneState);
        }
        
        async Task ProcessDescriptor(AssetDescriptor assetDescriptor, bool hasSceneState)
        {
            var selectedAsset = m_SelectedAsset.GetValue();
            
            var isOpeningNewAsset = selectedAsset == null || !selectedAsset.Descriptor.AssetId.Equals(assetDescriptor.AssetId);
            var asset = await GetAssetAsync(assetDescriptor);

            if (isOpeningNewAsset)
            {
                // Switch organization if new asset does not belong to previous selected asset organization
                // This is required to refresh the users permissions
                if (selectedAsset == null || !selectedAsset.Descriptor.OrganizationId.Equals(assetDescriptor.OrganizationId))
                {
                    var organization = await m_OrganizationRepository.GetOrganizationAsync(assetDescriptor.OrganizationId);
                    await m_AssetListController.SelectOrganization(organization);
                }

                m_AppMessaging.ShowMessage(m_OpenLinkSuccessString, false, asset.Name);

                m_AppStateController.PrepareTransition(m_NextState).OnBeforeEnter(() => m_SelectedAsset.SetValue(asset)).Apply();
            }
            else
            {
                if (hasSceneState)
                {
                    m_DeepLinkingController.ProcessQueryArguments();
                }
            }
        }

        async Task<IAsset> GetAssetAsync(AssetDescriptor assetDescriptor)
        {
            var asset = await m_AssetRepository.GetAssetAsync(assetDescriptor, default)
                ?? throw new InvalidArgumentException("Asset not found.");
            return asset;
        }
    }
}
