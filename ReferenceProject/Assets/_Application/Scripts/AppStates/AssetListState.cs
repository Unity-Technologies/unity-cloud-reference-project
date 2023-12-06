using Unity.Cloud.Assets;
using Unity.ReferenceProject.AssetManager;
using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.DataStores;
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

        PropertyValue<IAsset> m_IAsset;

        [Inject]
        void Setup(IAssetListController assetListController, AssetManagerStore assetManagerStore)
        {
            m_AssetListController = assetListController;
            m_IAsset = assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset));
        }

        void Awake()
        {
            m_AssetListController.AssetSelected += OnAssetSelected;
        }

        void OnDestroy()
        {
            m_AssetListController.AssetSelected -= OnAssetSelected;
        }

        protected override void EnterStateInternal()
        {
            _ = m_AssetListController.Refresh();
        }

        void OnAssetSelected(IAsset asset)
        {
            AppStateController.PrepareTransition(m_SceneSelectedState).OnBeforeEnter(() => m_IAsset.SetValue(asset)).Apply();
        }
    }
}
