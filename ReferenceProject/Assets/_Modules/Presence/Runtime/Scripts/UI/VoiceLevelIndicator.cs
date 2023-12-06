using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class VoiceLevelIndicator : VisualElement
    {
        static readonly float k_MicLevelFallRate = 2f;
        
        IVisualElementScheduledItem m_Update;
        float m_SmoothedMicLevel;
        float m_SmoothedMicVelocity;
        float TargetVoiceLevel { get; set; }
        
        public void SetVoiceLevel(float voiceLevel)
        {
            if (Utils.IsVisible(this))
            {
                TargetVoiceLevel = voiceLevel;
                m_Update?.Pause();
                m_Update = null;
                m_Update = schedule.Execute(CalculateSmoothedVolumeLevel).Every(8L);
            }
        }

        void CalculateSmoothedVolumeLevel()
        {
            // Mic level visual indicator jumps up to current level, but falls down at smooth fixed rate
            if (m_SmoothedMicLevel > TargetVoiceLevel)
            {
                var unscaledDeltaTime = Time.unscaledDeltaTime;
                m_SmoothedMicLevel = Mathf.Clamp01(m_SmoothedMicLevel + m_SmoothedMicVelocity * unscaledDeltaTime);
                m_SmoothedMicVelocity -= k_MicLevelFallRate * unscaledDeltaTime;
            }
            else
            {
                m_SmoothedMicLevel = TargetVoiceLevel;
                m_SmoothedMicVelocity = 0f;
            }

            // Scale mic indicator
            var localScale = Vector3.one;
            localScale.y = m_SmoothedMicLevel;
            style.scale = new Scale(localScale);
            MarkDirtyRepaint();
        }
    }
}
