using System;
using Unity.Cloud.Assets;
using Zenject;

namespace Unity.ReferenceProject.AssetManager
{
    public class AssetManagerStoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var assetManagerStore = gameObject.AddComponent<AssetManagerStore>();

            assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset))
                .SetValue((IAsset)null);

            Container.Bind<AssetManagerStore>().FromInstance(assetManagerStore).AsSingle();
        }
    }
}

