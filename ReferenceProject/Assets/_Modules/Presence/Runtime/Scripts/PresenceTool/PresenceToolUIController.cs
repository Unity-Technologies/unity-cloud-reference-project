using System.Collections.Generic;
using System.Linq;
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
        bool m_ShowFollow = true;

        [SerializeField]
        ColorPalette m_AvatarColorPalette;

        [SerializeField]
        StyleSheet m_VoiceLevelStyleSheet;

        IPresenceStreamingRoom m_PresenceStreamingRoom;
        VoiceAvatarBadgesContainer m_VoiceAvatarsBadgesContainer;
        VisualElement m_ListContainerVisualElement;
        CollaboratorsDataPanel m_CollaboratorsDataPanel;
        IVoiceManager m_VoiceManager;
        IFollowManager m_FollowManager;
        VoiceLevelMicrophoneButton m_MuteButton;

        protected CollaboratorsDataPanel CollaboratorsDataPanel => m_CollaboratorsDataPanel;
        protected Room CurrentRoom { get; set; }

        [Inject]
        void Setup(IPresenceStreamingRoom presenceStreamingRoom, IVoiceManager voiceManager, IFollowManager followManager)
        {
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_VoiceManager = voiceManager;
            m_FollowManager = followManager;
        }

        protected override void Awake()
        {
            base.Awake();
            m_CollaboratorsDataPanel = new CollaboratorsDataPanel()
            {
                AvatarColorPalette = m_AvatarColorPalette,
                VoiceLevelStyleSheet = m_VoiceLevelStyleSheet,
                ShowFollowButton = m_ShowFollow
            };
            m_FollowManager.EnterFollowMode += OnEnterFollowMode;
            m_FollowManager.ExitFollowMode += OnExitFollowMode;
            m_FollowManager.ChangeFollowTarget += OnChangeFollowTarget;
        }

        protected override void OnDestroy()
        {
            if(m_FollowManager != null)
            {
                m_FollowManager.EnterFollowMode -= OnEnterFollowMode;
                m_FollowManager.ExitFollowMode -= OnExitFollowMode;
                m_FollowManager.ChangeFollowTarget -= OnChangeFollowTarget;
            }
        }

        void OnEnable()
        {
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            m_VoiceManager.VoiceStatusChanged += OnVoiceStatusChanged;
            m_VoiceManager.VoiceParticipantAdded += OnVoiceParticipantAdded;
            m_VoiceManager.VoiceParticipantRemoved += OnVoiceParticipantRemoved;
            m_VoiceManager.VoiceParticipantUpdated += OnVoiceParticipantUpdated;

            OnVoiceStatusChanged(m_VoiceManager.CurrentVoiceStatus);
        }

        void OnExitFollowMode()
        {
            m_CollaboratorsDataPanel.OnExitFollowMode();
        }

        void OnEnterFollowMode(IParticipant participant, bool isPresentation)
        {
            m_CollaboratorsDataPanel.UpdateFollowModeCollaboratorDataUI(participant.Id, isPresentation);
        }

        void OnChangeFollowTarget(IParticipant participant)
        {
            m_CollaboratorsDataPanel.UpdateFollowModeCollaboratorDataUI(participant.Id);
        }

        void OnDataUIExistFollowMode(IParticipant participant)
        {
            m_FollowManager.StopFollowMode();
        }

        void OnDataUIEnterFollowMode(IParticipant participant)
        {
            if (m_FollowManager.IsFollowing)
            {
                m_FollowManager.UpdateFollowTarget(participant);
            }
            else
            {
                m_FollowManager.StartFollowMode(participant);
            }
        }

        void OnDisable()
        {
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            m_VoiceManager.VoiceStatusChanged -= OnVoiceStatusChanged;
            m_VoiceManager.VoiceParticipantAdded -= OnVoiceParticipantAdded;
            m_VoiceManager.VoiceParticipantRemoved -= OnVoiceParticipantRemoved;
            m_VoiceManager.VoiceParticipantUpdated -= OnVoiceParticipantUpdated;
        }

        protected virtual void OnRoomJoined(Room room)
        {
            CurrentRoom = room;

            var cachedRoom = m_VoiceAvatarsBadgesContainer.Room ?? new RoomCached(null);
            cachedRoom.Room = room; // Assign new room
            m_VoiceAvatarsBadgesContainer.BindRoom(cachedRoom);
            m_VoiceAvatarsBadgesContainer.BindVoiceManager(m_VoiceManager);

            RefreshVisualTree();

            CurrentRoom.ParticipantAdded += OnParticipantAdded;
            CurrentRoom.ParticipantRemoved += OnParticipantRemoved;
        }

        protected void OnRoomLeft(Room room)
        {
            if(room != null)
            {
                foreach (var participant in room.ConnectedParticipants)
                {
                    if (participant.IsSelf)
                        continue;
                    OnParticipantRemoved(participant);
                }

                room.ParticipantAdded -= OnParticipantAdded;
                room.ParticipantRemoved -= OnParticipantRemoved;
            }

            m_CollaboratorsDataPanel.ClearParticipants();

            CurrentRoom = null;
            m_VoiceAvatarsBadgesContainer.UnBindVoiceManager(m_VoiceManager);

            RefreshVisualTree();
        }

        protected void OnParticipantRemoved(IParticipant participant)
        {
            m_CollaboratorsDataPanel.UnSubscribeUIFollowModeEvent(participant.Id);
            m_CollaboratorsDataPanel.DataUIEnterFollowMode -= OnDataUIEnterFollowMode;
            m_CollaboratorsDataPanel.DataUIExitFollowMode -= OnDataUIExistFollowMode;
            m_CollaboratorsDataPanel.RemoveVoiceToParticipant(participant.Id);
            m_CollaboratorsDataPanel.RemoveParticipant(participant.Id);

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
                m_CollaboratorsDataPanel.AddVoiceToParticipant(voice);
            }

            m_CollaboratorsDataPanel.SubscribeUIFollowModeEvent(participant.Id);
            m_CollaboratorsDataPanel.DataUIEnterFollowMode += OnDataUIEnterFollowMode;
            m_CollaboratorsDataPanel.DataUIExitFollowMode += OnDataUIExistFollowMode;

            RefreshVisualTree();
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            // Create Panel
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

                RefreshToolState();
            }
        }

        void RefreshToolState()
        {
            if (CurrentRoom != null && CurrentRoom.ConnectedParticipants.Where(p => !p.IsSelf).ToList().Count > 0)
            {
                SetToolState(ToolState.Active);
            }
            else
            {
                SetToolState(ToolState.Hidden);
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

        void OnVoiceStatusChanged(VoiceStatus status)
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
                        m_CollaboratorsDataPanel.RemoveVoiceToParticipant(participant.Id);
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
                            m_CollaboratorsDataPanel.AddVoiceToParticipant(voice);
                        }
                    }

                    break;
            }

            RefreshVisualTree();
        }

        void OnVoiceParticipantAdded(IEnumerable<IVoiceParticipant> participants)
        {
            m_CollaboratorsDataPanel.VoiceParticipantAdded(participants);
        }

        void OnVoiceParticipantRemoved(IEnumerable<IVoiceParticipant> participants)
        {
            m_CollaboratorsDataPanel.VoiceParticipantRemoved(participants);
        }

        void OnVoiceParticipantUpdated(IEnumerable<IVoiceParticipant> participants)
        {
            m_CollaboratorsDataPanel.VoiceParticipantUpdated(participants);
        }
    }
}
