using System;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
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

        [SerializeField]
        ColorPalette m_AvatarColorPalette;

        [Header("Localization")]
        [SerializeField]
        string m_LoadingString = "@Presence:Loading_Name";

        INetcodeParticipant m_Participant;
        PresenceStreamingRoom m_PresenceStreamingRoom;
        Camera m_StreamingCamera;
        Transform m_Transform;
        String m_CurrentAvatarName;

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
                // Set up the owner Avatar and the INetcodeParticipant to follow the camera
                var cameraTransform = m_StreamingCamera.transform;
                transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
                m_Participant.SetParticipantTransform(cameraTransform.position, cameraTransform.rotation);
            }
            else
            {
                transform.SetPositionAndRotation(m_Transform.position, m_Transform.rotation);
            }

            if (m_CurrentAvatarName.Equals(m_LoadingString)
                && m_PresenceStreamingRoom.GetParticipantFromID(m_Participant.ParticipantId) != null)
            {
                RefreshParticipant(m_Participant);
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
            m_CurrentAvatarName = m_LoadingString;
            RefreshParticipant(m_Participant);
        }

        void OnParticipantIDChanged(INetcodeParticipant participant, ParticipantId id)
        {
            RefreshParticipant(participant);
        }

        void RefreshParticipant(INetcodeParticipant participant)
        {
            var roomParticipant = m_PresenceStreamingRoom.GetParticipantFromID(participant.ParticipantId);
            var color = Color.grey;

            if (roomParticipant != null)
            {
                m_CurrentAvatarName = participant.IsOwner ? "Owner" : roomParticipant.Name;
                color = m_AvatarColorPalette.GetColor(roomParticipant.ColorIndex);
            }

            var visible = !participant.IsOwner;

            gameObject.name = $"Avatar ({m_CurrentAvatarName})";

            m_Model.SetVisible(visible);
            m_Model.SetColor(color);

            m_Tag.SetVisible(visible);
            m_Tag.SetName(m_CurrentAvatarName);
            m_Tag.SetColor(color);
        }
    }
}
