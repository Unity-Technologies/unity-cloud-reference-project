using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

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
        public Task<Room> GetRoomAsync(SceneId id)
        {
            return GetRoomAsync(new RoomId(id.ToString()));
        }

        /// <inheritdoc />
        public Task<Room> GetRoomAsync(RoomId id)
        {
            if (!m_CreatedRooms.ContainsKey(id))
            {
                Type myClassType = typeof(Room);
                object[] constructorArgs = { id, m_Joiner, m_Monitoring };

                m_CreatedRooms[id] = (Room)Activator.CreateInstance(myClassType, BindingFlags.Instance | BindingFlags.NonPublic, null, constructorArgs, null);
            }

            return Task.FromResult(m_CreatedRooms[id]);
        }
    }
}


