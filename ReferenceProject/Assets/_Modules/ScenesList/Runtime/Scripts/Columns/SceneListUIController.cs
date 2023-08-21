using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        static readonly string k_RefreshButton = "refresh-button";
        static readonly string k_LoadingIndicator = "loading";
        static readonly string k_Table = "table-scene-list";

        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        float m_ScrollFactor = 100;

        [SerializeField]
        string[] m_HeaderStyles;

        [SerializeField]
        string[] m_RowStyles;

        // TODO: remove this because in VR we do not have uiDocument reference
        public UIDocument UIDocument => m_UIDocument;

        Button m_RefreshButton;

        VisualElement m_RootVisualElement;

        readonly List<IScene> m_PrimaryKeyData = new();

        IMGUIContainer m_CurrentDisplayArrow;

        VisualElement m_LoadingIndicator;
        TableListView m_Table;

        ListTableServiceController m_ListTableServiceController;

        SceneWorkspaceProvider m_SceneWorkspaceProvider;

        Task m_Task;

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
            var container = m_RootVisualElement.Q<VisualElement>("Container");
            container.AddToClassList("scene-list-container");

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
            m_ListTableServiceController.Initialize(m_RootVisualElement, m_Table, AllColumns);
            m_ListTableServiceController.itemsSource = default;

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
                var extraColumns = GetComponents<TableListColumn>();
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

        public void SetVisibility(bool isVisible)
        {
            SetVisibility(m_RootVisualElement, isVisible);
        }

        IEnumerator UpdateData()
        {
            if (m_Task is { IsCompleted: false })
            {
                yield break;
            }
            
            // Refresh current data before we make request to source.
            // Could be data that is not only at the source (eg: local saved data) to show it immediately
            m_Table.RefreshItems();

            SetVisibility(m_LoadingIndicator, true);
            
            // Request new data
            m_Task = RefreshScenesAndWorkspacesAsync();
            yield return new WaitWhile(() => !m_Task.IsCompleted);

            if (ChangesAtSource())
            {
                ClearAndUpdateUI();
            }

            SetVisibility(m_LoadingIndicator, false);
        }

        bool ChangesAtSource()
        {
            if (!Enumerable.SequenceEqual(m_SceneWorkspaceProvider.GetAllScenes(), m_PrimaryKeyData, new SceneComparer()))
                return true;
            return false;
        }

        async Task RefreshScenesAndWorkspacesAsync()
        {
            try
            {
                await m_SceneWorkspaceProvider?.RefreshAsync();
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

        void ClearAndUpdateUI()
        {
            m_PrimaryKeyData.Clear();
            m_PrimaryKeyData.AddRange(m_SceneWorkspaceProvider.GetAllScenes());
            m_ListTableServiceController.itemsSource = m_PrimaryKeyData.Select(x => (object)x).ToList();

            m_ListTableServiceController.RefreshServices();
        }

        static void SetVisibility(VisualElement visualElement, bool isVisible)
        {
            if (visualElement != null)
            {
                visualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void SetBackground(bool toolIsOpen)
        {
            if (toolIsOpen)
            {
                m_RootVisualElement.AddToClassList("scene-list-background");
            }
            else
            {
                m_RootVisualElement.RemoveFromClassList("scene-list-background");
            }
        }
    }
}
