using System;
using Unity.ReferenceProject.Settings;
using Zenject;

namespace Unity.ReferenceProject
{
    public class GlobalSettingsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGlobalSettings>().To<GlobalSettings>().AsSingle();
        }
    }
}
