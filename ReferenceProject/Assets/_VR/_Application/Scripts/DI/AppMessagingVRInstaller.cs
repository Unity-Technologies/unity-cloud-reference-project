using Unity.ReferenceProject.Messaging;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class AppMessagingVRInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAppMessaging>().To<AppMessagingVR>().AsSingle();
        }
    }
}
