using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class MuteToolUIController : ToolUIController
    {
        [SerializeField]
        string m_VivoxUnsupportedString = "@Presence:Vivox_Unsupported";
        [SerializeField]
        string m_VivoxMicrophoneString = "@Presence:Mic";
        [SerializeField]
        InputActionReference m_MuteActionReference;

        [Header("UIToolkit")]
        [SerializeField]
        string m_ButtonStyleClass;

        [SerializeField]
        StyleSheet[] m_AdditionalStyleSheets;

        VoiceLevelMicrophoneButton m_MuteButton;
        IVoiceManager m_VoiceManager;
        InputAction m_MuteAction;
        IPresenceStreamingRoom m_PresenceStreamingRoom;

        [Inject]
        void Setup(IVoiceManager voiceManager, IPresenceStreamingRoom presenceStreamingRoom)
        {
            m_VoiceManager = voiceManager;
            m_PresenceStreamingRoom = presenceStreamingRoom;
        }

        protected override void Awake()
        {
            if (m_MuteActionReference != null)
            {
                m_MuteAction = m_MuteActionReference.action;
            }
        }

        void OnEnable()
        {
            m_VoiceManager.MuteStatusUpdated += UpdateMuteButtonIcon;
            m_VoiceManager.VoiceParticipantUpdated += VoiceParticipantUpdated;
            m_VoiceManager.VoiceStatusChanged += OnVoiceStatusChanged;
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;

            if (m_MuteAction != null)
            {
                m_MuteAction.Enable();
                m_MuteAction.performed += OnMuteAction;
            }
        }

        void OnRoomJoined(Room room)
        {
            m_VoiceManager.CheckPermissions();
        }

        void OnDisable()
        {
            m_VoiceManager.MuteStatusUpdated -= UpdateMuteButtonIcon;
            m_VoiceManager.VoiceParticipantUpdated -= VoiceParticipantUpdated;

            if (m_MuteAction != null)
            {
                m_MuteAction.Disable();
                m_MuteAction.performed -= OnMuteAction;
            }
        }

        public override VisualElement GetButtonContent()
        {
            m_MuteButton = new VoiceLevelMicrophoneButton(m_VoiceManager.Muted, OnMute);
            if (m_VoiceManager.CurrentVoiceStatus == VoiceStatus.Unsupported)
            {
                m_MuteButton.tooltip = m_VivoxUnsupportedString;
                m_MuteButton.parent?.SetEnabled(false);
            }

            m_MuteButton.AddToClassList(m_ButtonStyleClass);
            m_MuteButton.size = Size.L;
            if (m_AdditionalStyleSheets != null)
            {
                foreach (var styleSheet in m_AdditionalStyleSheets)
                {
                    m_MuteButton.styleSheets.Add(styleSheet);
                }
            }

            return m_MuteButton;
        }

        void UpdateMuteButtonIcon(bool isMuted)
        {
            m_MuteButton?.SetMuted(isMuted);
        }

        void OnVoiceStatusChanged(VoiceStatus status)
        {
            switch (status)
            {
                case VoiceStatus.Unsupported:
                    m_MuteButton?.SetVoiceLevel(0);
                    m_VoiceManager.Muted = true;
                    m_MuteButton?.SetMuted(true);
                    m_MuteButton?.parent?.SetEnabled(false);
                    if (m_MuteButton != null) m_MuteButton.tooltip = m_VivoxUnsupportedString;
                    break;
                case VoiceStatus.NotConnected:
                    m_MuteButton?.SetVoiceLevel(0);
                    m_MuteButton?.SetMuted(m_VoiceManager.Muted);
                    m_MuteButton?.parent?.SetEnabled(true);
                    if (m_MuteButton != null) m_MuteButton.tooltip = m_VivoxMicrophoneString;
                    break;
                case VoiceStatus.Connected:
                    m_MuteButton?.SetMuted(m_VoiceManager.Muted);
                    m_MuteButton?.parent?.SetEnabled(true);
                    if (m_MuteButton != null) m_MuteButton.tooltip = m_VivoxMicrophoneString;
                    break;
            }
        }

        void VoiceParticipantUpdated(IEnumerable<IVoiceParticipant> participants)
        {
            if (m_VoiceManager.Muted)
                return;
            var selfUpdate = participants.FirstOrDefault(p => p.IsSelf);
            if (selfUpdate == null)
                return;
            m_MuteButton?.SetVoiceLevel((float)selfUpdate.AudioIntensity);
        }

        void OnMute()
        {
            m_VoiceManager.Muted = !m_VoiceManager.Muted;
        }

        void OnMuteAction(InputAction.CallbackContext callback)
        {
            OnMute();
        }
    }
}
