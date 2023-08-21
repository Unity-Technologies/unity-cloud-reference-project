using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class DummyServerTests : TestBase
    {
        [Test]
        public async Task Add_Remove_Participants()
        {
            var roomID = GetNextFreeRoomID;
            string firstParticipantId = GetNextParticipantID;
            m_DummyServer.AddParticipant(firstParticipantId, roomID.ToString());
            m_DummyServer.AddParticipant(GetNextParticipantID, roomID.ToString());

            // Start room monitoring
            var room = await m_PresenceManager.GetRoomAsync(roomID);
            await room.StartMonitoringAsync(new NoRetryPolicy(), new CancellationToken());
            Assert.AreEqual(2, room.ConnectedParticipants.Count);

            // Stop monitoring
            await room.StopMonitoringAsync(null, new CancellationToken());
            Assert.AreEqual(0, room.ConnectedParticipants.Count);

            // Start room monitoring
            await room.StartMonitoringAsync(new NoRetryPolicy(), new CancellationToken());
            Assert.AreEqual(2, room.ConnectedParticipants.Count);

            // Remove one participant
            m_DummyServer.RemoveParticipant(firstParticipantId);
            Assert.AreEqual(1, room.ConnectedParticipants.Count);

            // Add one participant
            m_DummyServer.AddParticipant(GetNextParticipantID, roomID.ToString());
            Assert.AreEqual(2, room.ConnectedParticipants.Count);

            // Remove all participants
            m_DummyServer.RemoveAllParticipants();
            Assert.AreEqual(0, room.ConnectedParticipants.Count);

            await room.StopMonitoringAsync(null, new CancellationToken());
        }
    }
}
