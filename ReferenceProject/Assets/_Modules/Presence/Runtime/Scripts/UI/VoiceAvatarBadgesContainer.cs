using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class VoiceAvatarBadgesContainer : AvatarBadgesContainer
    {
#if USE_VIVOX
        static readonly string k_AvatarDividerUssClassName = "avatar-divider";
        
        VoiceManager m_VoiceManager;
        IEnumerable<VoiceParticipant> m_VoiceParticipants;

        protected override void RefreshParticipantsBadges(RoomCached room)
        {
            if (m_VoiceParticipants != null && !m_VoiceParticipants.IsEmpty())
            {
                var speakingParticipant = m_VoiceParticipants.OrderByDescending(p => p.AudioIntensity).First();
                if (speakingParticipant.AudioIntensity > 0)
                {
                    var participant = room.Participants.FirstOrDefault(p =>
                        VoiceManager.GetVoiceId(m_VoiceManager.GetVoiceParticipant(p)) == speakingParticipant.VoiceId);
                    if (participant != null)
                    {
                        Add(CreateParticipantBadge(participant));
                        var divider = new Divider { name = "divider", vertical = true, spacing = Spacing.S };
                        divider.AddToClassList(k_AvatarDividerUssClassName);
                        Add(divider);
                    }
                }
            }

            base.RefreshParticipantsBadges(room);
        }
        
        void OnVoiceServiceUpdated(IEnumerable<VoiceParticipant> voiceParticipants)
        {
            m_VoiceParticipants = voiceParticipants;
            RefreshContainer(Room);
        }
#endif
        
        public VoiceAvatarBadgesContainer(ColorPalette avatarColorPalette) : base(avatarColorPalette)
        {
            
        }
        
        public void BindVoiceManager(VoiceManager voiceManager)
        {
#if USE_VIVOX
            m_VoiceManager = voiceManager;
            m_VoiceManager.VoiceServiceUpdated += OnVoiceServiceUpdated;
            RefreshContainer(Room);
#endif
        }
    }
}

