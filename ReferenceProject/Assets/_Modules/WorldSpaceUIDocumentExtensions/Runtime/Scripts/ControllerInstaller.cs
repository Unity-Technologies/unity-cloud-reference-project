using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.WorldSpaceUIDocumentExtensions
{
    public class ControllerInstaller : MonoInstaller
    {
        [SerializeField]
        ControllerInfo m_CurrentControllerInfo;

        public override void InstallBindings()
        {
            var ControllerStore = gameObject.AddComponent<ControllerStore>();

            var property = ControllerStore.GetProperty<IControllerInfo>(nameof(ControllerViewModel.ControllerInfo));
            property.SetValue(m_CurrentControllerInfo);

            Container.Bind<PropertyValue<IControllerInfo>>()
                .FromInstance(property);
        }
    }
}
