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
    public sealed class PresenceRoomsManager : MonoBehaviour
    {
        readonly Dictionary<SceneId, Room> m_Rooms = new();
        IRoomProvider<Room> m_RoomProvider;
        IAuthenticationStateProvider m_AuthenticationStateProvider;

        ISceneProvider m_SceneProvider;
        
        Task m_LeaveRoomTask;
        Task m_JoinRoomTask;
        
        readonly Dictionary<Room, HashSet<int>> m_Subscribers = new ();
        readonly HashSet<Room> m_MonitorRooms = new ();
        readonly HashSet<Room> m_QueuedHashSet = new ();

        Task m_TrackMonitoringTask;
        Task m_MonitoringQueueTask;

        // For cancel Room.StartMonitoring or Room.StopMonitoring task 
        CancellationTokenSource m_MonitoringCancellationTokenSource; 

        // Cancels only tasks in this script. It will not cancel Room.StartMonitoring or Room.StopMonitoring task 
        readonly CancellationTokenSource m_LocalCancellationTokenSource = new CancellationTokenSource(); 
        
        bool m_Destroyed;

        [Inject]
        public void Setup(IRoomProvider<Room> roomProvider, IAuthenticationStateProvider authenticationStateProvider, ISceneProvider sceneProvider)
        {
            m_RoomProvider = roomProvider;
            m_SceneProvider = sceneProvider;
            m_AuthenticationStateProvider = authenticationStateProvider;
        }

        void OnEnable()
        {
            m_AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Update rooms
            OnAuthenticationStateChanged(m_AuthenticationStateProvider.AuthenticationState);
        }

        async void OnDisable()
        {
            m_Destroyed = true;
            m_AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            
            CancelToken(m_LocalCancellationTokenSource); // It will stop TrackMonitoringAsync
            
            if (m_MonitoringQueueTask != null && !m_MonitoringQueueTask.IsCompleted)
            {
                await m_MonitoringQueueTask;
            }

            await ResetRoomsAsync();

            CancelToken(m_MonitoringCancellationTokenSource);
        }
        
        void OnDestroy()
        {
            m_Destroyed = true;
            m_MonitoringCancellationTokenSource?.Dispose();
            m_LocalCancellationTokenSource?.Dispose();
        }

        void OnAuthenticationStateChanged(AuthenticationState newAuthenticationState)
        {
            if (newAuthenticationState == AuthenticationState.LoggedIn)
            {
                _ = GetAllRoomsAsync().ConfigureAwait(false);
            }
        }

        async Task GetAllRoomsAsync()
        {
            m_Rooms.Clear();
            
            var sceneList = await m_SceneProvider.ListScenesAsync();
            
            foreach (var sceneId in sceneList.Select(scene => scene.Id))
            {
                var room = await m_RoomProvider.GetRoomAsync(sceneId);
            
                if (room == null)
                    continue;
                
                m_Rooms[sceneId] = room;
            }
        }
        
        async Task MonitorRoom(Room room, int instanceId, bool isSubscribe, CancellationToken cancellationToken)
        {
            if(room == null)
                return;

            if (!m_Subscribers.ContainsKey(room))
            {
                m_Subscribers.Add(room, new HashSet<int>()); 
            }

            if (isSubscribe)
            {
                m_Subscribers[room].Add(instanceId);
            }
            else
            {
                m_Subscribers[room].Remove(instanceId);
            }
            
            m_QueuedHashSet.Add(room);
            
            if (m_TrackMonitoringTask == null || m_TrackMonitoringTask.IsCompleted)
            {
                m_TrackMonitoringTask = TrackMonitoringAsync(cancellationToken);
                await m_TrackMonitoringTask;
            }
        }
        
        async Task TrackMonitoringAsync(CancellationToken cancellationToken)
        {
            while (m_QueuedHashSet.Count > 0)
            {
                var room = m_QueuedHashSet.FirstOrDefault();

                if (room == null)
                    break;
                
                m_QueuedHashSet.Remove(room);

                if(!m_Subscribers.TryGetValue(room, out var subscribers) || cancellationToken.IsCancellationRequested)
                    break;

                if (subscribers.Count != 0 && !m_MonitorRooms.Contains(room))
                {
                    // Start monitoring
                    m_MonitoringQueueTask = StartMonitoringRoomAsync(room);
                }
                else if(subscribers.Count == 0 && m_MonitorRooms.Contains(room))
                {
                    // Stop monitoring
                    m_MonitoringQueueTask = StopMonitoringRoomAsync(room, true);
                }
                await m_MonitoringQueueTask;
            }
            
            m_TrackMonitoringTask = null;
        }
        
        async Task StartMonitoringRoomAsync(Room room)
        {
            if (room == null)
                return;
            
            var result = await HandleRetryQueuedAndExecuteActionAsync(() => room.StartMonitoringAsync(new NoRetryPolicy(), GetCancellationToken(ref m_MonitoringCancellationTokenSource))); 

            if (result)
            {
                m_MonitorRooms.Add(room);
            }
        }

        async Task StopMonitoringRoomAsync(Room room, bool isRemoveRoom)
        {
            if (room == null)
                return;

            var result = await HandleRetryQueuedAndExecuteActionAsync(() => room.StopMonitoringAsync(new NoRetryPolicy(), GetCancellationToken(ref m_MonitoringCancellationTokenSource)));
            
            if (result && isRemoveRoom)
            {
                m_MonitorRooms.Remove(room);
            }
        }
        
        async Task ResetRoomsAsync()
        {
            foreach (var room in m_MonitorRooms)
            {
                await StopMonitoringRoomAsync(room, false);
            }

            m_Subscribers.Clear();
            m_MonitorRooms.Clear();
            m_QueuedHashSet.Clear();
        }
        public async Task SubscribeForMonitoring(Room room, int instanceId)
        {
            if (m_Destroyed) // Stops execution if this gameObject already destroyed
                return;
            
            await MonitorRoom(room, instanceId, true, m_LocalCancellationTokenSource.Token);
        }
        public async Task UnsubscribeFromMonitoring(Room room, int instanceId)
        {
            if (m_Destroyed) // Stops execution if this gameObject already destroyed
                return;
            await MonitorRoom(room, instanceId, false, m_LocalCancellationTokenSource.Token);
        }
        
        public async Task<bool> JoinRoomAsync(SceneId sceneId, int instanceId)
        {
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
            
            if (!m_Rooms.ContainsKey(sceneId))
            {
                var room = await m_RoomProvider.GetRoomAsync(sceneId);
                m_Rooms.Add(sceneId, room);
            }
            
            // You need first to monitor the room events
            if (!m_Subscribers.ContainsKey(m_Rooms[sceneId]))
            {
                m_Subscribers.Add(m_Rooms[sceneId], new HashSet<int>()); 
            }
            else
            {
                m_Subscribers[m_Rooms[sceneId]].Add(instanceId);
            }
            
            if (!m_MonitorRooms.Contains(m_Rooms[sceneId]))
            {
                await StartMonitoringRoomAsync(m_Rooms[sceneId]);
            }

            m_JoinRoomTask = m_Rooms[sceneId].JoinAsync(new NoRetryPolicy(), CancellationToken.None);
            await m_JoinRoomTask;
            
            return true;
        }
        
        public async Task<bool> LeaveRoomAsync(Room room)
        {
            if (room == null)
                return false;

            // Already attempting to disconnect from Joined room
            if (m_LeaveRoomTask != null && !m_LeaveRoomTask.IsCompleted)
            {
                await m_LeaveRoomTask;
                return true;
            }

            m_LeaveRoomTask = room.LeaveAsync();
            await m_LeaveRoomTask;

            return true;
        }
        
        public Room GetRoomForScene(SceneId sceneId) => m_Rooms.ContainsKey(sceneId) ? m_Rooms[sceneId] : null;

        async Task<bool> HandleRetryQueuedAndExecuteActionAsync(Func<Task> action)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
            finally
            {
                m_MonitoringCancellationTokenSource?.Dispose();
                m_MonitoringCancellationTokenSource = null;
            }
        }
        
        static CancellationToken GetCancellationToken(ref CancellationTokenSource cancellationToken)
        {
            CancelToken(cancellationToken);
            cancellationToken = new CancellationTokenSource();
            return cancellationToken.Token;
        }

        static void CancelToken(CancellationTokenSource cancellationToken)
        {
            if (cancellationToken != null && cancellationToken.Token.CanBeCanceled)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }
        }
    }
}
