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
        static readonly string k_CollaboratorDataBadgeUssClassName = "collaborator-data-badge";
        static readonly string k_CollaboratorDataHeaderUssClassName = "collaborator-data-header";
        static readonly string k_CollaboratorDataMicrophoneUssClassName = "collaborator-data-microphone";
        
        public readonly IParticipant Participant;

        public CollaboratorDataUI(IParticipant participant)
        {
            Participant = participant;
        }
        
        public VoiceParticipant? VoiceParticipant { get; set; }
        public bool IsVoiceParticipant => VoiceParticipant != null;


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
            
            var icon = new Icon();
            if (VoiceParticipant is { IsMutedForAll: false })
            {
                icon.name = "Microphone";
                icon.iconName = "microphone";
            }
            else
            {
                icon.name = "MicrophoneSlash";
                icon.iconName = "microphone-slash";

                if (!IsVoiceParticipant)
                {
                    icon.SetEnabled(false);
                    icon.tooltip = "@Presence:Vivox_Unsupported";
                }
            }
            
            icon.AddToClassList(k_CollaboratorDataMicrophoneUssClassName);
            element.Add(icon);
            return element;
        }
    }
}
