using System;
using Unity.Cloud.Common;
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

        PresenceRoomsManager m_PresenceRoomsManager;
        
        [Inject]
        void Setup(PresenceRoomsManager presenceRoomsManager)
        {
            m_PresenceRoomsManager = presenceRoomsManager;
        }

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildTextHeader(e, columnData);
        }

        protected override void OnBindCell(VisualElement e, IColumnData columnData, object data)
        {
            if (data is not IScene scene)
                return;

            var avatarsContainer = e.Q<AvatarBadgesContainer>(m_AvatarsContainerName);
            avatarsContainer.BindRoom(m_PresenceRoomsManager.GetRoomForScene(scene.Id));
        }

        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var avatarsContainer = new AvatarBadgesContainer
            {
                name = m_AvatarsContainerName,
                maxParticipantsCount = 2,
                removeOwner = m_IsRemoveOwner
            };
            
            foreach (var style in m_AvatarsStyles)
            {
                avatarsContainer.AddToClassList(style);
            }

            e.Add(avatarsContainer);
        }
    }
}
