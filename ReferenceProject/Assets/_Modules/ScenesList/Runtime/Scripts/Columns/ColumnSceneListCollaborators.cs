using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.Presence;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListCollaborators : TableListColumn
    {
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
            if(data is not IScene scene)
                return;
            
            var avatarsContainer = e.Q<AvatarBadgesContainer>(m_AvatarsContainerName);
            avatarsContainer.Clear();
            m_PresenceRoomsManager.BindRoomEventsToAvatarBadgesContainer(scene.Id, avatarsContainer);
        }

        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var avatarsContainer = AvatarBadgesContainer.Build();
            avatarsContainer.name = m_AvatarsContainerName;
            foreach (var style in m_AvatarsStyles)
            {
                avatarsContainer.AddToClassList(style);
            }
            
            e.Add(avatarsContainer);
        }
    }
}
