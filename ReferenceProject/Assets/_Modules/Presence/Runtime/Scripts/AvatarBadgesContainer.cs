using System;
using Unity.Cloud.Presence;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarBadgesContainer : VisualElement
    {
        public static readonly string avatarsContainerUssClassName = "avatar-badge-container";

        public static readonly string avatarBadgeUssClassName = "avatar-badge";
        
        RoomCached m_Room;
        public RoomCached Room => m_Room;
        
        bool m_RemoveOwner;

        public bool RemoveOwner
        {
            get { return m_RemoveOwner; }
            set
            {
                m_RemoveOwner = value;
                RefreshContainer(m_Room);
            }
        }

        int m_MaxAvatarsCount;

        public int MaxParticipantsCount
        {
            get { return m_MaxAvatarsCount; }
            set
            {
                m_MaxAvatarsCount = value;
                RefreshContainer(m_Room);
            }
        }

        public ColorPalette AvatarColorPalette { get; set; }

        public AvatarBadgesContainer()
        {
            name = "avatar-badges-container";
            MaxParticipantsCount = 2;
            RegisterCallback<DetachFromPanelEvent>(_ => UnbindRoomWithoutNotify());
            AddToClassList(avatarsContainerUssClassName);
        }

        public void BindRoom(RoomCached room)
        {
            UnbindRoomWithoutNotify();

            m_Room = room;

            if (m_Room != null)
            {
                m_Room.ParticipantsChanged += OnParticipantsChanged;
            }

            RefreshContainer(m_Room);
        }

        void UnbindRoomWithoutNotify()
        {
            if (m_Room != null)
            {
                m_Room.ParticipantsChanged -= OnParticipantsChanged;
            }
        }

        void OnParticipantsChanged() => RefreshContainer(m_Room);
        

        void RefreshContainer(RoomCached room)
        {
            Clear();
            
            if(room == null)
                return;

            var participants = room.Participants;

            if (participants.Count == 0)
                return;

            var count = 0;
            bool isOwnerRemoved = false;
            foreach (var participant in participants)
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

        VisualElement CreateParticipantBadge(IParticipant participant)
        {
            var avatar = new AvatarBadge();
            avatar.Initials.text = Utils.GetInitials(participant.Name);
            avatar.tooltip = participant.Name;
            avatar.backgroundColor = AvatarColorPalette.GetColor(participant.ColorIndex);
            avatar.size = Size.M;
            avatar.outlineColor = Color.clear;
            avatar.AddToClassList(avatarBadgeUssClassName);
            return avatar;
        }

        static VisualElement CreatePlusBadge(int count)
        {
            var plusSign = new AvatarBadge();
            plusSign.Initials.text = "+" + (count);
            plusSign.backgroundColor = new Color(156, 156, 156, 255) / 255f;
            plusSign.size = Size.M;
            plusSign.outlineColor = Color.clear;
            plusSign.AddToClassList(avatarBadgeUssClassName);
            return plusSign;
        }

        public new class UxmlFactory : UxmlFactory<AvatarBadgesContainer, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_RemoveOwner = new() { name = "remove-owner", defaultValue = false };

            readonly UxmlIntAttributeDescription m_MaxParticipantsCount = new() { name = "max-participants-count", defaultValue = 2 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = (AvatarBadgesContainer)ve;
                container.RemoveOwner = m_RemoveOwner.GetValueFromBag(bag, cc);
                container.MaxParticipantsCount = m_MaxParticipantsCount.GetValueFromBag(bag, cc);
            }
        }
    }
}
