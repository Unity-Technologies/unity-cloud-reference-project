using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Permissions;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    public enum AssetListPermission
    {
        None,
        AssetOpen,
        AssetPublish
    }

    public interface IAssetListController
    {
        IOrganization SelectedOrganization { get; }
        IAssetProject SelectedProject { get; }
        IAsset SelectedAsset { get; }
        bool IsCollectionSelected { get; }

        AssetSearchFilter Filters { get; }
        event Action RefreshStarted;
        event Action HideContent;
        event Action<IEnumerable<IOrganization>> OrganizationsPopulated;
        event Func<IOrganization, Task> OrganizationSelected;
        event Action<IAssetProject> ProjectSelected;
        event Action<IAssetCollection> CollectionSelected;
        event Action<IAsset> AssetSelected;
        event Action<IAsset> AssetHighlighted;
        event Action<bool> Loading;

        bool HighlightAsset(IAsset asset);
        Task<IAsyncEnumerable<IAsset>> GetAssetsAcrossAllProjectsAsync();
        IAsyncEnumerable<IAsset> GetAssetsAsync();
        Task<IEnumerable<IAssetProject>> GetAllProjects();
        Task<IAssetProject> GetProject(ProjectDescriptor projectDescriptor);
        void Close();
        Task Refresh();
        Task SelectOrganization(IOrganization organization);
        void SelectProject(IAssetProject project);
        void SelectCollection(IAssetCollection collection);
        void OpenStreamableAsset();
        Task UpdatePermissionAsync(ProjectId projectId);
        bool CheckPermission(AssetListPermission permission);
    }

    public class AssetListController : IAssetListController
    {
        static readonly Range k_DefaultPagination = Range.All;
        static readonly string k_SavedOrganizationIdKey = "SavedOrganizationId";
        static readonly string k_SavedProjectIdKey = "SavedProjectId";
        static readonly string k_OpenAssetPermission = "amc.assets.download";
        static readonly string k_PublishAssetPermission = "amc.assets.publish";

        public IOrganization SelectedOrganization { get; private set; }
        public IAssetProject SelectedProject { get; private set; }
        public IAsset SelectedAsset { get; private set; }
        public AssetSearchFilter Filters => m_Filters;
        public bool IsCollectionSelected { get; private set; }

        public event Action RefreshStarted;
        public event Action RefreshFinished;
        public event Action HideContent;
        public event Action<IEnumerable<IOrganization>> OrganizationsPopulated;
        public event Func<IOrganization, Task> OrganizationSelected;
        public event Action<IAssetProject> ProjectSelected;
        public event Action<IAssetCollection> CollectionSelected;
        public event Action<IAsset> AssetSelected;
        public event Action<IAsset> AssetHighlighted;
        public event Action<bool> Loading;
        public event Action AllProjects;

        readonly AssetSearchFilter m_Filters;
        CancellationTokenSource m_CancellationTokenSource;
        readonly Dictionary<OrganizationId, string> m_LastSelectedProjectIds = new();
        readonly Dictionary<ProjectId, IAssetProject> m_Projects = new();

        readonly IAssetRepository m_AssetRepository;
        readonly IOrganizationRepository m_OrganizationRepository;
        readonly IPermissionsController m_PermissionsController;

        public AssetListController(IAssetRepository assetRepository, IOrganizationRepository organizationRepository, IPermissionsController permissionsController)
        {
            m_AssetRepository = assetRepository;
            m_OrganizationRepository = organizationRepository;
            m_PermissionsController = permissionsController;

            m_Filters = new AssetSearchFilter();

            var lastSelectedOrganizationId = new OrganizationId(PlayerPrefs.GetString(k_SavedOrganizationIdKey));
            var lastSelectedProjectId = PlayerPrefs.GetString(k_SavedProjectIdKey);
            m_LastSelectedProjectIds.Add(lastSelectedOrganizationId, lastSelectedProjectId);
        }

        public bool HighlightAsset(IAsset asset)
        {
            SelectedAsset = asset;
            AssetHighlighted?.Invoke(asset);
            return true;
        }

        public async Task<IAsyncEnumerable<IAsset>> GetAssetsAcrossAllProjectsAsync()
        {
            try
            {
                CancelToken();

                var projects = await GetAllProjectsInternal();
                var projectDescriptors = projects.Select(p => p.Descriptor);

                var assetQueryBuilder = m_AssetRepository.QueryAssets(projectDescriptors)
                    .LimitTo(k_DefaultPagination)
                    .SelectWhereMatchesFilter(m_Filters);

                var assets = assetQueryBuilder.ExecuteAsync(m_CancellationTokenSource.Token);

                NullifyToken();

                return assets;
            }
            catch (OperationCanceledException oe)
            {
                Debug.LogException(oe);
                throw;
            }
            catch (AggregateException e)
            {
                Debug.LogException(e.InnerException);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                CancelToken();
                NullifyToken();
            }
        }

        public IAsyncEnumerable<IAsset> GetAssetsAsync()
        {
            try
            {
                CancelToken();

                var assetQueryBuilder = SelectedProject.QueryAssets()
                    .LimitTo(k_DefaultPagination)
                    .SelectWhereMatchesFilter(m_Filters);

                var assets = assetQueryBuilder.ExecuteAsync(m_CancellationTokenSource.Token);
                NullifyToken();
                return assets;
            }
            catch (OperationCanceledException oe)
            {
                Debug.LogException(oe);
                throw;
            }
            catch (AggregateException e)
            {
                Debug.LogException(e.InnerException);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                CancelToken();
                NullifyToken();
            }
        }

        public async Task<IAssetProject> GetProject(ProjectDescriptor projectDescriptor)
        {
            if (m_Projects.TryGetValue(projectDescriptor.ProjectId, out var project))
            {
                return project;
            }

            var newCancellationToken = false;
            if (m_CancellationTokenSource == null)
            {
                m_CancellationTokenSource = new CancellationTokenSource();
                newCancellationToken = true;
            }

            var newProject = await m_AssetRepository.GetAssetProjectAsync(projectDescriptor, m_CancellationTokenSource.Token);
            m_Projects.Add(projectDescriptor.ProjectId, newProject);
            if (newCancellationToken)
            {
                NullifyToken();
            }

            return newProject;
        }

        public async Task<IEnumerable<IAssetProject>> GetAllProjects()
        {
            CancelToken();

            var projectList = SelectedOrganization.ListProjectsAsync(Range.All);
            await foreach (var project in projectList)
            {
                TextureController.SetProjectIconUrl(project.Descriptor.ProjectId.ToString(), project.IconUrl);
            }

            var projects = await GetAllProjectsInternal();

            foreach (var project in projects)
            {
                m_Projects.TryAdd(project.Descriptor.ProjectId, project);
            }

            NullifyToken();

            return projects;
        }

        public void Close()
        {
            HideContent?.Invoke();
        }

        public async Task Refresh()
        {
            if (SelectedOrganization != null)
            {
                return;
            }
            
            RefreshStarted?.Invoke();
            await RefreshOrganization();
            RefreshFinished?.Invoke();
        }

        public async Task SelectOrganization(IOrganization organization)
        {
            m_PermissionsController.Reset();
            SelectedOrganization = organization;
            PlayerPrefs.SetString(k_SavedOrganizationIdKey, organization.Id.ToString());
            await OrganizationSelected?.Invoke(organization)!;
            await m_PermissionsController.SetOrganization(organization);

            // Select last selected project or first project by default
            await SelectLastOrDefaultProject();
        }

        public void SelectProject(IAssetProject project)
        {
            SelectProject(project, true);
        }

        public void SelectCollection(IAssetCollection collection)
        {
            m_Filters.Collections.WhereContains(collection.Descriptor.Path);
            IsCollectionSelected = true;

            var project = m_Projects[collection.Descriptor.ProjectDescriptor.ProjectId];
            SelectProject(project, false);
            CollectionSelected?.Invoke(collection);
        }

        public void OpenStreamableAsset()
        {
            if (SelectedAsset != null)
            {
                AssetSelected?.Invoke(SelectedAsset);
            }
        }

        public async Task UpdatePermissionAsync(ProjectId projectId)
        {
            if (SelectedOrganization == null)
            {
                m_PermissionsController.Reset();
                return;
            }

            await m_PermissionsController.SetProject(projectId);
        }

        public bool CheckPermission(AssetListPermission permission)
        {
            if (SelectedOrganization == null)
            {
                return false;
            }

            switch (permission)
            {
                case AssetListPermission.AssetOpen:
                    return m_PermissionsController.Permissions?.Contains(k_OpenAssetPermission) ?? false;
                case AssetListPermission.AssetPublish:
                    return m_PermissionsController.Permissions?.Contains(k_PublishAssetPermission) ?? false;
                default:
                    return false;
            }
        }

        async Task RefreshOrganization()
        {
            Loading?.Invoke(true);

            CancelToken();

            var organizations = new List<IOrganization>();

            await foreach (var org in m_OrganizationRepository.ListOrganizationsAsync(Range.All, m_CancellationTokenSource.Token))
            {
                organizations.Add(org);
            }

            OrganizationsPopulated?.Invoke(organizations);

            var savedOrganizationId = PlayerPrefs.GetString(k_SavedOrganizationIdKey);
            IOrganization lastOrganization = null;
            if (!string.IsNullOrEmpty(savedOrganizationId))
            {
                lastOrganization = organizations.Find(o => o.Id.ToString() == savedOrganizationId);
                if (lastOrganization != null)
                {
                    await SelectOrganization(lastOrganization);
                }
            }

            if (lastOrganization == null)
            {
                var firstOrganization = organizations.FirstOrDefault();
                if (firstOrganization != null)
                {
                    await SelectOrganization(firstOrganization);
                }
            }

            Loading?.Invoke(false);
        }

        async Task SelectLastOrDefaultProject()
        {
            if (SelectedOrganization != null)
            {
                CancelToken();

                var projects = m_AssetRepository.ListAssetProjectsAsync(
                    SelectedOrganization.Id,
                    Range.All,
                    m_CancellationTokenSource.Token);

                var enumerator = projects.GetAsyncEnumerator(m_CancellationTokenSource.Token);

                if (m_LastSelectedProjectIds.TryGetValue(SelectedOrganization.Id, out var lastProjectId))
                {
                    if (lastProjectId == "*")
                    {
                        SelectProject(null);
                        AllProjects?.Invoke();
                        return;
                    }

                    while (await enumerator.MoveNextAsync())
                    {
                        if (enumerator.Current.Descriptor.ProjectId.ToString() == lastProjectId)
                        {
                            SelectProject(enumerator.Current);
                            NullifyToken();
                            return;
                        }
                    }
                }

                // Select first project by default
                while (await enumerator.MoveNextAsync())
                {
                    SelectProject(enumerator.Current);
                    NullifyToken();
                    return;
                }

                Debug.Log($"No project found in {SelectedOrganization.Name}");
                SelectProject(null);
                NullifyToken();
            }
        }

        async Task<IEnumerable<IAssetProject>> GetAllProjectsInternal()
        {
            var projects = new List<IAssetProject>();
            var projectsAsync = m_AssetRepository.ListAssetProjectsAsync(SelectedOrganization.Id, k_DefaultPagination, m_CancellationTokenSource.Token);

            await foreach (var project in projectsAsync)
            {
                projects.Add(project);
            }

            return projects;
        }

        void SelectProject(IAssetProject project, bool clearCollection)
        {
            if (clearCollection)
            {
                m_Filters.Collections.WhereContains((IEnumerable<CollectionPath>)null);
                IsCollectionSelected = false;
                CollectionSelected?.Invoke(null);
            }

            SelectedProject = project;
            ProjectSelected?.Invoke(project);

            var projectId = project != null ? project.Descriptor.ProjectId : ProjectId.None;

            if (project != null)
            {
                PlayerPrefs.SetString(k_SavedProjectIdKey, projectId.ToString());
                m_LastSelectedProjectIds[SelectedOrganization.Id] = project.Descriptor.ProjectId.ToString();
                CancelToken();
            }
            else
            {
                PlayerPrefs.SetString(k_SavedProjectIdKey, "*");
                m_LastSelectedProjectIds[SelectedOrganization.Id] = "*";
            }

            _ = m_PermissionsController.SetProject(projectId, m_CancellationTokenSource.Token);
        }

        void NullifyToken()
        {
            m_CancellationTokenSource?.Dispose();
            m_CancellationTokenSource = null;
        }

        void CancelToken()
        {
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource?.Dispose();
            m_CancellationTokenSource = new CancellationTokenSource();
        }
    }
}
