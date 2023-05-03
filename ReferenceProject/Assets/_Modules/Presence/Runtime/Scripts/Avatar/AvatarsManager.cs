using System;
using System.Collections.Generic;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarsManager : MonoBehaviour
    {
        [SerializeField]
        Avatar m_ParticipantAvatarPrefab;

        [SerializeField]
        Transform m_ParticipantRoot;

        readonly Dictionary<INetcodeParticipant, Avatar> m_Avatars = new();
        
        INetcodeService m_NetcodeService;

        DiContainer m_DiContainer;

        [Inject]
        public void Setup(DiContainer container, INetcodeService netCodeService)
        {
            m_DiContainer = container;
            m_NetcodeService = netCodeService;
        }

        void OnEnable()
        {
            m_NetcodeService.ParticipantAdded += OnParticipantAdded;
            m_NetcodeService.ParticipantRemoved += OnParticipantRemoved;
        }

        void OnDisable()
        {
            m_NetcodeService.ParticipantAdded -= OnParticipantAdded;
            m_NetcodeService.ParticipantRemoved -= OnParticipantRemoved;
        }

        void OnParticipantAdded(INetcodeParticipant participant)
        {
            var avatar = m_DiContainer.InstantiatePrefabForComponent<Avatar>(m_ParticipantAvatarPrefab, m_ParticipantRoot);
            avatar.InitParticipant(participant);
            m_Avatars[participant] = avatar;
        }

        void OnParticipantRemoved(INetcodeParticipant participant)
        {
            // Remove avatar when other participants leave
            if (m_Avatars.ContainsKey(participant))
            {
                var avatar = m_Avatars[participant];
                Destroy(avatar.gameObject);
                m_Avatars.Remove(participant);
            }
        }
    }
}
