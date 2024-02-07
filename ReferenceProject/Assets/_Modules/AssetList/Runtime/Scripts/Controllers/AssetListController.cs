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
        static readonly Pagination k_DefaultPagination = new(nameof(IAsset.Name), Range.All);
        static readonly string k_SavedOrganizationIdKey = "SavedOrganizationId";
        static readonly string k_SavedProjectIdKey = "SavedProjectId";
        static readonly string k_OpenAssetPermission = "amc.assets.download";
        static readonly string k_PublishAssetPermission = "amc.assets.publish";

        public IOrganization SelectedOrganization { get; private set; }
        public IAssetProject SelectedProject { get; private set; }
        public IAsset SelectedAsset { get; private set; }
        public AssetSearchFilter Filters => m_Filters;
        public bool IsCollectionSelected => m_Filters.Collections.Any();

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
        readonly IServiceHttpClient m_ServiceHttpClient;

        public AssetListController(IAssetRepository assetRepository, IOrganizationRepository organizationRepository, IPermissionsController permissionsController, IServiceHttpClient serviceHttpClient)
        {
            m_AssetRepository = assetRepository;
            m_OrganizationRepository = organizationRepository;
            m_PermissionsController = permissionsController;
            m_ServiceHttpClient = serviceHttpClient;

            m_Filters = new AssetSearchFilter();
            m_Filters.IncludedFields = FieldsFilter.All;

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
                var projectIds = projects.Select(p => p.Descriptor.ProjectId);
                var assets = m_AssetRepository.SearchAssetsAsync(SelectedOrganization.Id, projectIds, m_Filters, k_DefaultPagination, m_CancellationTokenSource.Token);

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
                var assets = SelectedProject.SearchAssetsAsync(m_Filters, k_DefaultPagination, m_CancellationTokenSource.Token);
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

            // Get Projects info
            var url = $"https://services.unity.com/api/unity/legacy/v1/organizations/{SelectedOrganization.Id}/projects?limit=100";

            var response = await m_ServiceHttpClient.GetAsync(url);
            var allProjectResponse = await response.JsonDeserializeAsync<AllProjectResponse>();

            foreach (var project in allProjectResponse.results)
            {
                TextureController.SetProjectIconUrl(project.id, project.iconUrl);
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
            m_Filters.Collections.Clear();
            m_Filters.Collections.Add(collection.Descriptor.CollectionPath);

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
            var organizations = await m_OrganizationRepository.ListOrganizationsAsync();
            OrganizationsPopulated?.Invoke(organizations);

            var savedOrganizationId = PlayerPrefs.GetString(k_SavedOrganizationIdKey);
            IOrganization lastOrganization = null;
            if (!string.IsNullOrEmpty(savedOrganizationId))
            {
                lastOrganization = organizations.FirstOrDefault(o => o.Id.ToString() == savedOrganizationId);
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
                    new(nameof(IProject.Name), Range.All),
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
                m_Filters.Collections.Clear();
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
