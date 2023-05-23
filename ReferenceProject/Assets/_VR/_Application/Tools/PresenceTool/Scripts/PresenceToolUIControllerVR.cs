using System.Linq;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Avatar = UnityEngine.Dt.App.UI.Avatar;

namespace Unity.ReferenceProject.VR
{
    public class PresenceToolUIControllerVR : PresenceToolUIController
    {
        Avatar m_AvatarsButton;
        
        public override VisualElement GetIcon()
        {
            m_AvatarsButton = new Avatar();
            m_AvatarsButton.backgroundImage = new StyleBackground(Icon);
            m_AvatarsButton.backgroundColor = Color.clear;
            m_AvatarsButton.size = Size.L;
            return m_AvatarsButton;
        }

        protected override void OnRoomJoined(Room room)
        {
            CurrentRoom = room;
            
            m_AvatarsButton.notificationBadge.text = CurrentRoom.ConnectedParticipants.ToString();

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
                m_AvatarsButton.withNotification = true;
                m_AvatarsButton.notificationBadge.text = CurrentRoom.ConnectedParticipants.Count.ToString();
            }
            else
            {
                m_AvatarsButton.withNotification = false;
                m_AvatarsButton.notificationBadge.text = null;
            }
        }
    }
}
