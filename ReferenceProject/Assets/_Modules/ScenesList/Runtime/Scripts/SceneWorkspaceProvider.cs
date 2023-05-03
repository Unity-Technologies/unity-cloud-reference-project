using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Storage;
using UnityEngine;

namespace Unity.ReferenceProject.ScenesList
{
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
