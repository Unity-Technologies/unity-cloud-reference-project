using System;
using Unity.ReferenceProject.Navigation;
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
        }
    }
}
