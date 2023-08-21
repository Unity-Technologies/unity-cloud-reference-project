using System;
using System.Collections.Generic;
using System.Linq;
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
        PresenceRoomsManager m_PresenceRoomsManager;
        Room m_CurrentRoom;
        
        public event Action<Room> RoomJoined;
        public event Action RoomLeft;

        [Inject]
        public void Setup(IAuthenticationStateProvider authenticationStateProvider, ISceneEvents sceneEvents, PresenceRoomsManager roomManager)
        {
            m_AuthenticationStateProvider = authenticationStateProvider;
            m_PresenceRoomsManager = roomManager;

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
            if (m_CurrentRoom != null)
            {
                Debug.LogError("Previous Room Not Left Yet");
                return;
            }
            
            var isJoined = await m_PresenceRoomsManager.JoinRoomAsync(sceneId, GetInstanceID());
            
            if(!isJoined)
                return;
            
            m_CurrentRoom = m_PresenceRoomsManager.GetRoomForScene(sceneId);
            RoomJoined?.Invoke(m_CurrentRoom);
        }

        async Task LeaveRoom()
        {
            var isRoomLeft = await m_PresenceRoomsManager.LeaveRoomAsync(m_CurrentRoom);
            await m_PresenceRoomsManager.UnsubscribeFromMonitoring(m_CurrentRoom, GetInstanceID());

            if (isRoomLeft)
            {
                m_CurrentRoom = null;
                RoomLeft?.Invoke();
            }
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
            return m_CurrentRoom?.ConnectedParticipants.FirstOrDefault(p => p.Id == id);
        }
    }
}
