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
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public sealed class PresenceRoomsManager : MonoBehaviour, IDisposable
    {
        readonly List<Room> m_Rooms = new();
        readonly Dictionary<Room, AvatarBadgesContainer> m_AvatarBadgesContainers = new();
        IRoomProvider<Room> m_RoomProvider;
        IAuthenticationStateProvider m_AuthenticationStateProvider;
        IWorkspaceRepository m_WorkspaceRepository;
        
        Room m_CurrentRoom;
        
        List<IWorkspace> m_CurrentWorkspaces;

        CancellationTokenSource m_CancellationTokenSource;
        CancellationToken CancellationToken => m_CancellationTokenSource.Token;
        public event Func<IWorkspace, Task> WorkspaceSelected;
        public event Action<Room> RoomJoined;
        public event Action RoomLeft;

        SceneId m_SceneId = SceneId.None;

        [Inject]
        public void Setup(IRoomProvider<Room> roomProvider, IWorkspaceRepository workspaceRepository, IAuthenticationStateProvider authenticationStateProvider)
        {
            m_RoomProvider = roomProvider;
            m_WorkspaceRepository = workspaceRepository;
            m_AuthenticationStateProvider = authenticationStateProvider;
        }

        public async void OnEnable()
        {
            m_AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
            WorkspaceSelected += OnWorkspaceSelected;

            // Update UI with current state
            await ApplyAuthenticationState(m_AuthenticationStateProvider.AuthenticationState);
        }

        public async void OnDisable()
        {
            await ResetRoom();

            m_AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            WorkspaceSelected -= OnWorkspaceSelected;

            if (m_CancellationTokenSource != null && CancellationToken.CanBeCanceled)
                m_CancellationTokenSource.Cancel();
        }

        void OnAuthenticationStateChanged(AuthenticationState newAuthenticationState)
        {
            ApplyAuthenticationState(newAuthenticationState).ConfigureAwait(false);
        }

        async Task ApplyAuthenticationState(AuthenticationState state)
        {
            switch (state)
            {
                case AuthenticationState.LoggedIn:
                    await PopulateWorkspaces();
                    break;
                case AuthenticationState.LoggedOut:
                    await ResetRoom();
                    break;
            }
        }

        async Task PopulateWorkspaces()
        {
            await ResetRoom();

            var workspaces = m_WorkspaceRepository.ListWorkspacesAsync(Range.All);
            var list = new List<IWorkspace>();
            await foreach (var workspace in workspaces)
            {
                list.Add(workspace);
            }

            m_CurrentWorkspaces = list;
        }

        async Task OnWorkspaceSelected(IWorkspace workspace)
        {
            await ResetRoom();

            var scenes = await workspace.ListScenesAsync();
            var sceneList = scenes.ToList();

            foreach (var scene in sceneList)
            {
                var room = await m_RoomProvider.GetRoomAsync((string)scene.Id);

                if (!m_Rooms.Contains(room))
                {
                    m_Rooms.Add(room);
                    await StartMonitoringRoomAsync(room);
                }
            }
        }
        
        public void OnWorkspaceOptionChanged(ChangeEvent<int> changeEvent)
        {
            if (m_CurrentWorkspaces.Count > 0)
            {
                var workspace = m_CurrentWorkspaces[changeEvent.newValue];
                WorkspaceSelected?.Invoke(workspace);
            }
        }

        async Task ResetRoom()
        {
            if (m_CurrentRoom != null)
            {
                await LeaveRoom();
                m_CurrentRoom = null;
            }

            bool notifyRoom = true;

            foreach (var room in m_Rooms)
            {
                notifyRoom = await StopMonitoringRoomAsync(room, notifyRoom);
            }

            m_Rooms.Clear();
        }

        async Task StartMonitoringRoomAsync(Room room)
        {
            if (room != null)
            {
                ManageCancellation();
                IRetryPolicy retryPolicy = new ExponentialBackoffRetryPolicy();
                var result = await HandleRetryQueuedAndExecuteAction(() => room.StartMonitoringAsync(retryPolicy, CancellationToken));

                if (result && m_AvatarBadgesContainers.TryGetValue(room, out AvatarBadgesContainer container))
                {
                    BindRoomEventsToAvatarBadgesContainer(room, container);
                }
            }
        }

        async Task<bool> StopMonitoringRoomAsync(Room room, bool notifyRoom = true)
        {
            if (room != null)
            {
                UnBindRoomEventsToAvatarBadgesContainer(room);

                if (notifyRoom)
                {
                    ManageCancellation();
                    IRetryPolicy retryPolicy = new ExponentialBackoffRetryPolicy();
                    return await HandleRetryQueuedAndExecuteAction(() =>
                        room.StopMonitoringAsync(retryPolicy, CancellationToken));
                }
            }

            return false;
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

            ManageCancellation();

            m_CurrentRoom = await m_RoomProvider.GetRoomAsync((string)sceneId);
            
            // No need to retry in this sample
            IRetryPolicy retryPolicy = new NoRetryPolicy();
            var result = await HandleRetryQueuedAndExecuteAction(() => m_CurrentRoom.JoinAsync(retryPolicy, CancellationToken));

            if (result)
            {
                RoomJoined?.Invoke(m_CurrentRoom);
            }
        }

        async Task LeaveRoom()
        {
            if (m_CurrentRoom == null)
            {
                Debug.LogError("Unable to leave Room since none has been joined.");
                return;
            }
            
            await m_CurrentRoom.LeaveAsync();
            m_CurrentRoom = null;

            RoomLeft?.Invoke();
        }

        async void OnJoinRoom()
        {
            if (m_CurrentRoom != null)
            {
                await LeaveRoom();
            }

            await JoinRoom(m_SceneId);
        }

        void OnSceneSelected(IScene scene)
        {
            m_SceneId = SceneId.None;
            
            if (scene != null)
            {
                m_SceneId = scene.Id;
            }
        }

        void ManageCancellation()
        {
            if (m_CancellationTokenSource != null && CancellationToken.CanBeCanceled)
                m_CancellationTokenSource.Cancel();

            m_CancellationTokenSource = new CancellationTokenSource();
        }

        async Task<bool> HandleRetryQueuedAndExecuteAction(Func<Task> action)
        {
            try
            {
                await action();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                m_CancellationTokenSource?.Dispose();
                m_CancellationTokenSource = null;
            }
        }

        public int ConnectedParticipantsForScene(IScene scene)
        {
            if (m_Rooms.Where(x => x.RoomId == scene.Id.ToString()) is Room room)
            {
                return room.ConnectedParticipants.Count;
            }

            return 0;
        }

        public string GetParticipantNameFromID(string id)
        {
            var participant = m_CurrentRoom.ConnectedParticipants.FirstOrDefault(p => p.Id == id);
            if (!string.IsNullOrEmpty(participant.Name))
                return participant.Name;

            return null;
        }

        public void BindRoomEventsToAvatarBadgesContainer(SceneId id, AvatarBadgesContainer container)
        {
            var room = m_Rooms.FirstOrDefault(x => x.RoomId == id.ToString());
            if (room == null)
                return;
            BindRoomEventsToAvatarBadgesContainer(room, container);
        }

        public void BindRoomEventsToAvatarBadgesContainer(Room room, AvatarBadgesContainer container)
        {
            if (m_AvatarBadgesContainers.ContainsKey(room))
            {
                UnBindRoomEventsToAvatarBadgesContainer(room);
                m_AvatarBadgesContainers[room] = container;
            }
            else
                m_AvatarBadgesContainers.Add(room, container);
            
            container.AddParticipants(room.ConnectedParticipants);
            room.ParticipantAdded += container.AddParticipant;
            room.ParticipantRemoved += container.RemoveParticipant;
        }

        void UnBindRoomEventsToAvatarBadgesContainer(Room room)
        {
            if (room != null && m_AvatarBadgesContainers.TryGetValue(room, out var container))
            {
                container.ClearParticipants();
                room.ParticipantAdded -= container.AddParticipant;
                room.ParticipantRemoved -= container.RemoveParticipant;
            }
        }

        public void Dispose()
        {
            m_CancellationTokenSource?.Dispose();
        }
    }
}
