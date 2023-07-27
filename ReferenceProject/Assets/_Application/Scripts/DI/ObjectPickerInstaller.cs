using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class ObjectPickerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IObjectPicker>().To<DataStreamPicker>().AsSingle();
        }
    }
}
