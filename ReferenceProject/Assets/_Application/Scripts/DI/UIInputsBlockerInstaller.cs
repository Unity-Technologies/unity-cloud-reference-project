using System;
using Unity.ReferenceProject.UIInputBlocker;
using Zenject;

namespace Unity.ReferenceProject
{
    public class UIInputsBlockerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var dispatcher = gameObject.AddComponent<UIInputBlockerEventsDispatcher>();
            Container.Bind<IUIInputBlockerEventsDispatcher>().FromInstance(dispatcher).AsSingle();
            Container.Bind<IUIInputBlockerEvents>().FromInstance(dispatcher).AsSingle();
        }
    }
}
