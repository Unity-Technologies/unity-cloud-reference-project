using System;
using Zenject;

namespace Unity.ReferenceProject.AccessHistory
{
    public class AccessHistoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var accessHistory = new AccessHistoryController();
            Container.Bind<IAccessHistoryController>().FromInstance(accessHistory);
        }
    }
}
