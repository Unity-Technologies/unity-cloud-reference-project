using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Cloud.Presence;

#if USE_VIVOX
using System.Threading;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Threading.Tasks;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
using Zenject;
#endif

namespace Unity.ReferenceProject.Presence
{
    public enum VoiceStatus
    {
        Unsupported,
        NotConnected,
        Connected
    }

    public interface IVoiceManager
    {
        event Action<VoiceStatus> VoiceStatusChanged;
        event Action<IEnumerable<IVoiceParticipant>> VoiceParticipantAdded;
        event Action<IEnumerable<IVoiceParticipant>> VoiceParticipantRemoved;
        event Action<IEnumerable<IVoiceParticipant>> VoiceParticipantUpdated;
        event Action<bool> MuteStatusUpdated;
        VoiceStatus CurrentVoiceStatus { get; }
        bool Muted { get; set; }
        IVoiceParticipant GetVoiceParticipant(IParticipant participant);
        void CheckPermissions();
    }

    public class VoiceManager : MonoBehaviour, IVoiceManager
    {
        VoiceStatus m_CurrentVoiceStatus = VoiceStatus.Unsupported;

        public VoiceStatus CurrentVoiceStatus
        {
            get => m_CurrentVoiceStatus;
            private set
            {
                if (m_CurrentVoiceStatus != value)
                {
                    m_CurrentVoiceStatus = value;
                    VoiceStatusChanged?.Invoke(m_CurrentVoiceStatus);
                }
            }
        }

        public event Action<VoiceStatus> VoiceStatusChanged;
        public event Action<bool> MuteStatusUpdated;
        public event Action<IEnumerable<IVoiceParticipant>> VoiceParticipantAdded;
        public event Action<IEnumerable<IVoiceParticipant>> VoiceParticipantRemoved;
        public event Action<IEnumerable<IVoiceParticipant>> VoiceParticipantUpdated;
#if USE_VIVOX
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        IPresenceVivoxServiceComponents m_PresenceVivoxServiceComponents;
        ServicesInitializer m_ServicesInitializer;
        CancellationTokenSource m_LeavingRoomCancellationToken;
        IPresenceRoomsManager m_PresenceRoomsManager;

        [Inject]
        void Setup(IPresenceRoomsManager presenceRoomsManager, IPresenceStreamingRoom presenceStreamingRoom, IPresenceVivoxServiceComponents presenceVivoxServiceComponents, InitializationOptions initializationOptions, ServicesInitializer servicesInitializer)
        {
            CurrentVoiceStatus = VoiceStatus.NotConnected;
            m_PresenceRoomsManager = presenceRoomsManager;
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_PresenceVivoxServiceComponents = presenceVivoxServiceComponents;
            m_ServicesInitializer = servicesInitializer;
            initializationOptions.SetVivoxCredentials(m_PresenceVivoxServiceComponents.VivoxServer, m_PresenceVivoxServiceComponents.VivoxDomain, m_PresenceVivoxServiceComponents.VivoxIssuer);
        }

        void Awake()
        {
            if (m_ServicesInitializer.Initialized)
            {
                OnUnityServicesInitialized();
            }
            else
            {
                m_ServicesInitializer.UnityServicesInitialized += OnUnityServicesInitialized;
            }
        }

        async void OnUnityServicesInitialized()
        {
            VivoxService.Instance.SetTokenProvider(m_PresenceVivoxServiceComponents.VivoxTokenProvider);
            await VivoxService.Instance.InitializeAsync();
            Muted = true;
            m_ServicesInitializer.UnityServicesInitialized -= OnUnityServicesInitialized;
        }

        void OnEnable()
        {
            CurrentVoiceStatus = VoiceStatus.NotConnected;
            m_PresenceVivoxServiceComponents.VoiceService.VoiceParticipantAdded += OnVoiceParticipantAdded;
            m_PresenceVivoxServiceComponents.VoiceService.VoiceParticipantRemoved += OnVoiceParticipantRemoved;
            m_PresenceVivoxServiceComponents.VoiceService.VoiceParticipantUpdated += OnVoiceParticipantUpdated;
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
        }

