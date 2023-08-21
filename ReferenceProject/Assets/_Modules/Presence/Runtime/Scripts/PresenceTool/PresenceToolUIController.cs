using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class PresenceToolUIController : ToolUIController
    {
        [SerializeField]
        int m_MaxParticipantsCount = 4;

        [SerializeField]
        ColorPalette m_AvatarColorPalette;
        
        [SerializeField]
        string m_VivoxUnsupportedString = "@Presence:Vivox_Unsupported";

        static readonly string k_CollaboratorDataMicrophoneUssClassName = "collaborator-data-microphone";
        
        PresenceStreamingRoom m_PresenceStreamingRoom;
        VoiceAvatarBadgesContainer m_VoiceAvatarsBadgesContainer;
        VisualElement m_ListContainerVisualElement;
        CollaboratorsDataPanel m_CollaboratorsDataPanel;
        VoiceManager m_VoiceManager;
        IconButton m_MuteButton;

        protected CollaboratorsDataPanel CollaboratorsDataPanel => m_CollaboratorsDataPanel;
        protected Room CurrentRoom { get; set; }

        [Inject]
        void Setup(PresenceStreamingRoom presenceStreamingRoom, VoiceManager voiceManager)
        {
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_VoiceManager = voiceManager;
        }

        protected override void Awake()
        {
            base.Awake();
            m_CollaboratorsDataPanel = new CollaboratorsDataPanel()
            {
                AvatarColorPalette = m_AvatarColorPalette
            };
        }

        void OnEnable()
        {
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            m_VoiceManager.VoiceStatusChanged += VoiceStatusChanged;
            m_VoiceManager.VoiceServiceUpdated += VoiceServiceUpdated;
            
            VoiceStatusChanged(m_VoiceManager.CurrentVoiceStatus);
        }

        void OnDisable()
        {
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            m_VoiceManager.VoiceStatusChanged -= VoiceStatusChanged;
            m_VoiceManager.VoiceServiceUpdated -= VoiceServiceUpdated;
            m_VoiceManager.MuteStatusUpdated -= UpdateMuteButtonIcon;
        }

        protected virtual void OnRoomJoined(Room room)
        {
            CurrentRoom = room;

            var cachedRoom = m_VoiceAvatarsBadgesContainer.Room ?? new RoomCached(null);
            cachedRoom.Room = room; // Assign new room
            m_VoiceAvatarsBadgesContainer.BindRoom(cachedRoom);
            m_VoiceAvatarsBadgesContainer.BindVoiceManager(m_VoiceManager);
            
            foreach (var participant in room.ConnectedParticipants)
            {
                if (participant.IsSelf)
                    continue;
                
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
            m_CollaboratorsDataPanel.RemoveVoiceToParticipant(participant);
            m_CollaboratorsDataPanel.RemoveParticipant(participant);

            RefreshVisualTree();
        }

        protected void OnParticipantAdded(IParticipant participant)
        {
            if (participant.IsSelf)
                return;

            m_CollaboratorsDataPanel.AddParticipant(participant);
            
            var voice = m_VoiceManager.GetVoiceParticipant(participant);
            if (voice != null)
            {
                m_CollaboratorsDataPanel.AddVoiceToParticipant(participant, (VoiceParticipant)voice);
            }
            
            RefreshVisualTree();
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            var container = new VisualElement();
            container.AddToClassList(k_CollaboratorDataMicrophoneUssClassName);
            
            m_MuteButton = new IconButton(m_VoiceManager.Muted ? "microphone-slash" : "microphone", () =>
            {
                m_VoiceManager.Muted = !m_VoiceManager.Muted;
            });
            m_VoiceManager.MuteStatusUpdated += UpdateMuteButtonIcon;

            if (m_VoiceManager.CurrentVoiceStatus == VoiceStatus.Unsupported)
            {
                m_MuteButton.tooltip = m_VivoxUnsupportedString;
                m_MuteButton.SetEnabled(false);
            }

            container.Add(m_MuteButton);
            rootVisualElement.Add(container);

            if (m_ListContainerVisualElement == null || m_CollaboratorsDataPanel.IsDirty)
            {
                m_ListContainerVisualElement = m_CollaboratorsDataPanel.CreateVisualTree();
                rootVisualElement.Add(m_ListContainerVisualElement);
            }

            return rootVisualElement;
        }

        void UpdateMuteButtonIcon(bool isMuted)
        {
            m_MuteButton.icon = isMuted switch
            {
                true => "microphone-slash",
                _ => "microphone"
            };
        }

        protected virtual void RefreshVisualTree()
        {
            if (m_CollaboratorsDataPanel.IsDirty)
            {
                m_ListContainerVisualElement.Clear();
                m_ListContainerVisualElement = m_CollaboratorsDataPanel.CreateVisualTree();
                
                RefreshButtonDisplayStyle();
            }
        }

        void RefreshButtonDisplayStyle()
        {
            if (CurrentRoom != null && CurrentRoom.ConnectedParticipants.Where(p => !p.IsSelf).ToList().Count > 0)
            {
                SetButtonDisplayStyle(DisplayStyle.Flex);
            }
            else
            {
                SetButtonDisplayStyle(DisplayStyle.None);
            }
        }

        public override VisualElement GetButtonContent()
        {
            m_VoiceAvatarsBadgesContainer = new VoiceAvatarBadgesContainer(m_AvatarColorPalette)
            {
                MaxParticipantsCount = m_MaxParticipantsCount,
            };
            return m_VoiceAvatarsBadgesContainer;
        }
        
        void VoiceStatusChanged(VoiceStatus status)
        {
            switch (status)
            {
                case VoiceStatus.Unsupported:
                    m_CollaboratorsDataPanel.DisableVivox();
                    break;
                case VoiceStatus.NotConnected:
                case VoiceStatus.NoRoom:
                    if (CurrentRoom == null)
                        break;
                    
                    foreach (var participant in CurrentRoom.ConnectedParticipants)
                    {
                        m_CollaboratorsDataPanel.RemoveVoiceToParticipant(participant);
                    }
                    break;
                case VoiceStatus.Connected:
                    if (CurrentRoom == null)
                        break;

                    foreach (var participant in CurrentRoom.ConnectedParticipants)
                    {
                        var voice = m_VoiceManager.GetVoiceParticipant(participant);
                        if (voice != null)
                        {
                            m_CollaboratorsDataPanel.AddVoiceToParticipant(participant, (VoiceParticipant)voice);
                        }
                    }
                    break;
            }
            
            RefreshVisualTree();
        }

        void VoiceServiceUpdated(IEnumerable<VoiceParticipant> participants)
        {
            m_CollaboratorsDataPanel.VoiceServiceUpdated(participants);
        }
    }
}
