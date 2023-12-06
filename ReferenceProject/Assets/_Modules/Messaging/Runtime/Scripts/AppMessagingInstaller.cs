using Unity.ReferenceProject.Messaging;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class AppMessagingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAppMessaging>().To<AppMessaging>().AsSingle();
        }
    }
}
