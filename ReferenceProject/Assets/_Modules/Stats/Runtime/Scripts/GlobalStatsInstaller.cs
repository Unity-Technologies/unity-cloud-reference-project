using System;
using Unity.ReferenceProject.Stats;
using Zenject;

namespace Unity.ReferenceProject
{
    public class GlobalStatsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGlobalStats>().To<GlobalStats>().AsSingle();
        }
    }
}
