using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Unity.Cloud.Common.Runtime;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class TestBase
    {
        protected DummyServer m_DummyServer;
        protected DummyPresenceManager m_PresenceManager;

        int roomIdPool = 0;
        protected RoomId GetNextFreeRoomID => new RoomId((++roomIdPool).ToString());

        int participantIdPool = 0;
        protected string GetNextParticipantID => (++participantIdPool).ToString();

        [SetUp]
        public void Setup()
        {
            m_DummyServer = new DummyServer();
            m_PresenceManager = new DummyPresenceManager(new DummyRoomMonitoring(m_DummyServer), new DummyRoomJoiner());
        }
    }
}

