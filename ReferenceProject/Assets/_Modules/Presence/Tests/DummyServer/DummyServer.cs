using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Cloud.Presence;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public struct DummyParticipant : IParticipant
    {
        public ParticipantId Id { get; internal set; }

        public string Name { get; internal set; }

        public RoomId RoomId { get; internal set; }

        public int ColorIndex { get; internal set; }

        public DateTime Connected { get; internal set; }

        public CommunicationId CommunicationId { get; internal set; }

        public bool IsSelf { get; internal set; }
    }
    
    public class DummyServer
    {
        public event Action<IParticipant, bool> ChangeParticipantStatus;
        
        readonly Dictionary<string, IParticipant> m_AllParticipants = new Dictionary<string, IParticipant>();
        readonly HashSet<string> m_AllMonitoringRooms = new HashSet<string>();

        public void StartToMonitorRoom(RoomId roomId)
        {
            if (m_AllMonitoringRooms.Add(roomId.ToString()))
            {
                foreach (var keyValuePair in m_AllParticipants.Where(x => x.Value.RoomId == roomId))
                {
                    ChangeParticipantStatus?.Invoke(keyValuePair.Value, true);
                }
            }
        }
        
        public void StopToMonitorRoom(RoomId roomId)
        {
            m_AllMonitoringRooms.Remove(roomId.ToString());
        }
        
        public void StopToMonitorAllRooms()
        {
            m_AllMonitoringRooms.Clear();
        }

        public void AddParticipant(string uid, string roomId, string name = null, bool isSelf = false)
        {
            var participant = new DummyParticipant()
            {
                Id = new ParticipantId(uid),
                Name = string.IsNullOrEmpty(name) ? GenerateName() : name,
                RoomId = new RoomId(roomId),
                IsSelf = isSelf
            };

            m_AllParticipants.Add(uid, participant);

            if (m_AllMonitoringRooms.Contains(roomId))
            {
                ChangeParticipantStatus?.Invoke(participant, true);
            }
        }

        public void RemoveParticipant(string uid)
        {
            if (m_AllParticipants.TryGetValue(uid, out var participant))
            {
                if (m_AllMonitoringRooms.Contains(participant.RoomId.ToString()))
                {
                    ChangeParticipantStatus?.Invoke(participant, false);
                }

                m_AllParticipants.Remove(uid);
            }
        }

        public void RemoveAllParticipants()
        {
            var participantsToUpdate = m_AllParticipants
                .Where(participant => m_AllMonitoringRooms.Contains(participant.Value.RoomId.ToString()));

            foreach (var participant in participantsToUpdate)
            {
                ChangeParticipantStatus?.Invoke(participant.Value, false);
            }
            
            m_AllParticipants.Clear();
        }
        
        string GenerateName()
        {
            return k_FullNamesDataBase[Random.Range(0, k_FullNamesDataBase.Length)];
        }

        readonly string[] k_FullNamesDataBase =
        {
            "Emma Johnson",
            "Liam Smith",
            "Olivia Williams",
            "Noah Brown",
            "Ava Jones",
            "Jackson Davis",
            "Isabella Miller",
            "Lucas Garcia",
            "Sophia Rodriguez",
            "Oliver Martinez",
            "Mia Martinez",
            "Aiden Hernandez",
            "Amelia Lopez",
            "Elijah Gonzalez",
            "Harper Perez",
            "James Lee",
            "Charlotte Kim",
            "Benjamin Lee",
            "Abigail Lee",
            "Mason Kim",
            "Evelyn Chang",
            "Logan Nguyen",
            "Ella Rivera",
            "William Yang",
            "Sofia Patel",
            "Michael Ali",
            "Alexander Kim",
            "Emily Wong",
            "Ethan Chen"
        };
    }
}
