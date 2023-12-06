using System;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetListUIController : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        AssetGridUIController m_AssetGrid;

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

        IAssetListController m_AssetListController;

        readonly string k_LoadingIndicator = "LoadingIndicator";
        readonly string k_ViewTypeGroup = "ViewTypeGroup";

        CircularProgress m_LoadingIndicator;
        ActionGroup m_ViewTypeGroup;

        int m_LoadingCount;

        public VisualElement RootVisualElement => m_UIDocument != null ? m_UIDocument.rootVisualElement : null;

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
            m_AssetListController.RefreshStarted += OnControllerRefreshStarted;
            m_AssetListController.HideContent += OnHide;
            m_AssetListController.Loading += OnLoading;
            m_AssetListController.AllProjects += OnAllProject;

            m_OrganizationList.OrganizationSelected += OnOrganizationListSelected;

            m_AssetGrid.AssetSelected += OnGridAssetSelected;

            m_ProjectList.AllProjectSelected += OnAllProjectSelected;
            m_ProjectList.ProjectSelected += m_AssetListController.SelectProject;

            m_AssetInfoUIController.OpenAssetButtonClicked += OnAssetInfoOpenAsset;
            m_AssetInfoUIController.ClosePanelButtonClicked += OnAssetInfoClosePanel;
            m_AssetInfoUIController.GenerateStreamableButtonClicked += OnAssetInfoGenerateStreamable;
        }

        void Start()
        {
            if (m_UIDocument != null)
            {
                InitUIToolkit(m_UIDocument.rootVisualElement);
            }
        }

        void OnDestroy()
        {
            m_AssetListController.OrganizationsPopulated -= m_OrganizationList.Populate;
            m_AssetListController.OrganizationSelected -= OnOrganizationSelected;
            m_AssetListController.ProjectSelected -= OnProjectSelected;
            m_AssetListController.RefreshStarted -= OnControllerRefreshStarted;
            m_AssetListController.HideContent -= OnHide;
            m_AssetListController.Loading -= OnLoading;

            m_OrganizationList.OrganizationSelected -= OnOrganizationListSelected;

            m_AssetGrid.AssetSelected -= OnGridAssetSelected;

            m_ProjectList.AllProjectSelected -= OnAllProjectSelected;
            m_ProjectList.ProjectSelected -= m_AssetListController.SelectProject;

            m_AssetInfoUIController.OpenAssetButtonClicked -= OnAssetInfoOpenAsset;
            m_AssetInfoUIController.ClosePanelButtonClicked -= OnAssetInfoClosePanel;

            m_Filter.StreamableFilterChanged -= OnStreamableFilterChanged;
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_LoadingIndicator = root.Q<CircularProgress>(k_LoadingIndicator);
            m_ViewTypeGroup = root.Q<ActionGroup>(k_ViewTypeGroup);

            m_OrganizationList.InitUIToolkit(root);
            m_ProjectList.InitUIToolkit(root);
            m_AssetGrid.InitUIToolkit(root);
            m_Filter.InitUIToolkit(root);
            m_Sort.InitUIToolkit(root);

            // Hide feature until it is implemented
            m_ViewTypeGroup.style.display = DisplayStyle.None;

            var listViewOptions = root.Q<Button>("ListViewOptions");
            listViewOptions.style.display = DisplayStyle.None;
            m_AssetInfoUIController.InitUIToolkit(root);

            var search = root.Q<SearchBar>("SearchBar");
            search.style.display = DisplayStyle.None;
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
                m_AssetInfoUIController.Show(asset);
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

            if (project == null)
            {
                m_AssetGrid.Clear();
                return;
            }

            await RefreshAsset();
            m_AssetInfoUIController.SelectedProjectName = project.Name;
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

        async Task LoadAllAssets()
        {
            try
            {
                OnLoading(true);
                await m_AssetGrid.Populate(await m_AssetListController.GetAssetsAcrossAllProjectsAsync());
            }
            finally
            {
                OnLoading(false);
            }
        }

        async void OnAllProject()
        {
            m_ProjectList.SelectAllProjectButton();
            await LoadAllAssets();
        }

        async void OnAllProjectSelected()
        {
            m_AssetGrid.Clear();
            m_AssetInfoUIController.Close();
            await LoadAllAssets();
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

        async Task RefreshAsset()
        {
            if (m_AssetListController.SelectedOrganization == null)
                return;
            
            try
            {
                OnLoading(true);
                
                if (m_AssetListController.SelectedProject == null)
                {
                    await m_AssetGrid.Populate(await m_AssetListController.GetAssetsAcrossAllProjectsAsync());
                }
                else
                {
                    await m_AssetGrid.Populate(m_AssetListController.GetAssetsAsync());
                }
            }
            finally
            {
                OnLoading(false);
            }
        }
        
        public void BringToFront()
        {
            RootVisualElement.BringToFront();
        }
    }
}
