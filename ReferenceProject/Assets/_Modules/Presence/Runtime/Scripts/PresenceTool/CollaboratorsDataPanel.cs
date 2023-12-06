using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class CollaboratorsDataPanel
    {
        static readonly string k_CollaboratorDataUssClassName = "container__collaborator-data";

        public bool IsDirty { get; private set; }
        public bool ShowFollowButton { get; set; } = true;
        public ColorPalette AvatarColorPalette { get; set; }
        public StyleSheet VoiceLevelStyleSheet { get; set; }

        VisualElement m_RootVisualElement;
        readonly Dictionary<CollaboratorDataUI, VisualElement> m_Collaborators = new();

        public event Action<IParticipant> DataUIEnterFollowMode;
        public event Action<IParticipant> DataUIExitFollowMode;

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
                Utils.SetVisible(participant.FollowButton, ShowFollowButton);
                element.AddToClassList(k_CollaboratorDataUssClassName);
                m_RootVisualElement.Add(element);

                m_Collaborators[participant] = element;
            }

            return m_RootVisualElement;
        }

        public void AddParticipant(IParticipant participant)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participant.Id);
            if (data.Key != null)
                return;

            m_Collaborators.Add(new CollaboratorDataUI(participant) { AvatarColorPalette = AvatarColorPalette, VoiceLevelStyleSheet = VoiceLevelStyleSheet }, null);
            IsDirty = true;
        }

        public void RemoveParticipant(ParticipantId participantId)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participantId);
            if (data.Key == null)
                return;

            m_Collaborators.Remove(data.Key);
            IsDirty = true;
        }

        public void ClearParticipants()
        {
            m_Collaborators.Clear();
            IsDirty = true;
        }

        void UnselectAllButtons()
        {
           foreach (var CollaboratorDataUI in m_Collaborators.Keys)
           {
               CollaboratorDataUI.UpdateButton(false);
           }
        }

        public void OnExitFollowMode()
        {
            UnselectAllButtons();
        }

        public void UpdateFollowModeCollaboratorDataUI(ParticipantId participantId, bool isPresentation = false)
        {
            UnselectAllButtons();
            if (isPresentation)
                return;
            var collaboratorUI = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participantId);
            collaboratorUI.Key.FollowModeSelected(participantId);
        }

        void OnUIEnterFollowMode(IParticipant participant)
        {
            DataUIEnterFollowMode?.Invoke(participant);
        }

        void OnUIExitFollowMode(IParticipant participant)
        {
            DataUIExitFollowMode?.Invoke(participant);
        }

        public void SubscribeUIFollowModeEvent(ParticipantId participantId)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participantId);
            if (data.Key == null)
                return;
            data.Key.UIEnterFollowMode += OnUIEnterFollowMode;
            data.Key.UIExitFollowMode += OnUIExitFollowMode;
        }
        public void UnSubscribeUIFollowModeEvent(ParticipantId participantId)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participantId);
            if (data.Key == null)
                return;
            data.Key.UIEnterFollowMode -= OnUIEnterFollowMode;
            data.Key.UIExitFollowMode -= OnUIExitFollowMode;
        }

        #region VIVOX

        public void AddVoiceToParticipant(IVoiceParticipant voiceParticipant)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == voiceParticipant.ParticipantId);
            if (data.Key == null || data.Key.IsVoiceParticipant)
                return;

            data.Key.VoiceParticipant = voiceParticipant;
            IsDirty = true;
        }

        public void RemoveVoiceToParticipant(ParticipantId participantId)
        {
            var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == participantId);
            if (data.Key == null || !data.Key.IsVoiceParticipant)
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

        public void VoiceParticipantAdded(IEnumerable<IVoiceParticipant> participants)
        {
            foreach (var voiceParticipant in participants)
            {
                AddVoiceToParticipant(voiceParticipant);
            }
        }

        public void VoiceParticipantRemoved(IEnumerable<IVoiceParticipant> participants)
        {
            foreach (var voiceParticipant in participants)
            {
                RemoveVoiceToParticipant(voiceParticipant.ParticipantId);
            }
        }

        public void VoiceParticipantUpdated(IEnumerable<IVoiceParticipant> participants)
        {
            if (m_Collaborators.Count == 0)
                return;

            foreach (var voiceParticipant in participants)
            {
                var data = m_Collaborators.FirstOrDefault(p => p.Key.Participant.Id == voiceParticipant.ParticipantId);
                if (data.Key != null)
                {
                    data.Key.VoiceParticipant = voiceParticipant;
                    IsDirty = true;
                }
            }
        }

        #endregion
    }
}
