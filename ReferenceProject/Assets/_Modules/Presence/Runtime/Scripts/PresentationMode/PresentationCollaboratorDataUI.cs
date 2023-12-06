using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class PresentationCollaboratorDataUI
    {
        public ColorPalette AvatarColorPalette { get; set; }
        static readonly string k_CollaboratorDataBadgeUssClassName = "avatar_badge__presentation-collaborator-data";
        static readonly string k_CollaboratorDataHeaderUssClassName = "heading__presentation-collaborator-data";
        readonly IParticipant m_Participant;
        
        public PresentationCollaboratorDataUI(IParticipant participant)
        {
            m_Participant = participant;
        }
        
        public VisualElement CreateVisualTree()
        {
            var root = new VisualElement();

            var avatar = new AvatarBadge
            {
                tooltip = m_Participant.Name,
                backgroundColor = AvatarColorPalette.GetColor(m_Participant.ColorIndex),
                size = Size.M,
                outlineColor = Color.clear,
                Initials =
                {
                    text = Utils.GetInitials(m_Participant.Name)
                }
            };
            
            avatar.AddToClassList(k_CollaboratorDataBadgeUssClassName);
            root.Add(avatar);

            var header = new Heading(m_Participant.Name)
            {
                size = HeadingSize.S
            };
            
            header.AddToClassList(k_CollaboratorDataHeaderUssClassName);
            root.Add(header);
            
            return root;
        }
    }
}
