using System;
using System.Linq;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class VoiceLevelMicrophoneButton : IconButton
    {
        static readonly string k_MicrophoneContainerUssClassName = "container__microphone";
        static readonly string k_MicrophoneVoiceLevelUssClassName = "image__microphone-voice-level";
        static readonly string k_MicrophoneIconName = "microphone2x";
        static readonly string k_MicrophoneSlashIconName = "microphone-slash2x";
        readonly VoiceLevelIndicator m_VoiceLevelIndicator;
        
        public VoiceLevelMicrophoneButton(bool muted, Action clickEvent = null)
            : base("", clickEvent)
        {
            var microphoneIcon = this.Q<Icon>(iconUssClassName);

            var microphoneContainer = new VisualElement { name = k_MicrophoneContainerUssClassName };
            microphoneContainer.AddToClassList(k_MicrophoneContainerUssClassName);

            m_VoiceLevelIndicator = new VoiceLevelIndicator { name = k_MicrophoneVoiceLevelUssClassName };
            m_VoiceLevelIndicator.AddToClassList(k_MicrophoneVoiceLevelUssClassName);

            var localScale = Vector3.one;
            localScale.y = 0;
            m_VoiceLevelIndicator.style.scale = new Scale(localScale);

            microphoneContainer.hierarchy.Add(m_VoiceLevelIndicator);
            microphoneIcon.parent?.hierarchy.Add(microphoneContainer);
            microphoneContainer.hierarchy.Add(microphoneIcon);

            SetMuted(muted);
        }

        public void SetMuted(bool muted)
        {
            icon = muted ? k_MicrophoneSlashIconName : k_MicrophoneIconName;
            Utils.SetVisible(m_VoiceLevelIndicator, !muted);
        }

        public void SetVoiceLevel(float voiceLevel)
        {
            m_VoiceLevelIndicator.SetVoiceLevel(voiceLevel);
        }
    }
}
