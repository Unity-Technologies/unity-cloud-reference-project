using System;
using Unity.Cloud.Presence;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class CollaboratorDataUI
    {
        public ColorPalette AvatarColorPalette { get; set; }
        public StyleSheet VoiceLevelStyleSheet { get; set; }
        static readonly string k_CollaboratorDataBadgeUssClassName = "avatar_badge__collaborator-data";
        static readonly string k_CollaboratorDataHeaderUssClassName = "heading__collaborator-data";
        static readonly string k_CollaboratorDataMicrophoneUssClassName = "button__collaborator-data-microphone";
        static readonly string k_FollowButtonUssClassName = "button__collaborator-follow";
        bool m_FollowButtonSelected = false;
        
        public readonly IParticipant Participant;
        public event Action<IParticipant> UIEnterFollowMode;
        public event Action<IParticipant> UIExitFollowMode;

        public CollaboratorDataUI(IParticipant participant)
        {
            Participant = participant;
        }

        IVoiceParticipant m_VoiceParticipant;
        public IVoiceParticipant VoiceParticipant
        {
            get { return m_VoiceParticipant; }
            set
            {
                m_VoiceParticipant = value;
                VoiceLevelIndicator?.SetVoiceLevel((float)(m_VoiceParticipant?.AudioIntensity ?? 0f));
            }
        }

        public bool IsVoiceParticipant => VoiceParticipant != null;
        public ActionButton FollowButton { get; set; }
        
        public VoiceLevelMicrophoneButton VoiceLevelIndicator { get; set; }
        
        public VisualElement CreateVisualTree()
        {
            var element = new VisualElement();

            var avatar = new AvatarBadge
            {
                tooltip = Participant.Name,
                backgroundColor = AvatarColorPalette.GetColor(Participant.ColorIndex),
                size = Size.M,
                outlineColor = Color.clear
            };
            avatar.Initials.text = Utils.GetInitials(Participant.Name);
            avatar.AddToClassList(k_CollaboratorDataBadgeUssClassName);
            element.Add(avatar);

            var header = new Heading(Participant.Name);
            header.size = HeadingSize.S;
            header.AddToClassList(k_CollaboratorDataHeaderUssClassName);
            element.Add(header);
            
            var voiceLevelIndicator = new VoiceLevelMicrophoneButton(false);
            voiceLevelIndicator.styleSheets.Add(VoiceLevelStyleSheet);
            voiceLevelIndicator.AddToClassList(k_CollaboratorDataMicrophoneUssClassName);
            voiceLevelIndicator.SetEnabled(false);
            voiceLevelIndicator.tooltip = "@Presence:VoiceLevel";

            VoiceLevelIndicator = voiceLevelIndicator;
            element.Add(voiceLevelIndicator);
            
            var followButton = new ActionButton();
            followButton.label = m_FollowButtonSelected ? "@Presence:Unfollow" : "@Presence:Follow";
            followButton.selected = m_FollowButtonSelected;
            followButton.focusable = false;
            followButton.clicked += () => OnClickFollow(Participant);
            followButton.AddToClassList(k_FollowButtonUssClassName);

            FollowButton = followButton;
            element.Add(followButton);

            return element;
        }
        
        void OnClickFollow(IParticipant participant)
        {
            if (!FollowButton.selected)
            {
                UIEnterFollowMode?.Invoke(participant);
                FollowButton.label = "@Presence:Unfollow";
                m_FollowButtonSelected = true;
            }
            else
            {
                UIExitFollowMode?.Invoke(participant);
                FollowButton.label = "@Presence:Follow";
                m_FollowButtonSelected = false;
            }
        }

        public void FollowModeSelected(ParticipantId id)
        {
            FollowButton.selected = true;
            m_FollowButtonSelected = true;
        }

        public void UpdateButton(bool isSelected)
        {
            m_FollowButtonSelected = isSelected;
            FollowButton.label = m_FollowButtonSelected ? "@Presence:Unfollow" : "@Presence:Follow";
            FollowButton.selected = m_FollowButtonSelected;
        }
    }
}
