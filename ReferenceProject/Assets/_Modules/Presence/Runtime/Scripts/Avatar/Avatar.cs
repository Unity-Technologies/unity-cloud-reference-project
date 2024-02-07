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
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        ICameraProvider m_CameraProvider;
        Transform m_Transform;
        String m_CurrentAvatarName;

        public static readonly string k_HideKey = "Hide";

        [Inject]
        void Setup(IPresenceStreamingRoom presenceStreamingRoom, ICameraProvider cameraProvider)
        {
            m_PresenceStreamingRoom = presenceStreamingRoom;
            m_CameraProvider = cameraProvider;
        }

        void Update()
        {
            if (m_Participant == null)
                return;

            if (m_Participant.IsOwner)
            {
                // Set up the owner Avatar and the INetcodeParticipant to follow the camera
                if(m_CameraProvider.Camera == null)
                    return;

                var cameraTransform = m_CameraProvider.Camera.transform;
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
                if (m_Participant.GenericDataHandler != null)
                {
                    m_Participant.GenericDataHandler.GenericDataUpdated -= OnGenericDataUpdated;
                }
            }
        }

        public void InitParticipant(INetcodeParticipant participant)
        {
            m_Participant = participant;
            m_Participant.GenericDataHandler.GenericDataUpdated += OnGenericDataUpdated;
            m_Transform = participant.Transform;
            m_Participant.ParticipantIdChanged += OnParticipantIDChanged;
            m_CurrentAvatarName = m_LoadingString;
            RefreshParticipant(m_Participant);
        }

        void OnGenericDataUpdated(GenericDataUpdate data)
        {
            if (data.Key == k_HideKey)
            {
                RefreshParticipant(m_Participant);
            }
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

            gameObject.name = $"Avatar ({m_CurrentAvatarName})";

            var isHidden = participant.IsOwner;

            if (participant.GenericDataHandler.GenericData.TryGetValue(k_HideKey, out var value))
            {
                isHidden = isHidden || GenericDataExtensions.DeserializeFromByteArray<bool>(value);
            }

            m_Model.SetVisible(!isHidden);
            m_Model.SetColor(color);

            m_Tag.SetVisible(!isHidden);
            m_Tag.SetName(m_CurrentAvatarName);
            m_Tag.SetInitials(m_CurrentAvatarName.Equals(m_LoadingString) ? "-" : Utils.GetInitials(m_CurrentAvatarName));
            m_Tag.SetColor(color);
        }
    }
}
