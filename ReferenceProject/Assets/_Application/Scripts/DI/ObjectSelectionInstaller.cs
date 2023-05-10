using System;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.DataStores;
using Zenject;

namespace Unity.ReferenceProject
{
    public class ObjectSelectionInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var objectSelectionStore = gameObject.AddComponent<ObjectSelectionStore>();

            var property = objectSelectionStore.GetProperty<IObjectSelectionInfo>(nameof(ObjectSelectionViewModel.SelectionInfo));

            property.SetValue(new ObjectSelectionInfo());

            Container.Bind<PropertyValue<IObjectSelectionInfo>>()
                .FromInstance(property);

            Container.Bind<ObjectSelectionActivator>().AsSingle();
        }
    }
}
