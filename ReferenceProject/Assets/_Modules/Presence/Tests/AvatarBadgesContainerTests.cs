using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.ReferenceProject.Common;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class AvatarBadgesContainerTests : TestBase
    {
        static ColorPalette CreateColorPalette() => ScriptableObject.CreateInstance<ColorPalette>();
        
        [Test]
        public async Task ParticipantBadgeMaxLimit()
        {
            var badgeContainer = new AvatarBadgesContainer(CreateColorPalette());

            var room = await m_PresenceManager.GetRoomAsync(GetNextFreeRoomID);
            var roomCached = new RoomCached(room);
            badgeContainer.BindRoom(roomCached);
            Assert.AreEqual(0, badgeContainer.childCount);

            await room.StartMonitoringAsync(null, new CancellationToken());
            Assert.AreEqual(0, badgeContainer.childCount);

            // Add one participant
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            Assert.AreEqual(1, badgeContainer.childCount);

            // Avoiding Self adding
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString(), isSelf: true);
            Assert.AreEqual(1, badgeContainer.childCount);

            // Test max count
            for (int i = 0; i < badgeContainer.MaxParticipantsCount + 3; i++)
            {
                m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            }

            Assert.AreEqual(badgeContainer.MaxParticipantsCount + 1, badgeContainer.childCount);
        }

        [Test]
        public async Task PlusSignBadge_When_Participants_Exceed_Less_Equals_MaxLimit()
        {
            var badgeContainer = new AvatarBadgesContainer(CreateColorPalette());

            var room = await m_PresenceManager.GetRoomAsync(GetNextFreeRoomID);
            var roomCached = new RoomCached(room);
            badgeContainer.BindRoom(roomCached);

            // Test =>  more participants count than max count
            for (int i = 0; i < badgeContainer.MaxParticipantsCount + 3; i++)
            {
                m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            }

            await room.StartMonitoringAsync(null, new CancellationToken());
            Assert.AreNotEqual(0, badgeContainer.childCount);
            Assert.AreEqual(AvatarBadgesContainer.PlusSignName, badgeContainer.Children().Last().name);
            
            m_DummyServer.RemoveAllParticipants(); // Reset Participants count
            
            // Test => less participants count than maxCount
            for (int i = 0; i < badgeContainer.MaxParticipantsCount - 1; i++)
            {
                m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            }
            
            Assert.AreNotEqual(0, badgeContainer.childCount);
            Assert.AreNotEqual(AvatarBadgesContainer.PlusSignName, badgeContainer.Children().Last().name);
            
            m_DummyServer.RemoveAllParticipants(); // Reset Participants count
            
            // Test =>  equal participants count than maxCount
            for (int i = 0; i < badgeContainer.MaxParticipantsCount; i++)
            {
                m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            }
            
            Assert.AreNotEqual(0, badgeContainer.childCount);
            Assert.AreNotEqual(AvatarBadgesContainer.PlusSignName, badgeContainer.Children().Last().name);
            
            // Test => avoid to add itself
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString(), isSelf: true);
            Assert.AreNotEqual(0, badgeContainer.childCount);
            Assert.AreNotEqual(AvatarBadgesContainer.PlusSignName, badgeContainer.Children().Last().name);
        }

        [Test]
        public async Task ParticipantCachedRoom_Remain_Same_After_Badges_Created()
        {
            var badgeContainer = new AvatarBadgesContainer(CreateColorPalette());
            var room = await m_PresenceManager.GetRoomAsync(GetNextFreeRoomID);
            var roomCached = new RoomCached(room);
            badgeContainer.BindRoom(roomCached);

            // Add Participant
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString(), isSelf: true);
            await room.StartMonitoringAsync(null, new CancellationToken());

            // Check Participants count in cached room
            Assert.AreEqual(room.ConnectedParticipants.Count, roomCached.Participants.Count);
        }

        [Test]
        public async Task NotAdding_Palette()
        {
            var badgeContainer = new AvatarBadgesContainer(CreateColorPalette());
            var room = await m_PresenceManager.GetRoomAsync(GetNextFreeRoomID);
            var roomCached = new RoomCached(room);
            badgeContainer.BindRoom(roomCached);

            // Add Participant
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString(), isSelf: true);
            await room.StartMonitoringAsync(null, new CancellationToken());

            // Check Participants count in cached room
            Assert.AreEqual(1, badgeContainer.childCount);
        }

        [Test]
        public async Task MaxParticipantsCount_Change()
        {
            var badgeContainer = new AvatarBadgesContainer(CreateColorPalette());
            badgeContainer.MaxParticipantsCount = 2;

            var room = await m_PresenceManager.GetRoomAsync(GetNextFreeRoomID);
            var roomCached = new RoomCached(room);
            badgeContainer.BindRoom(roomCached);

            // Test max count
            for (int i = 0; i < 10; i++)
            {
                m_DummyServer.AddParticipant(GetNextParticipantID, room.RoomId.ToString());
            }

            await room.StartMonitoringAsync(null, new CancellationToken());
            Assert.AreEqual(badgeContainer.MaxParticipantsCount + 1, badgeContainer.childCount);

            badgeContainer.MaxParticipantsCount = 3;
            Assert.AreEqual(badgeContainer.MaxParticipantsCount + 1, badgeContainer.childCount);

            badgeContainer.MaxParticipantsCount = 1;
            Assert.AreEqual(badgeContainer.MaxParticipantsCount + 1, badgeContainer.childCount);
        }
    }
}
