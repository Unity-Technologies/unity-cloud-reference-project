using System;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarBadgesContainer : VisualElement
    {
        public static readonly string avatarsContainerUssClassName = "avatar-badge-container";
        
        public static readonly string avatarBadgeUssClassName = "avatar-badge";
        
        Room m_Room;
        
        bool m_RemoveOwner;
        
        public bool removeOwner
        {
            get
            {
                return m_RemoveOwner;
            }
            set
            {
                m_RemoveOwner = value;
                RefreshContainer(m_Room);
            }
        }

        int m_MaxAvatarsCount;
        
        public int maxParticipantsCount
        {
            get
            {
                return m_MaxAvatarsCount;
            }
            set
            {
                m_MaxAvatarsCount = value;
                RefreshContainer(m_Room);
            }
        }

        public AvatarBadgesContainer()
        {
            name = "avatar-badges-container";
            maxParticipantsCount = 2;
            RegisterCallback<DetachFromPanelEvent>(_ => UnbindRoomWithoutNotify());
            AddToClassList(avatarsContainerUssClassName);
        }

        public void BindRoom(Room room)
        {
            UnbindRoomWithoutNotify();
            
            m_Room = room;

            if (m_Room != null)
            {
                m_Room.ParticipantAdded += OnParticipantAdded;
                m_Room.ParticipantRemoved += OnParticipantRemoved;
            }

            RefreshContainer(m_Room);
        }

        void UnbindRoomWithoutNotify()
        {
            if (m_Room != null)
            {
                m_Room.ParticipantAdded -= OnParticipantAdded;
                m_Room.ParticipantRemoved -= OnParticipantRemoved;
            }
        }

        void OnParticipantRemoved(IParticipant obj) => RefreshContainer(m_Room);

        void OnParticipantAdded(IParticipant obj) => RefreshContainer(m_Room);

        void RefreshContainer(Room room)
        {
            Clear();

            if (room == null || room.ConnectedParticipants.Count == 0)
                return;

            var count = 0;
            bool isOwnerRemoved = false;
            foreach (var participant in room.ConnectedParticipants)
            {
                count++;
                if (m_RemoveOwner && participant.IsSelf)
                {
                    isOwnerRemoved = true;
                    continue;
                }
                
                if (count <= m_MaxAvatarsCount)
                    Add(CreateParticipantBadge(participant));
            }
            
            var participantsOverflowCount = count - m_MaxAvatarsCount - (isOwnerRemoved ? 1 : 0);
            
            if (participantsOverflowCount > 0)
            {
                Add(CreatePlusBadge(participantsOverflowCount));
            }
        }

        static VisualElement CreateParticipantBadge(IParticipant participant)
        {
            var avatar = new UnityEngine.Dt.App.UI.Avatar
            {
                text = AvatarUtils.GetInitials(participant.Name),
                tooltip = participant.Name,
                backgroundColor = AvatarUtils.GetColor(participant.ColorIndex), // This is temporary as the Presence Package will update its implementation
                size = Size.M
            };
            avatar.AddToClassList(avatarBadgeUssClassName);
            return avatar;
        }
        
        static VisualElement CreatePlusBadge(int count)
        {
            var plusSign = new UnityEngine.Dt.App.UI.Avatar();
            plusSign.text = "+" + (count);
            plusSign.backgroundColor = new Color(156, 156, 156, 255) / 255f;
            plusSign.size = Size.M;
            plusSign.AddToClassList(avatarBadgeUssClassName);
            return plusSign;
        }

        public new class UxmlFactory : UxmlFactory<AvatarBadgesContainer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_RemoveOwner = new () { name = "remove-owner", defaultValue = false };
            
            readonly UxmlIntAttributeDescription m_MaxParticipantsCount = new() { name = "max-participants-count", defaultValue = 2 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = (AvatarBadgesContainer)ve;
                container.removeOwner = m_RemoveOwner.GetValueFromBag(bag, cc);
                container.maxParticipantsCount = m_MaxParticipantsCount.GetValueFromBag(bag, cc);
                
            }
        }
    }
}
