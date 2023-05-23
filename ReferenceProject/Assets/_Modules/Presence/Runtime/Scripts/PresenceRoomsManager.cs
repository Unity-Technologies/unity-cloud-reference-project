using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.Cloud.Storage;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public sealed class PresenceRoomsManager : MonoBehaviour, IDisposable
    {
        readonly Dictionary<SceneId, Room> m_Rooms = new();
        IRoomProvider<Room> m_RoomProvider;
        IAuthenticationStateProvider m_AuthenticationStateProvider;

        CancellationTokenSource m_CancellationTokenSource;
        CancellationToken CancellationToken => m_CancellationTokenSource.Token;

        ISceneProvider m_SceneProvider;
        
        Room m_CurrentRoom;
        public Room CurrentRoom => m_CurrentRoom;
        
        Task m_LeaveRoomTask;
        Task m_JoinRoomTask;

        [Inject]
        public void Setup(IRoomProvider<Room> roomProvider, IAuthenticationStateProvider authenticationStateProvider, ISceneProvider sceneProvider)
        {
            m_RoomProvider = roomProvider;
            m_SceneProvider = sceneProvider;
            m_AuthenticationStateProvider = authenticationStateProvider;
        }

        async void OnEnable()
        {
            m_AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Update rooms
            await ApplyAuthenticationStateAsync(m_AuthenticationStateProvider.AuthenticationState);
        }

        async void OnDisable()
        {
            await ResetRoom();

            m_AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;

            if (m_CancellationTokenSource != null && CancellationToken.CanBeCanceled)
                m_CancellationTokenSource.Cancel();
        }

        void OnAuthenticationStateChanged(AuthenticationState newAuthenticationState)
        {
           _ = ApplyAuthenticationStateAsync(newAuthenticationState).ConfigureAwait(false);
        }

        async Task ApplyAuthenticationStateAsync(AuthenticationState state)
        {
            switch (state)
            {
                case AuthenticationState.LoggedIn:
                    await StartMonitoringAllRoomsAsync();
                    break;
                case AuthenticationState.LoggedOut:
                    await ResetRoom();
                    break;
            }
        }

        async Task StartMonitoringAllRoomsAsync()
        {
            await ResetRoom();
            
            var sceneList = await m_SceneProvider.ListScenesAsync();
            
            foreach (var sceneId in sceneList.Select(scene => scene.Id))
            {
                var room = await m_RoomProvider.GetRoomAsync(sceneId);
            
                if (room == null)
                    continue;
                
                m_Rooms[sceneId] = room;
            }

            foreach (var room in m_Rooms)
            {
                await StartMonitoringRoomAsync(room.Value);
            }
        }

        async Task ResetRoom()
        {
            foreach (var room in m_Rooms)
            {
                await StopMonitoringRoomAsync(room.Value);
            }

            m_Rooms.Clear();
        }

        async Task StartMonitoringRoomAsync(Room room)
        {
            if (room != null)
            {
                ManageCancellation();
                IRetryPolicy retryPolicy = new ExponentialBackoffRetryPolicy();
                await HandleRetryQueuedAndExecuteAction(() => room.StartMonitoringAsync(retryPolicy, CancellationToken));
            }
        }

        async Task StopMonitoringRoomAsync(Room room)
        {
            if (room != null)
            {
                ManageCancellation();
                IRetryPolicy retryPolicy = new ExponentialBackoffRetryPolicy();
                await HandleRetryQueuedAndExecuteAction(() => room.StopMonitoringAsync(retryPolicy, CancellationToken));
            }
        }
        

        void ManageCancellation()
        {
            if (m_CancellationTokenSource != null && CancellationToken.CanBeCanceled)
                m_CancellationTokenSource.Cancel();

            m_CancellationTokenSource = new CancellationTokenSource();
        }

        async Task HandleRetryQueuedAndExecuteAction(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                m_CancellationTokenSource?.Dispose();
                m_CancellationTokenSource = null;
            }
        }

        public Room GetRoomForScene(SceneId sceneId)
        {
            return m_Rooms.ContainsKey(sceneId) ? m_Rooms[sceneId] : null;
        }

        public void Dispose()
        {
            m_CancellationTokenSource?.Dispose();
        }

        public async Task<bool> JoinRoom(SceneId sceneId)
        {
            if (m_CurrentRoom != null)
            {
                Debug.LogError("Previous Room needs to be leaved before entering a new one.");
                return false;
            }
            
            if (sceneId == SceneId.None)
            {
                Debug.LogError("No SceneId Provided.");
                return false;
            }

            if (m_JoinRoomTask != null && !m_JoinRoomTask.IsCompleted)
            {
                Debug.LogError($"Can't join room {sceneId} because previous join process still in progress.");
                return false;
            }

            if (!m_Rooms.TryGetValue(sceneId, out m_CurrentRoom))
            {
                m_CurrentRoom = await m_RoomProvider.GetRoomAsync(sceneId);
            }
            
            m_JoinRoomTask = m_CurrentRoom.JoinAsync(new NoRetryPolicy(), CancellationToken.None);
            await m_JoinRoomTask;
            
            return true;
        }

        public async Task<bool> LeaveRoom()
        {
            if (m_CurrentRoom == null)
                return false;

            if (m_LeaveRoomTask != null && !m_LeaveRoomTask.IsCompleted)
            {
                Debug.LogWarning($"Already attempting to disconnect from Joined room");
                await m_LeaveRoomTask;
                return true;
            }

            m_LeaveRoomTask = m_CurrentRoom.LeaveAsync();
            await m_LeaveRoomTask;

            m_CurrentRoom = null;
            return true;
        }
    }
}
