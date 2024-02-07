using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Identity;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public interface IPresenceStreamingRoom
    {
        event Action<Room> RoomJoined;
        event Action<Room> RoomLeft;
        IParticipant GetParticipantFromID(ParticipantId participantId);
    }

    public sealed class PresenceStreamingRoom : MonoBehaviour, IPresenceStreamingRoom
    {
        [SerializeField]
        ColorPalette m_AvatarColorPalette;

        ICloudSession m_Session;
        IPresenceRoomsManager m_PresenceRoomsManager;
        Room m_CurrentRoom;
        IAssetEvents m_AssetEvents;
        ISessionProvider m_SessionProvider;

        public event Action<Room> RoomJoined;
        public event Action<Room> RoomLeft;

        [Inject]
        public void Setup(ICloudSession session, IAssetEvents assetEvents, IPresenceRoomsManager roomManager, ISessionProvider sessionProvider)
        {
            m_Session = session;
            m_PresenceRoomsManager = roomManager;
            m_AssetEvents = assetEvents;
            m_SessionProvider = sessionProvider;

            if (enabled)
            {
                m_PresenceRoomsManager.RegisterOnDisconnect(LeaveRoom);
            }
        }

        void Awake()
        {
            m_AssetEvents.AssetLoaded += OnAssetLoaded;
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;
            m_SessionProvider.SessionChanged += OnRoomSessionChanged;
        }

        void OnDestroy()
        {
            if (m_AssetEvents != null)
            {
                m_AssetEvents.AssetLoaded -= OnAssetLoaded;
                m_AssetEvents.AssetUnloaded -= OnAssetUnloaded;
            }

            if (m_SessionProvider != null)
            {
                m_SessionProvider.SessionChanged -= OnRoomSessionChanged;
            }

            m_PresenceRoomsManager?.UnRegisterOnDisconnect(LeaveRoom);
        }

        void OnEnable()
        {
            if (m_PresenceRoomsManager != null)
            {
                m_PresenceRoomsManager.RegisterOnDisconnect(LeaveRoom);
            }
        }

        async void OnDisable()
        {
            m_PresenceRoomsManager.UnRegisterOnDisconnect(LeaveRoom);
            await LeaveRoom();
        }

        async Task JoinRoom(OrganizationId organizationId, AssetId assetId)
        {
            if (m_CurrentRoom != null)
            {
                Debug.LogError("Previous Room Not Left Yet");
                return;
            }
            
            await m_PresenceRoomsManager.JoinRoomAsync(organizationId, assetId, this);
        }

        void OnRoomSessionChanged(ISession session)
        {
            if (session == null)
            {
                RoomLeft?.Invoke(m_CurrentRoom);
                m_CurrentRoom.ParticipantAdded -= OnParticipantAdded;
                m_Session.UserData.UpdateBadgeColor(Color.grey);
            }
            else if (m_CurrentRoom == null)
            {
                m_CurrentRoom = (Room)session.Room;
                m_CurrentRoom.ParticipantAdded += OnParticipantAdded;

                if (m_CurrentRoom.ConnectedParticipants is { Count: > 0 })
                {
                    var self = m_CurrentRoom.ConnectedParticipants.FirstOrDefault(p => p.IsSelf);
                    m_Session.UserData.UpdateBadgeColor(self == null ? Color.grey : m_AvatarColorPalette.GetColor(self.ColorIndex));
                }

                RoomJoined?.Invoke(m_CurrentRoom);
            }
        }

        void OnParticipantAdded(IParticipant participant)
        {
            if (participant.IsSelf)
            {
                m_Session.UserData.UpdateBadgeColor(m_AvatarColorPalette.GetColor(participant.ColorIndex));
            }
        }

        async Task LeaveRoom()
        {
            if (m_CurrentRoom != null)
            {
                await m_PresenceRoomsManager.LeaveRoomAsync(m_CurrentRoom);
                await m_PresenceRoomsManager.UnsubscribeFromMonitoring(m_CurrentRoom, this);
                m_CurrentRoom = null;
            }
        }

        async void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            await LeaveRoom();
            await JoinRoom(asset.Descriptor.OrganizationGenesisId, asset.Descriptor.AssetId);
        }

        async void OnAssetUnloaded()
        {
            await LeaveRoom();
        }

        public IParticipant GetParticipantFromID(ParticipantId participantId)
        {
            return m_CurrentRoom?.ConnectedParticipants.FirstOrDefault(p => p.Id == participantId);
        }
    }
}
