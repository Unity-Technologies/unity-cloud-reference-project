using System;
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
        PresenceStreamingRoom m_PresenceStreamingRoom;
        
        [SerializeField]
        PresenceRoomsManager m_PresenceRoomsManager;

        [SerializeField]
        NetworkManager m_NetworkManager;
        
        [SerializeField]
        NetcodeParticipantManager m_NetcodeParticipantManager;
        
        [SerializeField]
        VoiceManager m_VoiceManager;

        void OnDestroy()
        {
            if (m_NetworkManager != null)
            {
                m_NetworkManager.Shutdown();
            }
        }

        public override void InstallBindings()
        {
            Container.Bind<PresenceStreamingRoom>().FromInstance(m_PresenceStreamingRoom).AsSingle();
            Container.Bind<PresenceRoomsManager>().FromInstance(m_PresenceRoomsManager).AsSingle();
            Container.Bind<NetworkManager>().FromInstance(m_NetworkManager).AsSingle();
            Container.Bind<NetcodeParticipantManager>().FromInstance(m_NetcodeParticipantManager).AsSingle();
            Container.Bind<INetcodeService>().To<NetcodeService>().AsSingle();
            Container.Bind<VoiceManager>().FromInstance(m_VoiceManager).AsSingle();
        }
    }
}
