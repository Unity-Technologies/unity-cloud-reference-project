using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Cloud.Presence;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarBadgesContainer : VisualElement
    {
        public event Action OnParticipantsChanged;
        readonly Dictionary<string, UnityEngine.Dt.App.UI.Avatar> m_Participants = new();
        const int m_FullyShownAvatarsCount = 2;
        
        AvatarBadgesContainer()
        {
            name = "avatars-container";
            AddToClassList("avatars-container");

            style.flexDirection = FlexDirection.Row;
        }

        public static AvatarBadgesContainer Build()
        {
            var panel = new AvatarBadgesContainer();

            return panel;
        }
        
        public AvatarBadgesContainer SetWidth(StyleLength value)
        {
            style.width = value;

            return this;
        }

        public void AddParticipants(IEnumerable<Participant> players)
        {
            bool isParticipantsCountChanged = false;
            foreach (var connectedParticipant in players)
            {
                isParticipantsCountChanged |= TryAddParticipant(connectedParticipant);
            }

            if (isParticipantsCountChanged)
            {
                OnParticipantsChanged?.Invoke(); 
            }
        }

        public void AddParticipant(Participant player)
        {
            if (TryAddParticipant(player))
            {
                OnParticipantsChanged?.Invoke();
            }
        }
        
        bool TryAddParticipant(Participant player)
        {
            if (!player.IsSelf && !m_Participants.ContainsKey(player.Id))
            {
                var avatar = new UnityEngine.Dt.App.UI.Avatar
                {
                    text = AvatarUtils.GetInitials(player.Name),
                    tooltip = player.Name,
                    backgroundColor = AvatarUtils.GetColor(player.ColorIndex),
                    size = Size.M
                };
                
                m_Participants[player.Id] = avatar;
                return true;
            }
            return false;
        }

        public void RemoveParticipant(Participant player)
        {
            if (!m_Participants.ContainsKey(player.Id))
                return;

            m_Participants.Remove(player.Id);
            
            OnParticipantsChanged?.Invoke();
        }

        public void DrawAvatarsToContainer(VisualElement container)
        {
            Clear();

            var count = 0;
            foreach (var pair in m_Participants)
            {
                count++;
                if (count > m_FullyShownAvatarsCount)
                    break;
                Add(pair.Value);
            }

            if (m_Participants.Count > m_FullyShownAvatarsCount)
            {
                var plusSign = new UnityEngine.Dt.App.UI.Avatar();
                plusSign.text = "+" + (m_Participants.Count - m_FullyShownAvatarsCount);
                plusSign.backgroundColor = Color.gray;
                plusSign.size = Size.M;
                Add(plusSign);
            }
        }

        public void ClearParticipants()
        {
            foreach (var avatar in m_Participants.Values)
            {
                if (avatar != null)
                    Remove(avatar);
            }

            m_Participants.Clear();
            
            OnParticipantsChanged?.Invoke();
        }
    }
}
