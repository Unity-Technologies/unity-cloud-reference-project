using System.Collections.Generic;
using System.Linq;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class CollaboratorsDataPanel
    {
        static readonly string k_CollaboratorDataUssClassName = "collaborator-data";

        public bool IsDirty { get; private set; }
        public ColorPalette AvatarColorPalette { get; set; }

        VisualElement m_RootVisualElement;
        readonly Dictionary<CollaboratorDataUI, VisualElement> m_Collaborators = new();

        public void AddParticipant(IParticipant participant)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participant.Id);
            if(data.Key != null)
                return;

            m_Collaborators.Add(new CollaboratorDataUI(participant){ AvatarColorPalette = this.AvatarColorPalette }, null);
            IsDirty = true;
        }

        public void RemoveParticipant(IParticipant participant)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participant.Id);
            if(data.Key == null)
                return;

            m_Collaborators.Remove(data.Key);
            IsDirty = true;
        }

        public void ClearParticipants()
        {
            m_Collaborators.Clear();
            IsDirty = true;
        }

        public VisualElement CreateVisualTree()
        {
            if (!IsDirty && m_RootVisualElement != null)
            {
                return m_RootVisualElement;
            }

            IsDirty = false;

            if (m_RootVisualElement == null)
            {
                m_RootVisualElement = new VisualElement();
            }
            else
            {
                m_RootVisualElement.Clear();
            }
            
            foreach (var participant in m_Collaborators.Keys.ToArray())
            {
                var element = participant.CreateVisualTree();
                element.AddToClassList(k_CollaboratorDataUssClassName);
                m_RootVisualElement.Add(element);

                m_Collaborators[participant] = element;
            }
            
            return m_RootVisualElement;
        }
    }
}
