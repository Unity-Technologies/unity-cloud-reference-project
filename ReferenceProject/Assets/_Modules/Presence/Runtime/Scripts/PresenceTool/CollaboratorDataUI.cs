using Unity.Cloud.Presence;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class CollaboratorDataUI
    {
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
            
            var avatar = new UnityEngine.Dt.App.UI.Avatar
            {
                text = AvatarUtils.GetInitials(Participant.Name),
                tooltip = Participant.Name,
                backgroundColor = AvatarUtils.GetColor(Participant.ColorIndex), // This is temporary as the Presence Package will update its implementation
                size = Size.M
            };
            avatar.AddToClassList(k_CollaboratorDataBadgeUssClassName);
            element.Add(avatar);

            var header = new Header(Participant.Name);
            header.size = HeaderSize.S;
            header.AddToClassList(k_CollaboratorDataHeaderUssClassName);
            element.Add(header); 

            return element;
        }
    }
}
