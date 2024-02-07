using System;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.MeasureTool
{
    public class AppUnitBindingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAppUnit>().To<AppUnit>().AsSingle();
        }
    }
}