        void OnDisable()
        {
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            m_PresenceVivoxServiceComponents.VoiceService.VoiceParticipantAdded -= OnVoiceParticipantAdded;
            m_PresenceVivoxServiceComponents.VoiceService.VoiceParticipantRemoved -= OnVoiceParticipantRemoved;
            m_PresenceVivoxServiceComponents.VoiceService.VoiceParticipantUpdated -= OnVoiceParticipantUpdated;
        }

        void OnRoomJoined(Room room)
        {
            CheckPermissions();
            if (CurrentVoiceStatus == VoiceStatus.Unsupported)
                return;
            m_LeavingRoomCancellationToken = new CancellationTokenSource();
            OnJoinVoice().ConfigureAwait(true);
        }

        void OnRoomLeft(Room room)
        {
            if (CurrentVoiceStatus == VoiceStatus.Unsupported)
                return;
            OnLeaveVoice().ConfigureAwait(true);
        }

        void OnVoiceParticipantAdded(IEnumerable<IVoiceParticipant> voiceParticipants)
        {
            if (CurrentVoiceStatus == VoiceStatus.Unsupported)
                return;
            CurrentVoiceStatus = m_PresenceVivoxServiceComponents.VoiceService.SelfVoiceParticipant != null ? VoiceStatus.Connected : VoiceStatus.NotConnected;
            VoiceParticipantAdded?.Invoke(voiceParticipants);
        }

        void OnVoiceParticipantRemoved(IEnumerable<IVoiceParticipant> voiceParticipants)
        {
            if (CurrentVoiceStatus == VoiceStatus.Unsupported)
                return;
            CurrentVoiceStatus = m_PresenceVivoxServiceComponents.VoiceService.SelfVoiceParticipant != null ? VoiceStatus.Connected : VoiceStatus.NotConnected;
            VoiceParticipantRemoved?.Invoke(voiceParticipants);
        }

        void OnVoiceParticipantUpdated(IEnumerable<IVoiceParticipant> voiceParticipants)
        {
            if (CurrentVoiceStatus == VoiceStatus.Unsupported)
                return;
            CurrentVoiceStatus = m_PresenceVivoxServiceComponents.VoiceService.SelfVoiceParticipant != null ? VoiceStatus.Connected : VoiceStatus.NotConnected;
            VoiceParticipantUpdated?.Invoke(voiceParticipants);
        }

        async Task OnJoinVoice()
        {
            while (CurrentVoiceStatus == VoiceStatus.Connected)
            {
                await Task.Delay(1, m_LeavingRoomCancellationToken.Token);
            }

            await m_PresenceVivoxServiceComponents.VoiceService.JoinAsync();
            m_LeavingRoomCancellationToken.Dispose();
            m_LeavingRoomCancellationToken = null;
        }

        async Task OnLeaveVoice()
        {
            m_LeavingRoomCancellationToken?.Cancel();
            await m_PresenceVivoxServiceComponents.VoiceService.LeaveAsync().ContinueWith(t =>
            {
                DisconnectedToVoice();
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        void DisconnectedToVoice()
        {
            CurrentVoiceStatus = VoiceStatus.NotConnected;
        }

        public static CommunicationId GetVoiceId(IVoiceParticipant voiceParticipant)
        {
            return voiceParticipant?.Id ?? CommunicationId.None;
        }
#endif
        public bool Muted
        {
            get
            {
#if USE_VIVOX
                return VivoxService.Instance != null && m_PresenceVivoxServiceComponents.VoiceService.Muted;
#else
                return true;
#endif
            }
            set
            {
#if USE_VIVOX
                if (VivoxService.Instance != null)
                {
                    m_PresenceVivoxServiceComponents.VoiceService.Muted = value;
                }
#endif
                MuteStatusUpdated?.Invoke(value);
            }
        }

        [NotNull]
        public IVoiceParticipant GetVoiceParticipant(IParticipant participant)
        {
#if USE_VIVOX
            return m_PresenceVivoxServiceComponents.VoiceService.GetServiceParticipant((Participant)participant);
#else
            return null;
#endif
        }
        
        public void CheckPermissions()
        {
#if USE_VIVOX
            if (!m_PresenceRoomsManager.CheckPermissions(PresencePermission.UseCommunication))
            {
                CurrentVoiceStatus = VoiceStatus.Unsupported;
            }
#endif
        }
    }
}
