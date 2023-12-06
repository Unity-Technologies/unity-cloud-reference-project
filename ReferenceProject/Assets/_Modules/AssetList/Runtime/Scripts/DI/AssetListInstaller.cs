using Zenject;

namespace Unity.ReferenceProject.AssetList.DI
{
    public class AssetListInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAssetListController>().To<AssetListController>().AsSingle();
        }
    }
}
