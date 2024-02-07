using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class AssetListRoomMonitoringController : MonoBehaviour
    {
        IAssetListController m_AssetListController;
        IPresenceRoomsManager m_PresenceRoomsManager;
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        readonly HashSet<Room> m_RoomsMonitoredFromAsset = new();

        [Inject]
        public void Setup(IAssetListController assetListController, IPresenceRoomsManager presenceRoomsManager, IPresenceStreamingRoom presenceStreamingRoom)
        {
            m_AssetListController = assetListController;
            m_PresenceRoomsManager = presenceRoomsManager;
            m_PresenceStreamingRoom = presenceStreamingRoom;
        }

        void OnEnable()
        {
            m_AssetListController.AssetHighlighted += OnAssetHighlighted;
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
        }

        void OnDisable()
        {
            m_AssetListController.AssetHighlighted -= OnAssetHighlighted;
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
        }

        async void OnAssetHighlighted(IAsset obj)
        {
            if (obj == null)
                return;

            var assetId = obj.Descriptor.AssetId;
            var room = m_PresenceRoomsManager.GetRoomForAsset(assetId);

            if (room == null)
            {
                room = m_PresenceRoomsManager.CheckPermissions(PresencePermission.CreateRoom) ? await m_PresenceRoomsManager.CreateRoomAsync(obj.Descriptor.OrganizationGenesisId, assetId) : await m_PresenceRoomsManager.GetRoomAsync(obj.Descriptor.OrganizationGenesisId, assetId);

                if (room == null)
                    return;

                m_PresenceRoomsManager.AddRoomForAsset(assetId, room);
            }

            await m_PresenceRoomsManager.SubscribeForMonitoring(room, this);
            m_RoomsMonitoredFromAsset.Add(room);
        }

        async void OnRoomJoined(Room obj)
        {
            if (obj == null)
                return;

            foreach (var room in m_RoomsMonitoredFromAsset)
            {
                await m_PresenceRoomsManager.UnsubscribeFromMonitoring(room, this);
            }

            m_RoomsMonitoredFromAsset.Clear();
        }
    }
}
