using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Storage;
using UnityEngine;

namespace Unity.ReferenceProject.ScenesList
{
    class SceneComparer : IEqualityComparer<IScene>
    {
        public bool Equals(IScene x, IScene y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name && x.Id.Equals(y.Id) && x.WorkspaceId.Equals(y.WorkspaceId) && Equals(x.Permissions, y.Permissions) && x.LatestVersion.Equals(y.LatestVersion);
        }

        public int GetHashCode(IScene obj)
        {
            return HashCode.Combine(obj.Name, obj.Id, obj.WorkspaceId, obj.Permissions, obj.LatestVersion);
        }
    }

    public class SceneWorkspaceProvider
    {
        readonly Dictionary<SceneId, IScene> m_IdToScenesLookUp = new ();
        readonly Dictionary<WorkspaceId, string> m_IdToWorkspaces = new ();

        Task m_RefreshTask;

        readonly IWorkspaceRepository m_WorkspaceProvider;

        public SceneWorkspaceProvider(IWorkspaceRepository workspaceProvider)
        {
            m_WorkspaceProvider = workspaceProvider;
        }

        public List<IScene> GetAllScenes() => m_IdToScenesLookUp.Values.ToList();

        public Task RefreshAsync()
        {
            if (m_RefreshTask == null || m_RefreshTask.IsCompleted)
                m_RefreshTask = PopulateWorkspacesAsync();
            return m_RefreshTask;
        }

        async Task PopulateWorkspacesAsync()
        {
            if (m_WorkspaceProvider == null)
            {
                Debug.LogWarning("No workspace provider available.");
                return;
            }

            var workspaces = m_WorkspaceProvider.ListWorkspacesAsync(Range.All);
            await GetAllScenesFromWorkspacesAsync(workspaces);
        }

        async Task GetAllScenesFromWorkspacesAsync(IAsyncEnumerable<IWorkspace> workspaces)
        {
            m_IdToScenesLookUp.Clear();
            m_IdToWorkspaces.Clear();

            await foreach (var workspace in workspaces)
            {
                var selectedScenes = await GetScenesFromWorkspaceAsync(workspace);
                foreach (var scene in selectedScenes)
                {
                    m_IdToScenesLookUp.TryAdd(scene.Id, scene);
                }
                m_IdToWorkspaces.TryAdd(workspace.Id, workspace.Name);
            }
        }

        static async Task<List<IScene>> GetScenesFromWorkspaceAsync(IWorkspace workspace)
        {
            var listScenesAsync = await workspace.ListScenesAsync();
            return listScenesAsync.ToList();
        }

        public string GetWorkspaceName(WorkspaceId workspaceID)
        {
            return m_IdToWorkspaces.TryGetValue(workspaceID, out var id) ? id : string.Empty;
        }
    }
}
