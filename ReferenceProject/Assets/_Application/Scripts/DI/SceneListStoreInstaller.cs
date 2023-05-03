using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.ScenesList;
using Unity.ReferenceProject.DataStores;
using Zenject;

namespace Unity.ReferenceProject
{
    public class SceneListStoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var sceneListStore = gameObject.AddComponent<SceneListStore>();

            Container.Bind<PropertyValue<IScene>>()
                .FromInstance(sceneListStore.GetProperty<IScene>(nameof(ScenesListViewModel.SelectedScene)));
        }
    }
}
