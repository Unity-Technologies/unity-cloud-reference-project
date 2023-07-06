using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cloud.Common;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListCollaborators : TableListColumn
    {
        [SerializeField]
        bool m_IsRemoveOwner;

        [SerializeField]
        string m_AvatarsContainerName;

        [SerializeField]
        string[] m_AvatarsStyles;

        [SerializeField]
        ColorPalette m_AvatarColorPalette;

        PresenceRoomsManager m_PresenceRoomsManager;

        readonly Dictionary<RoomId, RoomCached> m_RoomCache = new ();

        [Inject]
        void Setup(PresenceRoomsManager presenceRoomsManager)
        {
            m_PresenceRoomsManager = presenceRoomsManager;
        }

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildTextHeader(e, columnData);
        }

        readonly Dictionary<Room, int> m_Monitoring = new ();

        void SubscribeForRoomMonitoring(Room room, bool isSubscribe)
        {
            if (!m_Monitoring.ContainsKey(room))
            {
                m_Monitoring.Add(room, 0);
            }
            
            m_Monitoring[room] += isSubscribe ? 1 : -1;

            if (m_Monitoring[room] <= 0)
            {
                m_Monitoring[room] = 0;
                m_PresenceRoomsManager.UnsubscribeFromMonitoring(room, GetInstanceID());
            }
            else
            {
                m_PresenceRoomsManager.SubscribeForMonitoring(room, GetInstanceID());
            }
        }

        void UnsubscribeFromAllRoomsMonitoring()
        {
            foreach (var item in m_Monitoring)
            {
                if(m_PresenceRoomsManager == null)
                    break;
                
                m_PresenceRoomsManager.UnsubscribeFromMonitoring(item.Key, GetInstanceID());
            }
        }

        protected override void OnBindCell(VisualElement e, IColumnData columnData, object data)
        {
            if (data is not IScene scene)
                return;

            var avatarsContainer = e.Q<AvatarBadgesContainer>(m_AvatarsContainerName);
            var room = m_PresenceRoomsManager.GetRoomForScene(scene.Id);
            if (room == null)
            {
                Debug.LogError($"Room didn't initialize yet!");
                return;
            }
            
            if(!m_RoomCache.ContainsKey(room.RoomId))
                m_RoomCache.Add(room.RoomId, new RoomCached(room));
            avatarsContainer.BindRoom(m_RoomCache[room.RoomId]);
            
            SubscribeForRoomMonitoring(room, true);
        }

        protected override void OnUnbindCell(VisualElement e, IColumnData columnData, object data)
        {
            base.OnUnbindCell(e, columnData, data);
            if (data is not IScene scene)
                return;
            
            var room = m_PresenceRoomsManager.GetRoomForScene(scene.Id);
            if (room == null)
            {
                Debug.LogError($"Room didn't initialize yet!");
                return;
            }
            SubscribeForRoomMonitoring(room, false);
        }

        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var avatarsContainer = new AvatarBadgesContainer
            {
                name = m_AvatarsContainerName,
                MaxParticipantsCount = 2,
                RemoveOwner = m_IsRemoveOwner,
                AvatarColorPalette = m_AvatarColorPalette
            };

            foreach (var style in m_AvatarsStyles)
            {
                avatarsContainer.AddToClassList(style);
            }

            e.Add(avatarsContainer);
        }

        protected override void OnReset()
        {
            base.OnReset();
            UnsubscribeFromAllRoomsMonitoring();
            m_Monitoring.Clear();
        }
    }
}
