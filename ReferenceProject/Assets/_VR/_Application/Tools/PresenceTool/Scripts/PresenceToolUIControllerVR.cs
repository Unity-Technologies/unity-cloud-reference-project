using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
namespace Unity.ReferenceProject.VR
{
    public class PresenceToolUIControllerVR : PresenceToolUIController
    {
        Badge m_Badge;

        public override VisualElement GetButtonContent()
        {
            var icon = GetIcon();

            m_Badge = new Badge();
            m_Badge.verticalAnchor = VerticalAnchor.Bottom;
            m_Badge.horizontalAnchor = HorizontalAnchor.Right;
            m_Badge.overlapType = BadgeOverlapType.Circular;

            icon.Add(m_Badge);

            return icon;
        }

        protected override void OnRoomJoined(Room room)
        {
            CurrentRoom = room;

            m_Badge.content = CurrentRoom.ConnectedParticipants.Count;

            foreach (var participant in room.ConnectedParticipants)
            {
                CollaboratorsDataPanel.AddParticipant(participant);
            }

            RefreshVisualTree();

            CurrentRoom.ParticipantAdded += OnParticipantAdded;
            CurrentRoom.ParticipantRemoved += OnParticipantRemoved;
        }

        protected override void RefreshVisualTree()
        {
            RefreshAvatarNotificationBadge();

            base.RefreshVisualTree();
        }

        void RefreshAvatarNotificationBadge()
        {
            if (CurrentRoom != null && CurrentRoom.ConnectedParticipants.Count > (IsRemoveOwner ? 1 : 0))
            {
                m_Badge.visible = true;
                m_Badge.content = CurrentRoom.ConnectedParticipants.Count;
            }
            else
            {
                m_Badge.visible = false;
                m_Badge.content = 0;
            }
        }
    }
}
