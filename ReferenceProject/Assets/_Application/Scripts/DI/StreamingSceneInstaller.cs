using System;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class StreamingSceneInstaller : MonoInstaller
    {
        [SerializeField]
        NavigationManager m_NavigationManager;    

        public override void InstallBindings()
        {
            Container.Bind<INavigationManager>().FromInstance(m_NavigationManager).AsSingle();
            Container.Bind<IObjectPicker>().To<DataStreamPicker>().AsSingle();
        }
    }
}
