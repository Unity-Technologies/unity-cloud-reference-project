using System;
using System.Threading.Tasks;
using Unity.Cloud.Presence;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Tests.Runtime
{
    public class DummyRoomJoiner : IRoomJoiner
    {
        // Things below required by the interface
        // Hide warning CS0067: Never used
#pragma warning disable 0067
        public ISession Session { get; }
        public event Action<ISession> SessionChanged;
        
        public ConnectionStatus ConnectionStatus { get; }
        public event Action<ConnectionStatus> ConnectionStatusChanged;
#pragma warning restore 0067

        public Task JoinAsync(BaseRoom room)
        {
            // Because we do not use it in tests
            throw new NotImplementedException();
        }

        public Task LeaveAsync()
        {
            // Because we do not use it in tests
            throw new NotImplementedException();
        }
    }
}
