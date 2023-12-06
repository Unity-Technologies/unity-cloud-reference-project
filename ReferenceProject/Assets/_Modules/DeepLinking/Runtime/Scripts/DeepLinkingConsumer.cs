using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
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

        [FormerlySerializedAs("m_SceneAlreadyOpenedString")]
        [SerializeField]
        string m_AssetAlreadyOpenedString = "@DeepLinking:SceneAlreadyOpen";

        IAppStateController m_AppStateController;
        PropertyValue<IAsset> m_SelectedAsset;
        IAppMessaging m_AppMessaging;
        IDeepLinkingController m_DeepLinkingController;
        
        IOrganizationRepository m_OrganizationRepository;
        IAssetRepository m_AssetRepository;

        [Inject]
        public void Setup(IDeepLinkingController deepLinkingController, IAppStateController appStateController,
            IOrganizationRepository organizationRepository, IAssetRepository assetRepository,
            AssetManagerStore assetManagerStore, IAppMessaging appMessaging)
        {
            m_DeepLinkingController = deepLinkingController;
            m_AppStateController = appStateController;
            m_OrganizationRepository = organizationRepository;
            m_AssetRepository = assetRepository;
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

        void OnDeepLinkConsumed(DatasetDescriptor datasetDescriptor, bool hasNewSceneState)
        {
            _ = ProcessDescriptor(datasetDescriptor, hasNewSceneState);
        }
        
        async Task ProcessDescriptor(DatasetDescriptor datasetDescriptor, bool hasNewSceneState)
        {
            var selectedAsset = m_SelectedAsset.GetValue();

            var isOpeningNewScene = selectedAsset.Descriptor != datasetDescriptor.AssetDescriptor;

            if (isOpeningNewScene || hasNewSceneState)
            {
                var asset = await GetAssetAsync(datasetDescriptor.AssetDescriptor);

                m_AppMessaging.ShowMessage(m_OpenLinkSuccessString, false, asset.Name);
                
                m_AppStateController.PrepareTransition(m_NextState).OnBeforeEnter(() => m_SelectedAsset.SetValue(asset)).Apply();
            }
            else
            {
                m_AppMessaging.ShowWarning(m_AssetAlreadyOpenedString, true, selectedAsset.Name);
            }
        }
        
        async Task<IOrganization> GetOrganizationAsync(OrganizationId orgId)
        {
            var organizations = await m_OrganizationRepository.ListOrganizationsAsync();
            return organizations.FirstOrDefault(o => o.Id == orgId)
                ?? throw new InvalidArgumentException("Organization not found.");
        }

        async Task<IAssetProject> GetProjectAsync(ProjectDescriptor projectDescriptor)
        {
            var project = await m_AssetRepository.GetAssetProjectAsync(projectDescriptor, default)
                ?? throw new InvalidArgumentException("Project not found.");
            return project;
        }

        async Task<IAsset> GetAssetAsync(AssetDescriptor assetDescriptor)
        {
            var asset = await m_AssetRepository.GetAssetAsync(assetDescriptor, new FieldsFilter(), default)
                ?? throw new InvalidArgumentException("Asset not found.");
            return asset;
        }

        async Task<IDataset> GetDatasetAsync(DatasetDescriptor datasetDescriptor)
        {
            var dataset = await m_AssetRepository.GetDatasetAsync(datasetDescriptor, default, CancellationToken.None)
                ?? throw new InvalidArgumentException("Dataset not found.");
            return dataset;
        }
    }
}
