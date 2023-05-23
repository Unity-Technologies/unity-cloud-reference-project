using System;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class PresenceToolUIController : ToolUIController
    {
        [SerializeField]
        bool m_IsRemoveOwner;
        
        [SerializeField]
        int m_MaxParticipantsCount = 4;

        PresenceStreamingRoom m_PresenceStreamingRoom;
        AvatarBadgesContainer m_AvatarsBadgesContainer;
        VisualElement m_ListContainerVisualElement;
        CollaboratorsDataPanel m_CollaboratorsDataPanel;

        protected bool IsRemoveOwner => m_IsRemoveOwner;
        protected CollaboratorsDataPanel CollaboratorsDataPanel => m_CollaboratorsDataPanel;
        protected Room CurrentRoom { get; set; }

        [Inject]
        void Setup(PresenceStreamingRoom presenceStreamingRoom)
        {
            m_PresenceStreamingRoom = presenceStreamingRoom;
        }

        protected override void Awake()
        {
            base.Awake();
            m_CollaboratorsDataPanel = new CollaboratorsDataPanel();
        }

        void OnEnable()
        {
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
        }

        void OnDisable()
        {
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
        }
        
        protected virtual void OnRoomJoined(Room room)
        {
            CurrentRoom = room;
            m_AvatarsBadgesContainer.BindRoom(room);

            foreach (var participant in room.ConnectedParticipants)
            {
                m_CollaboratorsDataPanel.AddParticipant(participant);
            }
            
            RefreshVisualTree();
            
            CurrentRoom.ParticipantAdded += OnParticipantAdded;
            CurrentRoom.ParticipantRemoved += OnParticipantRemoved;
        }
        
        protected void OnRoomLeft()
        {
            m_CollaboratorsDataPanel.ClearParticipants();
            
            CurrentRoom.ParticipantAdded -= OnParticipantAdded;
            CurrentRoom.ParticipantRemoved -= OnParticipantRemoved;

            CurrentRoom = null;
            
            RefreshVisualTree();
        }
        
        protected void OnParticipantRemoved(IParticipant participant)
        {
            m_CollaboratorsDataPanel.RemoveParticipant(participant);
            RefreshVisualTree();
        }

        protected void OnParticipantAdded(IParticipant participant)
        {
            if (m_IsRemoveOwner && participant.IsSelf)
                return;
            
            m_CollaboratorsDataPanel.AddParticipant(participant);
            RefreshVisualTree();
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            
            if (m_ListContainerVisualElement == null || m_CollaboratorsDataPanel.IsDirty)
            {
                m_ListContainerVisualElement = m_CollaboratorsDataPanel.CreateVisualTree();

                rootVisualElement.Add(m_ListContainerVisualElement);
            }
            
            return rootVisualElement;
        }
        
        protected virtual void RefreshVisualTree()
        {
            if (m_CollaboratorsDataPanel.IsDirty)
            {
                m_ListContainerVisualElement.Clear();
                m_ListContainerVisualElement = m_CollaboratorsDataPanel.CreateVisualTree();
            }
        }

        public override VisualElement GetIcon()
        {
            m_AvatarsBadgesContainer = new AvatarBadgesContainer
            {
                maxParticipantsCount = m_MaxParticipantsCount,
                removeOwner = m_IsRemoveOwner
            };
            return m_AvatarsBadgesContainer;
        }
    }
}
