using System;
using System.Collections.Generic;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;

namespace Unity.ReferenceProject.Presence
{
    public class RoomCached
    {
        public event Action ParticipantsChanged;
        
        Room m_Room;

        public Room Room
        {
            get => m_Room;
            set
            {
                if (m_Room != null)
                {
                    m_Room.ParticipantAdded -= OnRefresh;
                    m_Room.ParticipantRemoved -= OnRefresh;
                }

                m_Room = value;

                if (value != null)
                {
                    value.ParticipantAdded += OnRefresh;
                    value.ParticipantRemoved += OnRefresh;
                }
                
                RefeshCachedParticipants();
            }
        }

        readonly List<IParticipant> m_CachedParticipants = new();
        
        public List<IParticipant> Participants => m_CachedParticipants;

        public RoomCached(Room room) => Room = room;

        void RefeshCachedParticipants()
        {
            m_CachedParticipants.Clear();
            
            if (m_Room != null)
            {
                m_CachedParticipants.AddRange(m_Room.ConnectedParticipants);
            }

            ParticipantsChanged?.Invoke();
        }

        void OnRefresh(IParticipant obj)
        {
            RefeshCachedParticipants();
        }
    }
}
