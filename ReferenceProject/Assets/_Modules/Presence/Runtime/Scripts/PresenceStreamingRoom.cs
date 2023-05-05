using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public sealed class PresenceStreamingRoom : MonoBehaviour
    {
        IRoomProvider<Room> m_RoomProvider;
        IAuthenticationStateProvider m_AuthenticationStateProvider;

        Room m_CurrentRoom;

        public event Action<Room> RoomJoined;
        public event Action RoomLeft;

        [Inject]
        public void Setup(IRoomProvider<Room> roomProvider, IAuthenticationStateProvider authenticationStateProvider, ISceneEvents sceneEvents)
        {
            m_RoomProvider = roomProvider;
            m_AuthenticationStateProvider = authenticationStateProvider;

            sceneEvents.SceneOpened += OnSceneOpened;
            sceneEvents.SceneClosed += OnSceneClosed;
        }

        public async void OnEnable()
        {
            m_AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Update UI with current state
            await ApplyAuthenticationState(m_AuthenticationStateProvider.AuthenticationState);
        }

        public async void OnDisable()
        {
            await LeaveRoom();

            m_AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        void OnAuthenticationStateChanged(AuthenticationState newAuthenticationState)
        {
            _ = ApplyAuthenticationState(newAuthenticationState);
        }

        async Task ApplyAuthenticationState(AuthenticationState state)
        {
            if (state == AuthenticationState.LoggedOut)
            {
                await LeaveRoom();
            }
        }

        async Task JoinRoom(SceneId sceneId)
        {
            if (m_CurrentRoom != null)
            {
                Debug.LogError("Previous Room needs to be leaved before entering a new one.");
                return;
            }
            
            if (sceneId == SceneId.None)
            {
                Debug.LogError("No SceneId Provided.");
                return;
            }

            m_CurrentRoom = await m_RoomProvider.GetRoomAsync(sceneId);

            await m_CurrentRoom.JoinAsync(new NoRetryPolicy(), CancellationToken.None);
            
            RoomJoined?.Invoke(m_CurrentRoom);
        }

        async Task LeaveRoom()
        {
            if (m_CurrentRoom == null)
                return;
            
            await m_CurrentRoom.LeaveAsync();
            //await m_CurrentRoom.StopMonitoringAsync(new NoRetryPolicy(), CancellationToken.None);
            m_CurrentRoom = null;

            RoomLeft?.Invoke();
        }

        async void OnSceneOpened(IScene scene)
        {
            await LeaveRoom();
            await JoinRoom(scene.Id);
        }

        async void OnSceneClosed()
        {
            await LeaveRoom();
        }

        
        public IParticipant GetParticipantFromID(ParticipantId id)
        {
            return m_CurrentRoom.ConnectedParticipants.FirstOrDefault(p => p.Id == id);
        }
    }
}
