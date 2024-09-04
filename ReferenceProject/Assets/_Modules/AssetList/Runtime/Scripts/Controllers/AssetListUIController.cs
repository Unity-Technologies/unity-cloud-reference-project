using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetCollectionInfo
    {
        public IAssetCollection Collection { get; set; }
        public int AssetCount { get; set; }
    }

    public class AssetListUIController : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        AssetGridUIController m_AssetGrid;

        [SerializeField]
        CollectionGridUIController m_CollectionGrid;

        [SerializeField]
        OrganizationUIController m_OrganizationList;

        [SerializeField]
        ProjectUIController m_ProjectList;

        [SerializeField]
        AssetInfoUIController m_AssetInfoUIController;

        [SerializeField]
        FilterUIController m_Filter;

        [SerializeField]
        SortUIController m_Sort;

        [SerializeField]
        TransformationWorkflowUIController m_TransformationWorkflowUIController;

        [SerializeField]
        SearchController m_SearchController;

        [SerializeField]
        string m_AssetManagerUrl = "https://cloud.unity.com/home";

        static readonly string k_LoadingIndicator = "LoadingIndicator";
        static readonly string k_ViewTypeGroup = "ViewTypeGroup";

        CircularProgress m_LoadingIndicator;
        ActionGroup m_ViewTypeGroup;
        VisualElement m_RootVisualElement;

        int m_LoadingCount;
        public VisualElement RootVisualElement => m_RootVisualElement;

        IAssetListController m_AssetListController;
        CancellationTokenSource m_RefreshCancellationTokenSource;

        [Inject]
        void Setup(IAssetListController assetListController)
        {
            m_AssetListController = assetListController;
        }

        void Awake()
        {
            m_Filter.StreamableFilterChanged += OnStreamableFilterChanged;

            m_AssetListController.OrganizationsPopulated += m_OrganizationList.Populate;
            m_AssetListController.OrganizationSelected += OnOrganizationSelected;
            m_AssetListController.ProjectSelected += OnProjectSelected;
            m_AssetListController.CollectionSelected += OnCollectionSelected;
            m_AssetListController.RefreshStarted += OnControllerRefreshStarted;
            m_AssetListController.HideContent += OnHide;
            m_AssetListController.Loading += OnLoading;

            m_OrganizationList.OrganizationSelected += OnOrganizationListSelected;

            m_AssetGrid.AssetSelected += OnGridAssetSelected;

            m_CollectionGrid.CollectionSelected += OnGridCollectionSelected;

            m_ProjectList.ProjectSelected += m_AssetListController.SelectProject;
            m_ProjectList.CollectionSelected += m_AssetListController.SelectCollection;

            m_Filter.FilterChanged += OnFilterChanged;

            m_AssetInfoUIController.OpenAssetButtonClicked += OnAssetInfoOpenAsset;
            m_AssetInfoUIController.ClosePanelButtonClicked += OnAssetInfoClosePanel;
            m_AssetInfoUIController.GenerateStreamableButtonClicked += OnAssetInfoGenerateStreamable;
            m_AssetInfoUIController.NeedProjectInfo += OnProjectInfoNeeded;
            m_SearchController.SearchValueChanged += OnSearchValueChanged;
        }

        void OnSearchValueChanged(string searchValue)
        {
            m_AssetGrid.UpdateWithSearchResult(searchValue);
        }

        void Start()
        {
            if (m_UIDocument != null)
            {
                InitUIToolkit(m_UIDocument.rootVisualElement);
            }

            m_Filter.SearchFilter = m_AssetListController.Filters;
        }

        void OnDestroy()
        {
            m_AssetListController.OrganizationsPopulated -= m_OrganizationList.Populate;
            m_AssetListController.OrganizationSelected -= OnOrganizationSelected;
            m_AssetListController.ProjectSelected -= OnProjectSelected;
            m_AssetListController.CollectionSelected -= OnCollectionSelected;
            m_AssetListController.RefreshStarted -= OnControllerRefreshStarted;
            m_AssetListController.HideContent -= OnHide;
            m_AssetListController.Loading -= OnLoading;

            m_OrganizationList.OrganizationSelected -= OnOrganizationListSelected;

            m_AssetGrid.AssetSelected -= OnGridAssetSelected;
            m_CollectionGrid.CollectionSelected -= OnGridCollectionSelected;

            m_ProjectList.ProjectSelected -= m_AssetListController.SelectProject;

            m_Filter.FilterChanged -= OnFilterChanged;

            m_AssetInfoUIController.OpenAssetButtonClicked -= OnAssetInfoOpenAsset;
            m_AssetInfoUIController.ClosePanelButtonClicked -= OnAssetInfoClosePanel;
            m_AssetInfoUIController.GenerateStreamableButtonClicked -= OnAssetInfoGenerateStreamable;
            m_AssetInfoUIController.NeedProjectInfo += OnProjectInfoNeeded;

            m_Filter.StreamableFilterChanged -= OnStreamableFilterChanged;
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_RootVisualElement = root;

            m_LoadingIndicator = root.Q<CircularProgress>(k_LoadingIndicator);
            m_ViewTypeGroup = root.Q<ActionGroup>(k_ViewTypeGroup);

            m_OrganizationList.InitUIToolkit(root);
            m_ProjectList.InitUIToolkit(root);
            m_AssetGrid.InitUIToolkit(root);
            m_CollectionGrid.InitUIToolkit(root);
            m_Filter.InitUIToolkit(root);
            m_Sort.InitUIToolkit(root);
            m_SearchController.InitUIToolkit(root);

            // Hide feature until it is implemented
            m_ViewTypeGroup.style.display = DisplayStyle.None;

            var listViewOptions = root.Q<Button>("ListViewOptions");
            listViewOptions.style.display = DisplayStyle.None;
            m_AssetInfoUIController.InitUIToolkit(root);
        }

        public void SetVisibility(bool visible)
        {
            if (m_UIDocument != null)
            {
                Utils.SetVisible(RootVisualElement, visible);
            }
        }

        void OnGridAssetSelected(IAsset asset)
        {
            if (m_AssetListController.HighlightAsset(asset))
            {
                if (m_AssetListController.SelectedProject != null)
                {
                    m_AssetInfoUIController.Show(asset,
                        m_AssetListController.CheckPermission(AssetListPermission.AssetOpen),
                        m_AssetListController.CheckPermission(AssetListPermission.AssetPublish));
                    return;
                }

                // Case where all projects is selected
                // Show asset info with disabled open button until access are checked
                m_AssetInfoUIController.Show(asset, false, false);
                _ = CheckPermissions(asset);
            }
        }

        void OnGridCollectionSelected(IAssetCollection collection)
        {
            m_AssetListController.SelectCollection(collection);
        }

        async Task CheckPermissions(IAsset asset)
        {
            await m_AssetListController.UpdatePermissionAsync(asset.Descriptor.ProjectId);
            if (m_AssetListController.SelectedAsset != null && m_AssetListController.SelectedAsset.Descriptor.AssetId == asset.Descriptor.AssetId) // If the asset is still selected, enable the open button
            {
                m_AssetInfoUIController.SetStreamableButtonState(m_AssetListController.CheckPermission(AssetListPermission.AssetPublish));

                var isStreamable = await StreamableAssetHelper.IsStreamable(asset);
                m_AssetInfoUIController.SetOpenButtonState(isStreamable && m_AssetListController.CheckPermission(AssetListPermission.AssetOpen));
            }
        }

        void OnControllerRefreshStarted()
        {
            Clear();
            SetVisibility(true);
        }

        void OnHide()
        {
            SetVisibility(false);
        }

        async void OnOrganizationListSelected(IOrganization organization)
        {
            try
            {
                OnLoading(true);
                await m_AssetListController.SelectOrganization(organization);
            }
            finally
            {
                OnLoading(false);
            }
        }

        void Clear()
        {
            m_ProjectList.Clear();
            m_AssetGrid.Clear();
            m_CollectionGrid.Clear();
            m_AssetInfoUIController.Close();
        }

        async Task OnOrganizationSelected(IOrganization organization)
        {
            m_OrganizationList.SelectOrganization(organization);

            Clear();

            try
            {
                OnLoading(true);
                var projects = await m_AssetListController.GetAllProjects();
                m_ProjectList.Populate(projects);
                m_AssetGrid.ShowNoProjectsWarning(!projects.Any());
            }
            finally
            {
                OnLoading(false);
            }
        }

        async void OnProjectSelected(IAssetProject project)
        {
            m_ProjectList.SelectProject(project);

            m_AssetInfoUIController.Close();
            m_Filter.Clear();

            await RefreshAsset();
        }

        void OnCollectionSelected(IAssetCollection collection)
        {
            m_ProjectList.SelectCollection(collection);
        }

        async void OnAssetInfoOpenAsset()
        {
            await Task.Delay(1);
            m_AssetListController.OpenStreamableAsset();
        }

        void OnAssetInfoClosePanel()
        {
            m_AssetGrid.ClearSelection();
        }

        void OnAssetInfoGenerateStreamable()
        {
            m_TransformationWorkflowUIController.ShowDialog(m_AssetListController.SelectedAsset);
        }

        void OnLoading(bool loading)
        {
            m_LoadingCount += loading ? 1 : -1;
            if (m_LoadingCount < 0)
            {
                m_LoadingCount = 0;
            }

            if (m_LoadingIndicator != null)
            {
                m_LoadingIndicator.visible = m_LoadingCount > 0;
            }
        }

        async void OnProjectInfoNeeded(ProjectDescriptor projectDescriptor)
        {
            var project = await m_AssetListController.GetProject(projectDescriptor);

            var projectId = project.Descriptor.ProjectId.ToString();
            var url = $"{m_AssetManagerUrl}/organizations/{project.Descriptor.OrganizationId}/projects/{projectId}/assets";
            _ = TextureController.GetProjectIcon(projectId, texture =>
            {
                m_AssetInfoUIController.SetButtonProjectInfo(project.Name, url, texture, TextureController.GetProjectIconColor(projectId));
            });
        }

        async void OnStreamableFilterChanged(bool filterStreamable)
        {
            if (filterStreamable)
            {
                StreamableAssetHelper.ApplyFilter(m_AssetListController.Filters);
            }
            else
            {
                StreamableAssetHelper.RemoveFilter(m_AssetListController.Filters);
            }

            await RefreshAsset();
        }

        void OnFilterChanged()
        {
            _ = RefreshAsset();
        }

        async Task RefreshAsset()
        {
            if (m_AssetListController.SelectedOrganization == null)
                return;

            try
            {
                OnLoading(true);
                m_AssetGrid.Clear();
                m_CollectionGrid.Clear();

                if (m_RefreshCancellationTokenSource != null)
                {
                    m_RefreshCancellationTokenSource.Cancel();
                    m_RefreshCancellationTokenSource.Dispose();
                }

                IAsyncEnumerable<IAsset> assets;
                if (m_AssetListController.SelectedProject == null)
                {
                    assets = await m_AssetListController.GetAssetsAcrossAllProjectsAsync();
                }
                else
                {
                    assets = m_AssetListController.GetAssetsAsync();
                }

                await foreach (var asset in assets)
                {
                    m_AssetGrid.Add(asset);
                    m_Filter.AddAsset(asset);
                }

                if (m_SearchController.CurrentSearchValue != null)
                {
                    m_AssetGrid.UpdateWithSearchResult(m_SearchController.CurrentSearchValue);
                }

                m_AssetGrid.CheckNoAssets();

                if (!m_AssetListController.IsCollectionSelected && m_AssetListController.SelectedProject != null)
                {
                    m_RefreshCancellationTokenSource = new CancellationTokenSource();

                    var collectionInfos = new List<AssetCollectionInfo>();
                    var filter = new AssetSearchFilter();

                    await foreach (var collection in m_AssetListController.SelectedProject.ListCollectionsAsync(Range.All, m_RefreshCancellationTokenSource.Token))
                    {
                        filter.Collections.WhereContains(collection.Descriptor.Path);
                        var assetCount = await m_AssetListController.SelectedProject.CountAssetsAsync(filter, m_RefreshCancellationTokenSource.Token);
                        collectionInfos.Add(new AssetCollectionInfo { Collection = collection, AssetCount = assetCount });
                    }

                    if (collectionInfos.Any())
                    {
                        m_CollectionGrid.Populate(collectionInfos);
                    }
                }
            }
            finally
            {
                OnLoading(false);
                m_RefreshCancellationTokenSource?.Cancel();
                m_RefreshCancellationTokenSource?.Dispose();
                m_RefreshCancellationTokenSource = null;
            }
        }

        public void BringToFront()
        {
            RootVisualElement.BringToFront();
        }
    }
}
