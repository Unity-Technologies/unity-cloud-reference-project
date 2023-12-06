using System;
using System.Linq;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public interface IFollowManager
    {
        void StopFollowMode();
        event Action<IParticipant, bool> EnterFollowMode;
        event Action ExitFollowMode;
        event Action<IParticipant> ChangeFollowTarget;
        void StartFollowMode(IParticipant participant, bool isPresentation = false);
        void UpdateFollowTarget(IParticipant participant);
        bool IsFollowing { get; }
    }

    public class FollowManager : MonoBehaviour, IFollowManager
    {
        public event Action<IParticipant, bool> EnterFollowMode;
        public event Action ExitFollowMode;
        public event Action<IParticipant> ChangeFollowTarget;
        IParticipant m_CurrentFollow;
        IAssetEvents m_AssetEvents;
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        NetcodeParticipantManager m_ParticipantManager;
        public bool IsFollowing { get; private set; }

        [Inject]
        void Setup(IAssetEvents assetEvents, IPresenceStreamingRoom presenceStreamingRoom,
            NetcodeParticipantManager participantManager)
        {
            m_AssetEvents = assetEvents;
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_ParticipantManager = participantManager;
        }

        void Awake()
        {
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            IsFollowing = false;
        }

        void OnDestroy()
        {
            m_AssetEvents.AssetUnloaded -= OnAssetUnloaded;
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            IsFollowing = false;
        }

        void OnParticipantLeave(IParticipant participant)
        {
            if (m_CurrentFollow != null && participant.Id == m_CurrentFollow.Id)
            {
                StopFollowMode();
            }
        }

        void OnRoomJoined(Room room)
        {
            room.ParticipantRemoved += OnParticipantLeave;
        }

        void OnRoomLeft(Room room)
        {
            if (room == null)
                return;

            room.ParticipantRemoved -= OnParticipantLeave;
        }

        public void StartFollowMode(IParticipant participant, bool isPresentation = false)
        {
            if (!IsFollowing || m_CurrentFollow != participant)
            {
                m_CurrentFollow = participant;

                var netcodeParticipant = m_ParticipantManager.NetcodeParticipants().FirstOrDefault(x => x.IsOwner);
                if (netcodeParticipant != null)
                {
                    GenericDataExtensions.SetValue(netcodeParticipant.GenericDataHandler, Avatar.k_HideKey, true);
                }

                EnterFollowMode?.Invoke(participant, isPresentation);
                IsFollowing = true;
            }
        }

        public void StopFollowMode()
        {
            if (IsFollowing)
            {
                var netcodeParticipant = m_ParticipantManager.NetcodeParticipants().FirstOrDefault(x => x.IsOwner);
                if (netcodeParticipant != null)
                {
                    GenericDataExtensions.SetValue(netcodeParticipant.GenericDataHandler, Avatar.k_HideKey, false);
                }

                IsFollowing = false;
                ExitFollowMode?.Invoke();
            }
        }

        public void UpdateFollowTarget(IParticipant participant)
        {
            if (IsFollowing)
            {
                m_CurrentFollow = participant;
                ChangeFollowTarget?.Invoke(participant);
            }
        }

        void OnAssetUnloaded()
        {
            StopFollowMode();
        }
    }
}
