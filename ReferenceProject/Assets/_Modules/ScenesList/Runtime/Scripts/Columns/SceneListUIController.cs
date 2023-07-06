using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.ScenesList
{
    public class SceneListUIController : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        // TODO: remove this because in VR we do not have uiDocument reference
        public UIDocument UIDocument => m_UIDocument;

        Button m_RefreshButton;

        VisualElement m_RootVisualElement;

        readonly List<object> m_PrimaryKeyData = new();

        IMGUIContainer m_CurrentDisplayArrow;

        VisualElement m_LoadingIndicator;
        TableListView m_Table;

        ListTableServiceController m_ListTableServiceController;

        [SerializeField]
        float m_ScrollFactor = 100;

        [SerializeField]
        string[] m_HeaderStyles;

        [SerializeField]
        string[] m_RowStyles;

        SceneWorkspaceProvider m_SceneWorkspaceProvider;

        Task m_Task;

        static readonly string k_RefreshButton = "refresh-button";
        static readonly string k_LoadingIndicator = "loading";
        static readonly string k_Table = "table-scene-list";

        public event Action<IScene> ProjectSelected;

        [Inject]
        public void Setup(SceneWorkspaceProvider sceneWorkspaceProvider)
        {
            m_SceneWorkspaceProvider = sceneWorkspaceProvider;
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
            if (m_RefreshButton != null)
            {
                m_RefreshButton.clicked -= Refresh;
            }

            if (m_Table != null)
            {
                m_Table.itemClicked -= OnItemClicked;
            }
        }

        public void InitUIToolkit(VisualElement rootVisualElement)
        {
            if (rootVisualElement == null)
                return;

            m_RootVisualElement = rootVisualElement;

            // UIToolkit
            m_RefreshButton = m_RootVisualElement.Q<Button>(k_RefreshButton);
            m_LoadingIndicator = m_RootVisualElement.Q<VisualElement>(k_LoadingIndicator);
            m_Table = m_RootVisualElement.Q<TableListView>(k_Table);

            if (m_RefreshButton == null)
            {
                Debug.LogError($"Can't find {nameof(Button)} with name: {k_RefreshButton} at root: {rootVisualElement.name}");
                return;
            }

            if (m_LoadingIndicator == null)
            {
                Debug.LogError($"Can't find {nameof(VisualElement)} with name: {k_LoadingIndicator} at root: {rootVisualElement.name}");
                return;
            }

            if (m_Table == null)
            {
                Debug.LogError($"Can't find {nameof(TableListView)} with name: {k_Table} at root: {rootVisualElement.name}");
                return;
            }

            m_RefreshButton.clicked += Refresh;

            m_Table.itemClicked += OnItemClicked;

            var scrollView = m_Table.Q<ScrollView>();
            m_Table.ListView.RegisterCallback<WheelEvent>((evt) =>
                {
                    scrollView.scrollOffset = new Vector2(0, scrollView.scrollOffset.y + m_ScrollFactor * evt.delta.y);
                    evt.StopPropagation();
                }
            );

            m_ListTableServiceController = new ListTableServiceController(this, GetComponents<IService>());
            m_ListTableServiceController.Initialize(m_RootVisualElement, m_Table, m_LoadingIndicator, AllColumns);
            m_ListTableServiceController.itemsSource = m_PrimaryKeyData;

            foreach (var headerStyle in m_HeaderStyles)
            {
                m_Table.AddStyleToHeader(headerStyle);
            }

            foreach (var rowStyle in m_RowStyles)
            {
                m_Table.AddStyleToRow(rowStyle);
            }
        }

        void OnItemClicked(object clickedItem)
        {
            if(clickedItem is not IScene scene)
                return;
            
            ProjectSelected?.Invoke(scene);
        }

        IColumnEventData[] AllColumns
        {
            get
            {
                var columns = new List<IColumnEventData>();
                var extraColumns = GetComponents<IExtraColumn>();
                foreach (var extraColumn in extraColumns)
                {
                    columns.Add(extraColumn.Column);
                }

                return columns.ToArray();
            }
        }

        public void Refresh()
        {
            StartCoroutine(UpdateData());
        }

        public void RefreshOnlyTable()
        {
            OnPrimaryKeyUpdate();
        }
        
        public void SetVisibility(bool isVisible)
        {
            SetVisibility(m_RootVisualElement, isVisible);
        }

        IEnumerator UpdateData()
        {
            if (m_Task != null && !m_Task.IsCompleted)
            {
                yield break;
            }
            
            SetVisibility(m_LoadingIndicator, true);  // Show loading indicator
            SetVisibility(m_Table, false);  // Hide list
            
            m_Task = RefreshScenesAndWorkspacesAsync();
            yield return new WaitWhile(() => !m_Task.IsCompleted);

            // Hide loading indicator
            SetVisibility(m_LoadingIndicator, false);
            SetVisibility(m_Table, true); // Hide list

            OnPrimaryKeyUpdate();
        }

        async Task RefreshScenesAndWorkspacesAsync()
        {
            try
            {
                await m_SceneWorkspaceProvider.RefreshAsync();
            }
            catch (System.Threading.ThreadAbortException e)
            {
                Debug.LogWarning($"ThreadAbortException: {e}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void OnPrimaryKeyUpdate() //string workspace
        {
            m_PrimaryKeyData.Clear();
            m_PrimaryKeyData.AddRange(m_SceneWorkspaceProvider.GetAllScenes());
            m_ListTableServiceController.itemsSource = m_PrimaryKeyData;

            m_ListTableServiceController.RefreshServices();
        }

        static void SetVisibility(VisualElement visualElement, bool isVisible)
        {
            if (visualElement != null)
            {
                visualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
