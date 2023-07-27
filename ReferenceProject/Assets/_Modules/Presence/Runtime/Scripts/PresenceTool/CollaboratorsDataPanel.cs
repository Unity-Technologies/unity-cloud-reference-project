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

        public VisualElement CreateVisualTree()
        {
            if (!IsDirty && m_RootVisualElement != null)
            {
                return m_RootVisualElement;
            }

            IsDirty = false;

            if (m_RootVisualElement == null)
            {
                m_RootVisualElement = new ScrollView();
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

        #region VIVOX
        public void AddVoiceToParticipant(IParticipant participant, VoiceParticipant voiceParticipant)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participant.Id);
            if(data.Key == null || data.Key.IsVoiceParticipant)
                return;

            data.Key.VoiceParticipant = voiceParticipant;
            IsDirty = true;
        }
        
        public void RemoveVoiceToParticipant(IParticipant participant)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participant.Id);
            if(data.Key == null || !data.Key.IsVoiceParticipant)
                return;

            data.Key.VoiceParticipant = null;
            IsDirty = true;
        }

        public void DisableVivox()
        {
            foreach (var collaborator in m_Collaborators)
            {
                collaborator.Key.VoiceParticipant = null;
                IsDirty = true;
            }
        }

        public void VoiceServiceUpdated(IEnumerable<VoiceParticipant> participants)
        {
            foreach (var collaboratorData in m_Collaborators.Keys)
            {
                var voice = participants.FirstOrDefault(p => collaboratorData.VoiceParticipant != null && p.VoiceId == collaboratorData.VoiceParticipant.Value.VoiceId);
                if(!collaboratorData.VoiceParticipant.Equals(voice))
                {
                    collaboratorData.VoiceParticipant = voice;
                    IsDirty = true;
                }
            }
        }

        #endregion
    }
}
