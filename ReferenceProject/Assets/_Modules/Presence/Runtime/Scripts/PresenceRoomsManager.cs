using System;
using System.Collections.Generic;
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

        [Inject]
        public void Setup(IRoomProvider<Room> roomProvider, IAuthenticationStateProvider authenticationStateProvider, ISceneProvider sceneProvider)
        {
            m_RoomProvider = roomProvider;
            m_SceneProvider = sceneProvider;
            m_AuthenticationStateProvider = authenticationStateProvider;
        }

        public async void OnEnable()
        {
            m_AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Update rooms
            await ApplyAuthenticationStateAsync(m_AuthenticationStateProvider.AuthenticationState);
        }

        public async void OnDisable()
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
            
            foreach (var scene in sceneList)
            {
                var room = await m_RoomProvider.GetRoomAsync(scene.Id);
            
                if (room == null)
                    continue;
                
                m_Rooms[scene.Id] = room;
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
    }
}
