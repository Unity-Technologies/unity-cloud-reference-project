using System;
using Unity.Cloud.Assets;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.AssetManager;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class AssetListToolUIControllerVR : AssetListToolUIController
    {
        static readonly string k_NoMarginUssClassName = "container__asset-list-no-margin";

        IDataStreamController m_DataStreamController;
        PropertyValue<IAsset> m_Asset;
        IAssetListController m_AssetListController;

        [Inject]
        void Setup(AssetManagerStore assetManagerStore, IDataStreamController dataStreamController, IAssetListController assetController)
        {
            m_Asset = assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset));
            m_DataStreamController = dataStreamController;
            m_AssetListController = assetController;
        }

        public override void OnToolOpened()
        {
            base.OnToolOpened();

            _ = m_AssetListController.Refresh();
            m_AssetListController.AssetSelected += OnAssetSelected;
        }

        public override void OnToolClosed()
        {
            base.OnToolClosed();
            m_AssetListController.Close();
            m_AssetListController.AssetSelected -= OnAssetSelected;
        }

        void OnAssetSelected(IAsset asset)
        {
            CloseSelf();

            if (asset.Descriptor.AssetId != m_Asset.GetValue().Descriptor.AssetId)
            {
                m_Asset.SetValue(m_AssetListController.SelectedAsset);
                m_DataStreamController.Load(m_AssetListController.SelectedAsset);
            }
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var visualElement = base.CreateVisualTree(template);
            m_AssetListUIController.InitUIToolkit(visualElement);

            var container = visualElement.Q("MainContainer");
            container.AddToClassList(k_NoMarginUssClassName);

            return visualElement;
        }
    }
}
