using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Cloud.Presence;

#if USE_VIVOX
using System.Threading.Tasks;
using Unity.Cloud.Presence.Runtime;
using Zenject;
#endif

namespace Unity.ReferenceProject.Presence
{
    public enum VoiceStatus
    {
        Unsupported,
        NotConnected,
        NoRoom,
        Connected
    }

    public class VoiceManager : MonoBehaviour
    {
        VoiceStatus m_CurrentVoiceStatus = VoiceStatus.Unsupported;

        public VoiceStatus CurrentVoiceStatus
        {
            get => m_CurrentVoiceStatus;
            private set
            {
                m_CurrentVoiceStatus = value;
                VoiceStatusChanged?.Invoke(m_CurrentVoiceStatus);
            }
        }

        public event Action<VoiceStatus> VoiceStatusChanged;
        public event Action<bool> MuteStatusUpdated;
        public event Action<IEnumerable<VoiceParticipant>> VoiceServiceUpdated;
#if USE_VIVOX
        PresenceStreamingRoom m_PresenceStreamingRoom;
        IVoiceService m_VoiceService;

        [Inject]
        void Setup(PresenceStreamingRoom presenceStreamingRoom, IVoiceService voiceService)
        {
            CurrentVoiceStatus = VoiceStatus.NotConnected;
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_VoiceService = voiceService;
        }
        
        void OnEnable()
        {
            m_VoiceService.VoiceUpdated += OnVoiceUpdated;
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            Muted = true;
        }

        void OnDisable()
        {
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            m_VoiceService.VoiceUpdated -= OnVoiceUpdated;
        }

        void OnRoomJoined(Room obj)
        {
            CurrentVoiceStatus = VoiceStatus.NotConnected;
            OnJoinVoice();
        }

        void OnRoomLeft()
        {
            OnLeaveVoice();
            CurrentVoiceStatus = VoiceStatus.NoRoom;
        }

        void OnVoiceUpdated(IEnumerable<VoiceParticipant> voiceParticipants)
        {
            CurrentVoiceStatus = m_VoiceService.MyVoiceParticipant.HasValue ? VoiceStatus.Connected : VoiceStatus.NotConnected;
            VoiceServiceUpdated?.Invoke(voiceParticipants);
        }
        
        void OnJoinVoice()
        {
            m_VoiceService.JoinAsync();
        }

        void OnLeaveVoice()
        {
            m_VoiceService.LeaveAsync().ContinueWith(t =>
            {
                DisconnectedToVoice();
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
        
        void DisconnectedToVoice()
        {
            CurrentVoiceStatus = VoiceStatus.NotConnected;
        }
        
        public static VoiceId GetVoiceId(VoiceParticipant? voiceParticipant)
        {
            return voiceParticipant?.VoiceId ?? VoiceId.None;
        }
#endif
        public bool Muted {
            get
            {
#if USE_VIVOX
                return m_VoiceService.Muted;
#else
                return true;
#endif
            }
            set
            {
#if USE_VIVOX
                m_VoiceService.Muted = value;
#endif
                MuteStatusUpdated?.Invoke(value);
            } 
        }

        public VoiceParticipant? GetVoiceParticipant(IParticipant participant)
        {
#if USE_VIVOX
            return m_VoiceService.GetServiceParticipant((Participant)participant);
#else
            return null;
#endif
        }
    }
}
