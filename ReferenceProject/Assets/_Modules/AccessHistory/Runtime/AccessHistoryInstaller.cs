using System;
using Zenject;

namespace Unity.ReferenceProject.AccessHistory
{
    public class AccessHistoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAccessHistoryController>().To<AccessHistoryController>().AsSingle();
        }
    }
}
