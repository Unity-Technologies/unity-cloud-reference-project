using System;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class Avatar : MonoBehaviour
    {
        [SerializeField]
        AvatarModel m_Model;

        [SerializeField]
        AvatarTag m_Tag;

        INetcodeParticipant m_Participant;
        PresenceStreamingRoom m_PresenceStreamingRoom;
        
        Camera m_StreamingCamera;
        Transform m_Transform;

        [Inject]
        void Setup(PresenceStreamingRoom presenceStreamingRoom, Camera streamingCamera)
        {
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_StreamingCamera = streamingCamera;
        }

        void Update()
        {
            if (m_Participant == null)
                return;
            
            if (m_Participant.IsOwner)
            {
                var cameraTransform = m_StreamingCamera.transform;
                transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
            }

            if (m_Participant != null)
            {
                if (m_Participant.IsOwner)
                {
                    var t = transform;
                    m_Participant.SetParticipantTransform(t.position, t.rotation);
                }
                else
                {
                    transform.SetPositionAndRotation(m_Transform.position, m_Transform.rotation);
                }
            }
        }

        void OnDestroy()
        {
            if (m_Participant != null)
            {
                m_Participant.ParticipantIdChanged -= OnParticipantIDChanged;
            }
        }

        public void InitParticipant(INetcodeParticipant participant)
        {
            m_Participant = participant;
            m_Transform = participant.Transform;
            m_Participant.ParticipantIdChanged += OnParticipantIDChanged;

            RefreshParticipant(m_Participant);
        }

        void OnParticipantIDChanged(INetcodeParticipant participant, string id)
        {
            RefreshParticipant(participant);
        }

        void RefreshParticipant(INetcodeParticipant participant)
        {
            var roomParticipant = m_PresenceStreamingRoom.GetParticipantFromID(participant.ParticipantId);
            var participantName = participant.IsOwner ? "Owner" : roomParticipant.Name;
            var color = AvatarUtils.GetColor(roomParticipant.ColorIndex);
            var visible = !participant.IsOwner;
            
            gameObject.name = $"Avatar ({participantName})";
            
            m_Model.SetVisible(visible);
            m_Model.SetColor(color);
            
            m_Tag.SetVisible(visible);
            m_Tag.SetName(participantName);
            m_Tag.SetColor(color);
        }
    }
}
