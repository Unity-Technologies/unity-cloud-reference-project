using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.StateMachine
{
    public class AppStateMachineInstaller : MonoInstaller
    {
        [SerializeField]
        AppStateMachine m_AppStateMachine;

        public override void InstallBindings()
        {
            Container.Bind<IAppStateController>().FromInstance(m_AppStateMachine.AppStateController).AsSingle();
        }
    }
}
