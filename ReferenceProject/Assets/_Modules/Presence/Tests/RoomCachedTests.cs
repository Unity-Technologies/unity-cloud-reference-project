using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class RoomCachedTests : TestBase
    {
        [Test]
        public async Task CachingParticipants()
        {
            var roomID = GetNextFreeRoomID;

            // Creating a room and start monitor
            var room = await m_PresenceManager.GetRoomAsync(roomID);
            await room.StartMonitoringAsync(new NoRetryPolicy(), new CancellationToken());

            // Add participants on a server
            m_DummyServer.AddParticipant(GetNextParticipantID, roomID.ToString());
            m_DummyServer.AddParticipant(GetNextParticipantID, roomID.ToString());
            Assert.AreEqual(2, room.ConnectedParticipants.Count);

            // Creating cached room
            RoomCached roomCached = new RoomCached(room);
            Assert.AreEqual(2, roomCached.Participants.Count);

            // Stop monitoring
            await room.StopMonitoringAsync(null, new CancellationToken());
            Assert.AreEqual(0, room.ConnectedParticipants.Count);
            Assert.AreEqual(2, roomCached.Participants.Count);
        }

        [Test]
        public async Task ParticipantChanged_Event()
        {
            var roomID = GetNextFreeRoomID;

            // Creating a room and start monitor
            var room = await m_PresenceManager.GetRoomAsync(roomID);
            RoomCached roomCached = new RoomCached(room);
            int callbackCount = 0;
            roomCached.ParticipantsChanged += () => callbackCount++;
            await room.StartMonitoringAsync(new NoRetryPolicy(), new CancellationToken());
            Assert.AreEqual(0, callbackCount); // Because no participants

            // Add participant on a server
            callbackCount = 0;
            m_DummyServer.AddParticipant(GetNextParticipantID, roomID.ToString());
            Assert.AreNotEqual(0, callbackCount);
        }
    }
}
