using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Identity;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public interface IPresenceRoomsManager
    {
        Task<bool> JoinRoomAsync(OrganizationId organizationId, AssetId assetId, object instance);
        Room GetRoomForAsset(AssetId assetId);
        Task<bool> LeaveRoomAsync(Room room);
        Task UnsubscribeFromMonitoring(Room room, object instance);

        void RegisterOnDisconnect(Func<Task> callback);
        void UnRegisterOnDisconnect(Func<Task> callback);
    }

    public sealed class PresenceRoomsManager : MonoBehaviour, IPresenceRoomsManager
    {
        ICloudSession m_Session;
        readonly Dictionary<AssetId, Room> m_Rooms = new();
        IRoomProvider<Room> m_RoomProvider;
        IPresenceManager m_PresenceManager;
        IAssetRepository m_AssetRepository;

        Task m_LeaveRoomTask;
        Task m_JoinRoomTask;

        readonly Dictionary<Room, HashSet<object>> m_Subscribers = new();
        readonly HashSet<Room> m_MonitorRooms = new();
        readonly HashSet<Room> m_QueuedHashSet = new();

        Task m_TrackMonitoringTask;
        Task m_MonitoringQueueTask;
        readonly HashSet<Func<Task>> m_DisconnectCallbacks = new();

        // Cancels only tasks in this script. It will not cancel Room.StartMonitoring or Room.StopMonitoring task 
        readonly CancellationTokenSource m_LocalCancellationTokenSource = new ();

        bool m_Destroyed;

        [Inject]
        public void Setup(ICloudSession session, IRoomProvider<Room> roomProvider, IPresenceManager presenceManager, IAssetRepository assetRepository)
        {
            m_Session = session;
            m_RoomProvider = roomProvider;
            m_PresenceManager = presenceManager;
            m_AssetRepository = assetRepository;
            m_Session.RegisterLoggedInCallback(ConnectToPresenceServer);
            m_Session.RegisterLoggingOutCallback(DisconnectFromPresenceServer);
        }
        
        async void OnEnable()
        {
            if (m_Session.State == SessionState.LoggedIn)
                await ConnectToPresenceServer();
        }

        async Task ConnectToPresenceServer()
        {
            if (m_PresenceManager.PresenceRoomsConnectionState != ConnectionState.Disconnected)
                return;

            await m_PresenceManager.ConnectPresenceRoomsAsync(new ExponentialBackoffRetryPolicy(), m_LocalCancellationTokenSource.Token);
        }

        async Task DisconnectFromPresenceServer()
        {
            if (m_PresenceManager.PresenceRoomsConnectionState != ConnectionState.Connected)
                return;

            foreach (Func<Task> callback in m_DisconnectCallbacks)
            {
                await callback.Invoke();
            }

            await m_PresenceManager.DisconnectPresenceRoomsAsync();
        }

        async void OnDisable()
        {
            m_Destroyed = true;

            CancelToken(m_LocalCancellationTokenSource); // It will stop TrackMonitoringAsync

            if (m_MonitoringQueueTask != null && !m_MonitoringQueueTask.IsCompleted)
            {
                await m_MonitoringQueueTask;
            }

            await ResetRoomsAsync();
        }

        void OnDestroy()
        {
            m_Destroyed = true;
            m_LocalCancellationTokenSource?.Dispose();

            m_Session.UnRegisterLoggedInCallback(ConnectToPresenceServer);
            m_Session.UnRegisterLoggingOutCallback(DisconnectFromPresenceServer);
        }

        async Task GetRoomsForOrganizationAsync(OrganizationId organizationId)
        {
            m_Rooms.Clear();

            var cancellationToken = new CancellationTokenSource().Token;
            var projects = m_AssetRepository.ListAssetProjectsAsync(organizationId, 
                new Pagination(nameof(IProject.Name), Range.All), cancellationToken);

            await foreach (var project in projects.WithCancellation(cancellationToken))
            {
                var assets = project.SearchAssetsAsync(
                    new AssetSearchFilter(),
                    new Pagination(nameof(IAsset.Name), Range.All),
                    cancellationToken);

                await foreach (var asset in assets.WithCancellation(cancellationToken))
                {
                    var assetId = asset.Descriptor.AssetId;
                    var rooms = await m_RoomProvider.GetRoomsAsync(organizationId.ToString(), "dataset", assetId.ToString(), asset.Name);
                    if (rooms.Any())
                    {
                        m_Rooms[assetId] = rooms[0];
                    }
                }
            }
        }

        async Task MonitorRoom(Room room, object instance, bool isSubscribe, CancellationToken cancellationToken)
        {
            if (room == null)
                return;

            if (!m_Subscribers.ContainsKey(room))
            {
                m_Subscribers.Add(room, new HashSet<object>());
            }

            if (isSubscribe)
            {
                m_Subscribers[room].Add(instance);
            }
            else
            {
                m_Subscribers[room].Remove(instance);
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

                if (!m_Subscribers.TryGetValue(room, out var subscribers) || cancellationToken.IsCancellationRequested)
                    break;

                if (subscribers.Count != 0 && !m_MonitorRooms.Contains(room))
                {
                    // Start monitoring
                    m_MonitoringQueueTask = StartMonitoringRoomAsync(room);
                }
                else if (subscribers.Count == 0 && m_MonitorRooms.Contains(room))
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

            var task = room.StartMonitoringAsync();
            await task;

            if (task.IsCompletedSuccessfully)
            {
                m_MonitorRooms.Add(room);
            }
        }

        async Task StopMonitoringRoomAsync(Room room, bool isRemoveRoom)
        {
            if (room == null)
                return;

            var task = room.StopMonitoringAsync();
            await task;

            if (task.IsCompletedSuccessfully && isRemoveRoom)
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

        public async Task SubscribeForMonitoring(Room room, object instance)
        {
            if (m_Destroyed) // Stops execution if this gameObject already destroyed
                return;

            await MonitorRoom(room, instance, true, m_LocalCancellationTokenSource.Token);
        }

        public async Task UnsubscribeFromMonitoring(Room room, object instance)
        {
            if (m_Destroyed) // Stops execution if this gameObject already destroyed
                return;
            
            await MonitorRoom(room, instance, false, m_LocalCancellationTokenSource.Token);
        }

        public async Task<bool> JoinRoomAsync(OrganizationId organizationId, AssetId assetId, object instance)
        {
            if (assetId == AssetId.None)
            {
                Debug.LogError("No Asset Provided.");
                return false;
            }

            if (m_JoinRoomTask != null && !m_JoinRoomTask.IsCompleted)
            {
                Debug.LogError($"Can't join room for asset id '{assetId}' because previous join process still in progress.");
                return false;
            }

            if (!m_Rooms.ContainsKey(assetId))
            {
                var assetIdStr = assetId.ToString();
                var room = await m_RoomProvider.CreateRoomAsync(organizationId.ToString(),
                    new RoomCreationParams(assetIdStr, assetIdStr, "asset")).ConfigureAwait(false);
                
                m_Rooms.Add(assetId, room);
            }

            // You need first to monitor the room events
            if (!m_Subscribers.ContainsKey(m_Rooms[assetId]))
            {
                m_Subscribers.Add(m_Rooms[assetId], new HashSet<object>());
            }
            else
            {
                m_Subscribers[m_Rooms[assetId]].Add(instance);
            }

            if (!m_MonitorRooms.Contains(m_Rooms[assetId]))
            {
                await StartMonitoringRoomAsync(m_Rooms[assetId]);
            }

            m_JoinRoomTask = m_Rooms[assetId].JoinAsync();
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

        public Room GetRoomForAsset(AssetId assetId) => m_Rooms.TryGetValue(assetId, out var room) ? room : null;

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

        public void RegisterOnDisconnect(Func<Task> callback)
        {
            m_DisconnectCallbacks.Add(callback);
        }

        public void UnRegisterOnDisconnect(Func<Task> callback)
        {
            m_DisconnectCallbacks.Remove(callback);
        }
    }
}
