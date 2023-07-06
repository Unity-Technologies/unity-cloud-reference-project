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
        IAuthenticationStateProvider m_AuthenticationStateProvider;
        PresenceRoomsManager m_RoomManager;
        
        public event Action<Room> RoomJoined;
        public event Action RoomLeft;

        [Inject]
        public void Setup(IAuthenticationStateProvider authenticationStateProvider, ISceneEvents sceneEvents, PresenceRoomsManager roomManager)
        {
            m_AuthenticationStateProvider = authenticationStateProvider;
            m_RoomManager = roomManager;

            sceneEvents.SceneOpened += OnSceneOpened;
            sceneEvents.SceneClosed += OnSceneClosed;
        }

        async void OnEnable()
        {
            m_AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Update UI with current state
            await ApplyAuthenticationState(m_AuthenticationStateProvider.AuthenticationState);
        }

        async void OnDisable()
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
            var isJoined = await m_RoomManager.JoinRoomAsync(sceneId);
            
            m_RoomManager.SubscribeForMonitoring(sceneId, GetInstanceID());
            
            if(!isJoined)
                return;
            
            RoomJoined?.Invoke(m_RoomManager.CurrentRoom);
        }

        async Task LeaveRoom()
        {
            var currentRoomLeft = await m_RoomManager.LeaveRoomAsync();
            
            m_RoomManager.UnsubscribeFromMonitoring(m_RoomManager.CurrentRoom, GetInstanceID());
            
            if(currentRoomLeft)
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
            return m_RoomManager.CurrentRoom?.ConnectedParticipants.FirstOrDefault(p => p.Id == id);
        }
    }
}
