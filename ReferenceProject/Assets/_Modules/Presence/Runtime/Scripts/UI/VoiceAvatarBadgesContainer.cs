using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;

namespace Unity.ReferenceProject.Presence
{
    public class VoiceAvatarBadgesContainer : AvatarBadgesContainer
    {
#if USE_VIVOX
        static readonly string k_AvatarDividerUssClassName = "divider__avatar";
        
        IVoiceManager m_VoiceManager;
        readonly List<IVoiceParticipant> m_VoiceParticipants = new ();

        protected override void RefreshParticipantsBadges(RoomCached room)
        {
            if (room == null || room.Room == null)
            {
                return;
            }

            if (m_VoiceParticipants != null && !m_VoiceParticipants.IsEmpty())
            {
                var speakingParticipant = m_VoiceParticipants.OrderByDescending(p => p.AudioIntensity).First();
                if (speakingParticipant.AudioIntensity > 0)
                {
                    var participant = room.Participants.FirstOrDefault(p =>
                        VoiceManager.GetVoiceId(m_VoiceManager.GetVoiceParticipant(p)) == speakingParticipant.Id);
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
        
        void VoiceParticipantAdded(IEnumerable<IVoiceParticipant> participants)
        {
            var matchesIds = m_VoiceParticipants.Where(v => participants.Any(p => p.Id == v.Id)).Select(p => p.ParticipantId).ToList();
            m_VoiceParticipants.AddRange(participants.Where(participant => !matchesIds.Contains(participant.ParticipantId)).ToList());
            RefreshContainer(Room);
        }
        
        void VoiceParticipantRemoved(IEnumerable<IVoiceParticipant> participants)
        {
            var matchesIds = m_VoiceParticipants.Where(v => participants.Any(p => p?.Id == v.Id)).Select(p => p?.ParticipantId).ToList();
            foreach (var participant in participants.Where(participant => matchesIds.Contains(participant?.ParticipantId)).ToList())
            {
                m_VoiceParticipants.Remove(participant);
            }
            RefreshContainer(Room);
        }
        
        void VoiceParticipantUpdated(IEnumerable<IVoiceParticipant> participants)
        {
            var matchesIds = m_VoiceParticipants.Where(v => participants.Any(p => p.Id == v.Id)).Select(p => p.ParticipantId).ToList();
            foreach (var participant in participants.Where(participant => matchesIds.Contains(participant.ParticipantId)).ToList())
            {
                m_VoiceParticipants[m_VoiceParticipants.IndexOf(m_VoiceParticipants.First(p => p.ParticipantId == participant.ParticipantId))] = participant;
            }
            RefreshContainer(Room);
        }
#endif
        
        public VoiceAvatarBadgesContainer(ColorPalette avatarColorPalette) : base(avatarColorPalette)
        {
            
        }
        
        public void BindVoiceManager(IVoiceManager voiceManager)
        {
#if USE_VIVOX
            m_VoiceManager = voiceManager;
            m_VoiceManager.VoiceParticipantAdded += VoiceParticipantAdded;
            m_VoiceManager.VoiceParticipantRemoved += VoiceParticipantRemoved;
            m_VoiceManager.VoiceParticipantUpdated += VoiceParticipantUpdated;
            RefreshContainer(Room);
#endif
        }

        public void UnBindVoiceManager(IVoiceManager voiceManager)
        {
#if USE_VIVOX
            voiceManager.VoiceParticipantAdded -= VoiceParticipantAdded;
            voiceManager.VoiceParticipantRemoved -= VoiceParticipantRemoved;
            voiceManager.VoiceParticipantUpdated -= VoiceParticipantUpdated;

            VoiceParticipantRemoved(m_VoiceParticipants);
            m_VoiceParticipants.Clear();
            m_VoiceManager = voiceManager;
#endif
        }
    }
}

