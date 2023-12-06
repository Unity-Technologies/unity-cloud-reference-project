using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
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
        InputActionReference m_MuteActionReference;

        [Header("UIToolkit")]
        [SerializeField]
        string m_ButtonStyleClass;

        [SerializeField]
        StyleSheet[] m_AdditionalStyleSheets;

        VoiceLevelMicrophoneButton m_MuteButton;
        IVoiceManager m_VoiceManager;
        IPresenceStreamingRoom m_PresenceStreamingRoom;
        InputAction m_MuteAction;

        [Inject]
        void Setup(IPresenceStreamingRoom presenceStreamingRoom, IVoiceManager voiceManager)
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

            if (m_MuteAction != null)
            {
                m_MuteAction.Enable();
                m_MuteAction.performed += OnMuteAction;
            }
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
                m_MuteButton.SetEnabled(false);
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
