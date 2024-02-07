using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Identity;
using Unity.ReferenceProject.Permissions;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public enum PresencePermission
    {
        CreateRoom,
        JoinRoom,
        ListRooms,
        MonitorRooms,
        StartPresentation,
        UseCommunication,
    }

    public interface IPresenceRoomsManager
    {
        Task<bool> JoinRoomAsync(OrganizationId organizationId, AssetId assetId, object instance);
        Room GetRoomForAsset(AssetId assetId);
        void AddRoomForAsset(AssetId assetId, Room room);
        Task<Room> GetRoomAsync(OrganizationId organizationId, AssetId assetId);
        Task<Room> CreateRoomAsync(OrganizationId organizationId, AssetId assetId);
        Task<bool> LeaveRoomAsync(Room room);
        Task SubscribeForMonitoring(Room room, object instance);
        Task UnsubscribeFromMonitoring(Room room, object instance);
        void RegisterOnDisconnect(Func<Task> callback);
        void UnRegisterOnDisconnect(Func<Task> callback);
        bool CheckPermissions(PresencePermission permission);
    }

    public sealed class PresenceRoomsManager : MonoBehaviour, IPresenceRoomsManager
    {
        static readonly string k_PresenceCreateRoomPermission = "cmp.presence.create_room";
        static readonly string k_PresenceJoinRoomPermission = "cmp.presence.join_room";
        static readonly string k_PresenceListRoomsPermission = "cmp.presence.list_rooms";
        static readonly string k_PresenceMonitorRoomsPermission = "cmp.presence.monitor_rooms";
        static readonly string k_PresenceStartPresentationPermission = "cmp.presence.start_presentation";
        static readonly string k_PresenceUseCommunicationPermission = "cmp.presence.use_communication";

        ICloudSession m_Session;
        readonly Dictionary<AssetId, Room> m_Rooms = new();

        IRoomProvider<Room> m_RoomProvider;
        IPresenceManager m_PresenceManager;
        IPermissionsController m_PermissionsController;

        Task m_LeaveRoomTask;
        Task m_JoinRoomTask;

        readonly Dictionary<Room, HashSet<object>> m_Subscribers = new();
        readonly HashSet<Room> m_MonitorRooms = new();
        readonly HashSet<Room> m_QueuedHashSet = new();

        Task m_TrackMonitoringTask;
        Task m_MonitoringQueueTask;
        readonly HashSet<Func<Task>> m_DisconnectCallbacks = new();

        // Cancels only tasks in this script. It will not cancel Room.StartMonitoring or Room.StopMonitoring task 
        readonly CancellationTokenSource m_LocalCancellationTokenSource = new();

        bool m_Destroyed;

        [Inject]
        public void Setup(ICloudSession session, IRoomProvider<Room> roomProvider, IPresenceManager presenceManager, IPermissionsController permissionsController)
        {
            m_Session = session;
            m_RoomProvider = roomProvider;
            m_PresenceManager = presenceManager;
            m_PermissionsController = permissionsController;
            m_Session.RegisterLoggedInCallback(ConnectToPresenceServer);
            m_Session.RegisterLoggingOutCallback(DisconnectFromPresenceServer);
        }

        async void OnEnable()
        {
            if (m_Session.State == SessionState.LoggedIn)
                await ConnectToPresenceServer();
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
            if (m_Destroyed || !CheckPermissions(PresencePermission.MonitorRooms)) // Stops execution if this gameObject already destroyed || user doesn't have permission
                return;

            await MonitorRoom(room, instance, true, m_LocalCancellationTokenSource.Token);
        }

        public async Task UnsubscribeFromMonitoring(Room room, object instance)
        {
            if (m_Destroyed) // Stops execution if this gameObject already destroyed
                return;

            await MonitorRoom(room, instance, false, m_LocalCancellationTokenSource.Token);
        }

        public async Task<Room> GetRoomAsync(OrganizationId organizationId, AssetId assetId)
        {
            if (!CheckPermissions(PresencePermission.ListRooms))
                return null;

            try
            {
                var assetIdStr = assetId.ToString();
                var rooms = await m_RoomProvider.GetRoomsAsync(organizationId.ToString(), "asset", assetIdStr, assetIdStr).ConfigureAwait(false);
                return rooms.FirstOrDefault();
            }
            catch (Exception e)
            {
                if (e.GetType().Name == "NotFoundException")
                    return null;

                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Room> CreateRoomAsync(OrganizationId organizationId, AssetId assetId)
        {
            if (!CheckPermissions(PresencePermission.CreateRoom))
                return null;

            var assetIdStr = assetId.ToString();
            return await m_RoomProvider.CreateRoomAsync(organizationId.ToString(), new RoomCreationParams(assetIdStr, assetIdStr, "asset")).ConfigureAwait(false);
        }

        public async Task<bool> JoinRoomAsync(OrganizationId organizationId, AssetId assetId, object instance)
        {
            if (assetId == AssetId.None)
            {
                Debug.LogError("No Asset Provided.");
                return false;
            }

            if (m_JoinRoomTask is { IsCompleted: false } || !CheckPermissions(PresencePermission.JoinRoom))
            {
                return false;
            }

            if (!m_Rooms.ContainsKey(assetId))
            {
                var room = CheckPermissions(PresencePermission.CreateRoom) ? await CreateRoomAsync(organizationId, assetId) : await GetRoomAsync(organizationId, assetId);

                if (room == null)
                    return false;

                m_Rooms.Add(assetId, room);
            }

            // You need first to monitor the room events
            await SubscribeForMonitoring(m_Rooms[assetId], instance);

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
        public void AddRoomForAsset(AssetId assetId, Room room) => m_Rooms[assetId] = room;

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

        public bool CheckPermissions(PresencePermission permission)
        {
            switch (permission)
            {
                case PresencePermission.CreateRoom:
                    return m_PermissionsController.Permissions.Contains(k_PresenceCreateRoomPermission);
                case PresencePermission.JoinRoom:
                    return m_PermissionsController.Permissions.Contains(k_PresenceJoinRoomPermission);
                case PresencePermission.ListRooms:
                    return m_PermissionsController.Permissions.Contains(k_PresenceListRoomsPermission);
                case PresencePermission.MonitorRooms:
                    return m_PermissionsController.Permissions.Contains(k_PresenceMonitorRoomsPermission);
                case PresencePermission.StartPresentation:
                    return m_PermissionsController.Permissions.Contains(k_PresenceStartPresentationPermission);
                case PresencePermission.UseCommunication:
                    return m_PermissionsController.Permissions.Contains(k_PresenceUseCommunicationPermission);
                default:
                    return false;
            }
        }
    }
}
