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
    public class PresentationToolUIController : ToolUIController
    {
        [SerializeField]
        ColorPalette m_AvatarColorPalette;
        
        PresentationManager m_PresentationManager;
        ActionButton m_StartPresentationButton;
        ActionButton m_StopPresentationButton;
        ActionButton m_JoinPresentationButton;
        ActionButton m_ExitPresentationButton;
        VisualElement m_PresenterVisualElement;
        VisualElement m_AudienceVisualElement;
        Text m_PresenterText;
        Text m_AudienceText;
        Text m_Empty;
        Divider m_Divider;
        
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        IPresenceRoomsManager m_PresenceRoomsManager;
        PresentationStatus m_PresentationStatus;
        readonly List<IParticipant> m_Participants = new ();
        static readonly string k_CollaboratorDataUssClassName = "container__collaborator-data";

        [Inject]
        void SetUp(PresentationManager presentationManager, IPresenceStreamingRoom presenceStreamingRoom, IPresenceRoomsManager presenceRoomsManager)
        {
            m_PresentationManager = presentationManager;
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_PresenceRoomsManager = presenceRoomsManager;
        }

        protected override void Awake()
        {
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            m_PresentationManager.PresentationStatusChanged += OnPresentationEvent;
        }

        protected override void OnDestroy()
        {
            if(m_PresenceStreamingRoom != null)
            {
                m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
                m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            }
            if(m_PresentationManager != null)
            {
                m_PresentationManager.PresentationStatusChanged -= OnPresentationEvent;
            }
        }

        void OnRoomLeft(Room room )
        {
            if(room != null)
            {
                room.ParticipantAdded -= OnParticipantAdded;
                room.ParticipantRemoved -= OnParticipantRemoved;
            }
            
            UpdateUI();
        }

        void OnRoomJoined(Room room)
        {
            room.ParticipantAdded += OnParticipantAdded;
            room.ParticipantRemoved += OnParticipantRemoved;
            
            foreach (var participant in room.ConnectedParticipants)
            {
                if (!m_Participants.Contains(participant))
                {
                    m_Participants.Add(participant);
                }
            }
            
            UpdateUI();
        }

        void OnPresentationEvent(PresentationStatus presentationStatus)
        {
            m_PresentationStatus = presentationStatus;
            UpdateUI();
        }

        void OnParticipantRemoved(IParticipant participant)
        {
            m_Participants.Remove(participant);
            UpdateUI();
        }

        void OnParticipantAdded(IParticipant participant)
        {
            if (!m_Participants.Contains(participant))
            {
                m_Participants.Add(participant);
            }
            UpdateUI();
        }
        
        void HideAllButtons()
        {
            Utils.SetVisible(m_StopPresentationButton,false);
            Utils.SetVisible(m_JoinPresentationButton,false);
            Utils.SetVisible(m_ExitPresentationButton,false);
            Utils.SetVisible(m_StartPresentationButton, false);
        }

        void SetVisibleAllParticipants(bool show)
        {
            Utils.SetVisible(m_PresenterVisualElement, show);
            Utils.SetVisible(m_AudienceVisualElement, show);
        }

        void ShowDisplayInfo(bool show)
        {
            Utils.SetVisible(m_PresenterText, show);
            Utils.SetVisible(m_AudienceText, show);
            Utils.SetVisible(m_Divider, show);
        }
        
        void UpdatePresenterUI()
        {
            if (m_PresentationStatus != PresentationStatus.NoPresentation)
            {
                m_PresenterVisualElement.Clear();
                var presenter = m_Participants.FirstOrDefault(participant => participant.Id == 
                    m_PresentationManager.PresenterId);
                if (presenter != null)
                {
                    PresentationCollaboratorDataUI collaboratorDataUI = new PresentationCollaboratorDataUI(presenter)
                        { AvatarColorPalette = m_AvatarColorPalette };
                    var collaboratorVisualElement = collaboratorDataUI.CreateVisualTree();
                    collaboratorVisualElement.AddToClassList(k_CollaboratorDataUssClassName);
                    m_PresenterVisualElement.Add(collaboratorVisualElement);
                }
            }
        }

        void UpdateAudienceUI()
        {
            m_AudienceVisualElement.Clear();
            Utils.SetVisible(m_Empty, false);
            
            foreach (var participant in m_Participants.Where(participant => m_PresentationManager.Audience.Contains(participant.Id))) { 
                
                PresentationCollaboratorDataUI collaboratorDataUI = new PresentationCollaboratorDataUI(participant)
                    { AvatarColorPalette = m_AvatarColorPalette };
                
                var collaboratorDataVisualElement = collaboratorDataUI.CreateVisualTree();
                collaboratorDataVisualElement.AddToClassList(k_CollaboratorDataUssClassName);
                m_AudienceVisualElement.Add(collaboratorDataVisualElement);
            }
            
            if(!m_AudienceVisualElement.Children().Any() && m_PresentationStatus is not (PresentationStatus.NoPresentation 
                   or PresentationStatus.NoRoom))
            {
                Utils.SetVisible(m_Empty, true);
            }
        }
        
        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);
            m_StopPresentationButton = root.Q<ActionButton>("StopPresentation");
            m_StartPresentationButton = root.Q<ActionButton>("StartPresentation");
            m_JoinPresentationButton = root.Q<ActionButton>("JoinPresentation");
            m_ExitPresentationButton = root.Q<ActionButton>("ExitPresentation");
            m_PresenterVisualElement = root.Q<VisualElement>("Presenter");
            m_AudienceVisualElement = root.Q<VisualElement>("Audience");
            
            m_PresenterText = root.Q<Text>("PresenterText");
            m_AudienceText = root.Q<Text>("AudienceText");
            m_Divider = root.Q<Divider>("Divider");
            m_Empty = root.Q<Text>("Empty");

            Utils.SetVisible(m_Empty, false);
            HideAllButtons();
            ShowDisplayInfo(false);
            
            m_StartPresentationButton.clicked += () => m_PresentationManager.StartPresentation();
            m_StopPresentationButton.clicked += () => m_PresentationManager.StopPresentation();
            m_JoinPresentationButton.clicked += () => m_PresentationManager.JoinPresentation();
            m_ExitPresentationButton.clicked += () => m_PresentationManager.ExitPresentation();
            
            return root;
        }
        
        void CheckPermissions()
        {
            m_StartPresentationButton.SetEnabled(m_PresenceRoomsManager.CheckPermissions(PresencePermission.StartPresentation));
        }

        void UpdateUI()
        {
            HideAllButtons();
            SetVisibleAllParticipants(true);
            UpdatePresenterUI();
            UpdateAudienceUI();
            ShowDisplayInfo(true);

            switch (m_PresentationStatus)
            {
                case PresentationStatus.Presenting :
                    Utils.SetVisible(m_StopPresentationButton,true);
                    break;
                case PresentationStatus.Attending :
                    Utils.SetVisible(m_ExitPresentationButton,true);
                    break;
                case PresentationStatus.NotAttending :
                    Utils.SetVisible(m_JoinPresentationButton, true);
                    break;
                case PresentationStatus.NoPresentation :
                    Utils.SetVisible(m_StartPresentationButton, true);
                    SetVisibleAllParticipants(false);
                    ShowDisplayInfo(false);
                    break;
                case PresentationStatus.NoRoom:
                    break;
            }

            CheckPermissions();
            
            SetToolState(m_PresentationStatus != PresentationStatus.NoRoom ? ToolState.Active : ToolState.Inactive);
        }
    }
}
