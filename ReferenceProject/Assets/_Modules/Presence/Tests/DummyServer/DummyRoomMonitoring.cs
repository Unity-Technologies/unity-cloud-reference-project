using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Unity.Cloud.Presence;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class DummyRoomMonitoring : IRoomMonitoring
    {
        public event Action<IParticipant> ParticipantConnected;
        public event Action<IParticipant> ParticipantDisconnected;
        public ConnectionStatus ConnectionStatus => ConnectionStatus.Connected;
        public event Action<ConnectionStatus> ConnectionStatusChanged;
        public ReadOnlyDictionary<RoomId, BaseRoom> AllMonitoredRooms { get; }

        readonly DummyServer m_Server;
        
        public DummyRoomMonitoring(DummyServer server)
        {
            m_Server = server;
            m_Server.ChangeParticipantStatus += ChangeParticipantStatus;
        }
        
        public Task StartMonitoringAsync(params BaseRoom[] rooms)
        {
            ConnectionStatusChanged?.Invoke(ConnectionStatus);
            
            m_Server.StartToMonitorRoom(rooms[0].RoomId);
            return Task.CompletedTask;
        }

        public Task StopMonitoringAsync(params BaseRoom[] rooms)
        {
            m_Server.StopToMonitorRoom(rooms[0].RoomId);
            return Task.CompletedTask;
        }

        public Task StopMonitoringAllAsync()
        {
            m_Server.StopToMonitorAllRooms();
            return Task.CompletedTask;
        }

        void ChangeParticipantStatus(IParticipant participant, bool isConnected)
        {
            if (isConnected)
            {
                ParticipantConnected?.Invoke(participant);
            }
            else
            {
                ParticipantDisconnected?.Invoke(participant);
            }
        }
    }
}
