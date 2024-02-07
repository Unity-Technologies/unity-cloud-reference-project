using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Permissions
{
    public class PermissionsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IPermissionsController>().To<PermissionsController>().AsSingle();
        }
    }
}
