using System;
using System.Linq;
using ModestTree;
using Unity.Cloud.Presence;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarBadgesContainer : VisualElement
    {
        static readonly string k_AvatarBadgesContainerUssClassName = "container__avatar-badges";
        static readonly string k_AvatarBadgeUssClassName = "avatar-badge";
        internal static readonly string PlusSignName = "plus-sign";

        RoomCached m_Room;
        public RoomCached Room => m_Room;

        int m_MaxAvatarsCount;
        public int MaxParticipantsCount
        {
            get => m_MaxAvatarsCount;
            set
            {
                m_MaxAvatarsCount = value;
                RefreshContainer(m_Room);
            }
        }
        
        readonly ColorPalette m_AvatarColorPalette;

        public AvatarBadgesContainer(ColorPalette avatarColorPalette)
        {
            name = k_AvatarBadgesContainerUssClassName;
            MaxParticipantsCount = 2;
            m_AvatarColorPalette = avatarColorPalette;
            RegisterCallback<DetachFromPanelEvent>(_ => UnbindRoomWithoutNotify());
            AddToClassList(k_AvatarBadgesContainerUssClassName);
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

        protected void UnbindRoomWithoutNotify()
        {
            if (m_Room != null)
            {
                m_Room.ParticipantsChanged -= OnParticipantsChanged;
            }
        }

        void OnParticipantsChanged() => RefreshContainer(m_Room);

        protected void RefreshContainer(RoomCached room)
        {
            Clear();

            if (!ClassListContains(k_AvatarBadgesContainerUssClassName))
            {
                ToggleInClassList(k_AvatarBadgesContainerUssClassName);
            }

            if (room == null)
            {
                return;
            }

            RefreshParticipantsBadges(room);
        }

        protected virtual void RefreshParticipantsBadges(RoomCached room)
        {
            if (room == null)
            {
                return;
            }
            
            var participants = room.Participants.ToList();
            participants.Remove(participants.FirstOrDefault(p => p.IsSelf));
            var minCount = Math.Min(m_MaxAvatarsCount, participants.Count);

            foreach (var participant in participants.GetRange(0, minCount))
            {
                Add(CreateParticipantBadge(participant));
            }
            
            var participantsOverflowCount = participants.Count - m_MaxAvatarsCount;
            if (participantsOverflowCount > 0)
            {
                Add(CreatePlusBadge(participantsOverflowCount));
            }
        }

        protected VisualElement CreateParticipantBadge(IParticipant participant)
        {
            var avatar = new AvatarBadge();
            avatar.Initials.text = Utils.GetInitials(participant.Name);
            avatar.tooltip = participant.Name;
            avatar.backgroundColor = m_AvatarColorPalette.GetColor(participant.ColorIndex);
            avatar.size = Size.M;
            avatar.outlineColor = Color.clear;
            avatar.AddToClassList(k_AvatarBadgeUssClassName);
            return avatar;
        }

        static VisualElement CreatePlusBadge(int count)
        {
            var plusSign = new AvatarBadge();
            plusSign.name = PlusSignName;
            plusSign.Initials.text = "+" + (count);
            plusSign.backgroundColor = new Color(156, 156, 156, 255) / 255f;
            plusSign.size = Size.M;
            plusSign.outlineColor = Color.clear;
            plusSign.AddToClassList(k_AvatarBadgeUssClassName);
            return plusSign;
        }
    }
}
