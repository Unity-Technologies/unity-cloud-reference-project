using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class DummyPresenceManager : IRoomProvider<Room>, ISessionProvider
    {
        readonly Dictionary<RoomId, Room> m_CreatedRooms = new();
        readonly IRoomMonitoring m_Monitoring;
        readonly IRoomJoiner m_Joiner;

        /// <inheritdoc />
        public ISession Session => m_Joiner.Session;

        /// <inheritdoc />
        public event Action<ISession> SessionChanged
        {
            add => m_Joiner.SessionChanged += value;
            remove => m_Joiner.SessionChanged -= value;
        }

        public DummyPresenceManager(IRoomMonitoring roomMonitoring, IRoomJoiner roomJoiner)
        {
            m_Monitoring = roomMonitoring;
            m_Joiner = roomJoiner;
        }

        /// <inheritdoc />
        public Task<Room> GetRoomAsync(RoomId id)
        {
            if (!m_CreatedRooms.ContainsKey(id))
            {
                var myClassType = typeof(Room);
                object[] constructorArgs = { id, "DummyRoomName", "DummyResourceId", "DummyResourceType",
                    "DummyOrganization", m_Joiner, m_Monitoring };

                m_CreatedRooms[id] = (Room)Activator.CreateInstance(myClassType, BindingFlags.Instance | BindingFlags.NonPublic, null, constructorArgs, null);
            }

            return Task.FromResult(m_CreatedRooms[id]);
        }

        // TODO: Implement the rest of the interface to make the tests pass
        public Task<Room> CreateRoomAsync(string organizationId, RoomCreationParams roomCreationParams)
        {
            throw new NotImplementedException();
        }

        public Task<Room[]> GetRoomsAsync(string organizationId, string resourceType, string resourceId, string name)
        {
            throw new NotImplementedException();
        }
    }
}
