using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class LocalizationInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAppLocalization>().To<AppLocalization>().AsSingle();
        }
    }
}
