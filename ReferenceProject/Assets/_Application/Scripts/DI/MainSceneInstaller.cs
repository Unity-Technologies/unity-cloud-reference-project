using Unity.ReferenceProject.StateMachine;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class MainSceneInstaller : MonoInstaller
    {
        [SerializeField]
        AppStateMachine m_AppStateMachine;

        public override void InstallBindings()
        {
            Container.Bind<IAppStateController>().FromInstance(m_AppStateMachine.AppStateController).AsSingle();
            Container.Bind<IAppLocalization>().To<AppLocalization>().AsSingle();
        }
    }
}