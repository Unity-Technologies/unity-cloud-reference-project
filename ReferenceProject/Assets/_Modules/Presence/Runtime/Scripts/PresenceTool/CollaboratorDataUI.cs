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
        public readonly IParticipant Participant;

        public CollaboratorDataUI(IParticipant participant)
        {
            Participant = participant;
        }

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

            return element;
        }
    }
}
