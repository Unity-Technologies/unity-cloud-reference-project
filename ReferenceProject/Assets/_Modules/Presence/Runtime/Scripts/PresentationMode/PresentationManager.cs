using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public enum PresentationStatus
    {
        NoRoom,
        NoPresentation,
        NotAttending,
        Attending,
        Presenting,
    }

    public class PresentationManager : MonoBehaviour
    {
        IPresentationService m_PresentationService;
        ISessionProvider m_SessionProvider;
        IAppMessaging m_AppMessaging;
        IFollowManager m_FollowManager;
        PresentationStatus m_PresentationStatus;
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        IToolUIManager m_ToolUIManager;

        ParticipantId m_CurrentParticipantId;
        Room m_CurrentRoom;
        ParticipantId m_PresenterId;
        public ParticipantId PresenterId => m_PresenterId;
        List<ParticipantId> m_Audience = new List<ParticipantId>();

        public List<ParticipantId> Audience => m_Audience;

        public event Action<ParticipantId> PresentationStarted;
        public event Action<ParticipantId> PresentationStopped;
        public event Action<ParticipantId> PresentationJoined;
        public event Action<ParticipantId> PresentationLeft;
        public event Action OnSessionChangedEvent;
        public event Action<PresentationStatus> PresentationStatusChanged;

        [Inject]
        void SetUp(IPresentationService presentationService, ISessionProvider sessionProvider,
            IFollowManager followManager, IPresenceStreamingRoom presenceStreamingRoom, IAppMessaging appMessaging, IToolUIManager toolUIManager)
        {
            m_PresentationService = presentationService;
            m_SessionProvider = sessionProvider;
            m_FollowManager = followManager;
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_AppMessaging = appMessaging;
            m_ToolUIManager = toolUIManager;
        }

        void Awake()
        {
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            m_FollowManager.ExitFollowMode += OnExitFollowMode;
            m_PresentationService.PresentationEvent += OnPresentationEvent;
            m_SessionProvider.SessionChanged += OnSessionChanged;
            m_CurrentParticipantId = m_SessionProvider.Session?.CurrentParticipantId ?? new ParticipantId(string.Empty);
        }

        void OnDestroy()
        {
            m_PresenceStreamingRoom.RoomJoined -= OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft -= OnRoomLeft;
            m_FollowManager.ExitFollowMode -= OnExitFollowMode;

            m_SessionProvider.SessionChanged -= OnSessionChanged;
            m_SessionProvider = null;

            m_PresentationService.PresentationEvent -= OnPresentationEvent;
            m_PresentationService = null;
        }

        void OnSessionChanged(ISession session)
        {
            if (session != null)
            {
                m_CurrentParticipantId = m_SessionProvider.Session?.CurrentParticipantId ??
                    new ParticipantId(string.Empty);
                OnSessionChangedEvent?.Invoke();
            }
        }

        void OnPresentationEvent(PresentationEventDetails presentationEventDetails)
        {
            var currentParticipantId = m_SessionProvider.Session?.CurrentParticipantId ??
                new ParticipantId(string.Empty);

            m_PresenterId = presentationEventDetails.PresenterParticipantId;
            m_Audience = presentationEventDetails.AttendeeParticipantIds.ToList();

            if (presentationEventDetails.IsPresentationOngoing)
            {
                if (m_PresentationStatus == PresentationStatus.NoPresentation && m_PresenterId != currentParticipantId)
                {
                    m_AppMessaging.ShowDialog("@Presence:Presentation",
                        "@Presence:PresentationStartedMessage",
                        "@Presence:Close",
                        null,
                        "@Presence:Join",
                        JoinPresentation,
                        new object[] { GetIdParticipant(m_PresenterId).Name });
                }

                if (presentationEventDetails.PresenterParticipantId == currentParticipantId)
                {
                    m_PresentationStatus = PresentationStatus.Presenting;
                }
                else if (presentationEventDetails.AttendeeParticipantIds.Contains(currentParticipantId))
                {
                    m_PresentationStatus = PresentationStatus.Attending;
                }
                else
                {
                    m_PresentationStatus = PresentationStatus.NotAttending;
                }
            }
            else
            {
                if (m_PresentationStatus == PresentationStatus.Attending)
                {
                    m_FollowManager.StopFollowMode();
                }

                m_PresentationStatus = PresentationStatus.NoPresentation;
            }

            PresentationStatusChanged?.Invoke(m_PresentationStatus);
        }

        void OnRoomJoined(Room room)
        {
            m_CurrentRoom = room;
        }

        void OnRoomLeft(Room room)
        {
            if (m_PresenterId == m_CurrentParticipantId)
            {
                StopPresentation();
            }

            m_CurrentRoom = null;
        }

        void OnExitFollowMode()
        {
            if (m_CurrentParticipantId != m_PresenterId)
            {
                m_PresentationService.LeavePresentationAsync();
                PresentationLeft?.Invoke(m_CurrentParticipantId);
            }
        }

        public void StartPresentation()
        {
            if (m_FollowManager.IsFollowing)
            {
                m_FollowManager.StopFollowMode();
            }

            m_PresentationService.StartPresentationAsync();
            PresentationStarted?.Invoke(m_CurrentParticipantId);
        }

        public void StopPresentation()
        {
            m_PresentationService.StopPresentationAsync();
            PresentationStopped?.Invoke(m_CurrentParticipantId);
        }

        public void JoinPresentation()
        {
            m_ToolUIManager.CloseAllTools();
            if (m_FollowManager.IsFollowing)
            {
                m_FollowManager.StopFollowMode();
            }

            m_PresentationService.JoinPresentationAsync();
            PresentationJoined?.Invoke(m_CurrentParticipantId);
            foreach (var participant in m_CurrentRoom.ConnectedParticipants)
            {
                if (m_PresenterId == participant.Id)
                {
                    m_FollowManager.StartFollowMode(participant, true);
                }
            }
        }

        public void ExitPresentation()
        {
            m_PresentationService.LeavePresentationAsync();
            PresentationLeft?.Invoke(m_CurrentParticipantId);
            m_FollowManager.StopFollowMode();
        }

        IParticipant GetIdParticipant(ParticipantId participantId)
        {
            return m_CurrentRoom.ConnectedParticipants.FirstOrDefault(participant => participantId == participant.Id);
        }
    }
}
