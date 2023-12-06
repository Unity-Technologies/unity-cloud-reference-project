using Unity.Cloud.Presence.Runtime;
using Unity.Netcode;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class PresenceInstaller : MonoInstaller
    {
        [SerializeField]
        GameObject m_PresenceInstallerComponents;

        [SerializeField]
        NetworkManager m_NetworkManagerPrefab;

        NetworkManager m_NetworkManager;

        IFollowObject m_FollowObject;

        void OnDestroy()
        {
            if (m_NetworkManager != null)
            {
                m_NetworkManager.Shutdown();
            }

            if(m_FollowObject != null)
            {
                m_FollowObject.Dispose();
            }
        }

        public override void InstallBindings()
        {
            gameObject.AddComponent<UnityCloudClientTransport>();

            m_NetworkManager = Instantiate(m_NetworkManagerPrefab);
            Container.Bind<NetworkManager>().FromInstance(m_NetworkManager).AsSingle();

            var netcodeParticipantManager = m_PresenceInstallerComponents.GetComponent<NetcodeParticipantManager>();
            Container.Bind<NetcodeParticipantManager>().FromInstance(netcodeParticipantManager).AsSingle();
            Container.Bind<INetcodeService>().To<NetcodeService>().AsSingle();

            var presenceRoomsManager = m_PresenceInstallerComponents.GetComponent<IPresenceRoomsManager>();
            Container.Bind<IPresenceRoomsManager>().FromInstance(presenceRoomsManager).AsSingle();

            var presenceStreamingRoom = m_PresenceInstallerComponents.GetComponent<IPresenceStreamingRoom>();
            Container.Bind<IPresenceStreamingRoom>().FromInstance(presenceStreamingRoom).AsSingle();

            var voiceManager = m_PresenceInstallerComponents.GetComponent<IVoiceManager>();
            Container.Bind<IVoiceManager>().FromInstance(voiceManager).AsSingle();

            var followManager = m_PresenceInstallerComponents.GetComponent<IFollowManager>();
            Container.Bind<IFollowManager>().FromInstance(followManager).AsSingle();

            FollowObject followObject = new FollowObject();
            followObject.Setup(followManager, netcodeParticipantManager);
            Container.Bind<IFollowObject>().FromInstance(followObject).AsSingle();
            m_FollowObject = followObject;
                
            var presentationManager = m_PresenceInstallerComponents.GetComponent<PresentationManager>();
            Container.Bind<PresentationManager>().FromInstance(presentationManager).AsSingle();
        }
    }
}
