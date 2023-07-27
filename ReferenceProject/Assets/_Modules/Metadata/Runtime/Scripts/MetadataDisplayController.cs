using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.SearchSortFilter;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.Cloud.Common;
using Unity.Cloud.Metadata;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Messaging;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Metadata
{
    public class MetadataDisplayController : MonoBehaviour
    {
        static readonly string k_ListViewParameterList = "ParameterList";
        static readonly string k_TextListElementKey = "ParameterLabel";
        static readonly string k_TextListElementValue = "ParameterValue";
        static readonly string k_TextEmptySelection = "EmptySelection";
        static readonly string k_SearchBar = "search-input";
        static readonly string k_DropdownGroups = "group-dropdown";
        [SerializeField]
        VisualTreeAsset m_ListElementTemplate;

        [SerializeField]
        bool m_UseHover;

        [SerializeField]
        string m_StringAllInformation = "@MetadataDisplay:AllInformation";

        readonly List<MetadataList> m_CacheMetadataList = new();
        readonly List<MetadataList> m_MetadataList = new();

        FilterModule<MetadataList> m_FilterModule;
        FilterSingleUI<MetadataList> m_FilterSingleUI;
        Dropdown m_GroupsDropdown;
        HighlightModule m_HighlightModule;
        Text m_LabelEmptySelection;
        ListView m_ParameterListView;
        SearchBar m_SearchBar;
        SearchModule<MetadataList> m_SearchModule;
        SearchUI m_SearchUI;
        
        CancellationTokenSource m_CancellationTokenSource;
        
        MetadataProvider m_MetadataProvider;
        
        ISceneEvents m_SceneEvents;
        IServiceHttpClient m_ServiceHttpClient;
        IServiceHostResolver m_ServiceHostResolver;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        ObjectSelectionHighlightActivator m_ObjectSelectionHighlightActivator;
        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;
        IAppMessaging m_AppMessaging;

        [Inject]
        void Setup(ISceneEvents sceneEvents, IServiceHttpClient serviceHttpClient, IServiceHostResolver serviceHostResolver,
            PropertyValue<IObjectSelectionInfo> objectSelectionProperty, ObjectSelectionActivator objectSelectionActivator,
            ObjectSelectionHighlightActivator objectSelectionHighlightActivator,
            IAppMessaging appMessaging)
        {
            m_SceneEvents = sceneEvents;
            m_ServiceHttpClient = serviceHttpClient;
            m_ServiceHostResolver = serviceHostResolver;
            m_ObjectSelectionProperty = objectSelectionProperty;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_ObjectSelectionHighlightActivator = objectSelectionHighlightActivator;
            m_AppMessaging = appMessaging;
        }

        void Awake()
        {
            m_SceneEvents.SceneOpened += OnSceneOpened;
        }

        void OnDestroy()
        {
            m_SceneEvents.SceneOpened -= OnSceneOpened;
            m_SearchUI?.UnregisterCallbacks();
            m_FilterSingleUI?.UnregisterCallbacks();
        }

        public void OpenTool()
        {
            // Activate Selection tool
            if (m_ObjectSelectionActivator != null)
                m_ObjectSelectionActivator.Subscribe(this);
            
            // Activate selection highlighting
            if (m_ObjectSelectionHighlightActivator != null)
                m_ObjectSelectionHighlightActivator.Subscribe(this);

            else
                Debug.LogError($"Null reference to {nameof(ObjectSelectionActivator)} on {nameof(MetadataDisplayController)}");

            if (m_ObjectSelectionProperty != null)
            {
                m_ObjectSelectionProperty.ValueChanged -= OnSelectionChanged;
                m_ObjectSelectionProperty.ValueChanged += OnSelectionChanged;

                OnSelectionChanged(m_ObjectSelectionProperty.GetValue()); // Refresh panel
            }
        }

        public void CloseTool()
        {
            if (m_ObjectSelectionProperty != null)
                m_ObjectSelectionProperty.ValueChanged -= OnSelectionChanged;

            // Disable selection tool
            m_ObjectSelectionActivator?.Unsubscribe(this);
            
            // Disable selection highlighting
            m_ObjectSelectionHighlightActivator?.Unsubscribe(this);
        }

        public void Initialize(VisualElement rootVisualElement)
        {
            m_ParameterListView = rootVisualElement.Q<ListView>(k_ListViewParameterList);
            m_LabelEmptySelection = rootVisualElement.Q<Text>(k_TextEmptySelection);

            if (m_LabelEmptySelection == null)
                Debug.LogError($"Can't find Text with name {k_TextEmptySelection}");

            SetupList(m_ParameterListView, m_CacheMetadataList);

            // Search setup
            m_SearchModule = new SearchModule<MetadataList>(
                (nameof(MetadataList.Key), new SearchBindNode<MetadataList>((x) => x.Key))
            );

            // Search UI setup
            m_SearchUI = new SearchUI(m_SearchModule, rootVisualElement, OnRefresh, null, k_SearchBar);

            // HighlightModule setup
            m_HighlightModule = new HighlightModule(m_SearchModule);

            // Filter setup
            m_FilterModule = new FilterModule<MetadataList>(
                (nameof(MetadataList.Group), new FilterBindNode<MetadataList>(x => x.Group, FilterCompareType.Equals))
            );

            // Filter UI setup
            m_FilterSingleUI = new FilterSingleUI<MetadataList>(m_FilterModule, rootVisualElement, OnRefresh, null, m_StringAllInformation, k_DropdownGroups);

            OnRefresh();
        }

        async void OnSceneOpened(IScene scene)
        {
            if (scene == null)
                return;

            await OnSceneOpenedAsync(scene);
        }

        async Task OnSceneOpenedAsync(IScene scene)
        {
            await InitializeMetadataProviderAsync(scene);
        }

        Task InitializeMetadataProviderAsync(IScene scene)
        {
            m_MetadataProvider = new MetadataProvider(m_ServiceHttpClient, m_ServiceHostResolver, scene.Id.ToString(), scene.LatestVersion.ToString());
            return Task.CompletedTask;
        }
        
        void OnSelectionChanged(IObjectSelectionInfo gameObjectSelectionInfo) => SetInstanceId(gameObjectSelectionInfo?.SelectedInstanceId ?? InstanceId.None);

        void OnRefresh()
        {
            StartCoroutine(RefreshCoroutine());
        }
        
        IEnumerator RefreshCoroutine()
        {
            m_CacheMetadataList.Clear();
            m_CacheMetadataList.AddRange(m_MetadataList);

            // Make cancellation token
            var cancellationTokenSource = new CancellationTokenSource();
            
            // If there is an old cancellation token source then cancel it
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource = cancellationTokenSource;

            var m_Task = RefreshAsync(cancellationTokenSource.Token);
            yield return new WaitWhile(() => !m_Task.IsCompleted);
            
            // Manage token
            if (m_CancellationTokenSource == cancellationTokenSource)
            {
                m_CancellationTokenSource = null;
            }
            
            cancellationTokenSource.Dispose();
            
            // Show Exception if it exists
            if (m_Task.Exception != null)
            {
                Debug.LogError($"Exception: {m_Task.Exception.Message}");
            }
            
            // Show cancellation if it exists
            if (m_Task.IsCanceled)
            {
                Debug.LogWarning($"Operation has been canceled");
                yield break;
            }

            m_ParameterListView.RefreshItems();

            m_ParameterListView.style.display = new StyleEnum<DisplayStyle>(m_CacheMetadataList.Count != 0 ? DisplayStyle.Flex : DisplayStyle.None);
            m_LabelEmptySelection.style.display = new StyleEnum<DisplayStyle>(m_MetadataList != null && m_MetadataList.Count != 0 ? DisplayStyle.None : DisplayStyle.Flex);
        }

        async Task RefreshAsync(CancellationToken cancellationToken)
        {
            await m_FilterModule.PerformFiltering(m_CacheMetadataList, cancellationToken);
            await m_SearchModule.PerformSearch(m_CacheMetadataList, cancellationToken);
        }

        void SetInstanceId(InstanceId id)
        {
            var mainThreadContext = SynchronizationContext.Current;
            SetInstanceIdAsync(id, mainThreadContext).ConfigureAwait(false);
        }

        async Task SetInstanceIdAsync(InstanceId id, SynchronizationContext mainThreadContext)
        {
            m_MetadataList.Clear();

            if (id != InstanceId.None)
            {
                try
                {
                    var query = m_MetadataProvider.Query();
                    query.SelectAll();
                    query.IncludedIn(id);
                    var result = await query.ExecuteAsync();

                    var metadataObjectList = result[id];
                    foreach (var key in metadataObjectList.Keys)
                    {
                        if(metadataObjectList.TryGetValue(key, out var metadataContainer))
                        {
                            switch (metadataContainer)
                            {
                                case MetadataObject metadataObject:
                                    string group = metadataObject["group"].ToString();
                                    string value = metadataObject["value"].ToString();
                                    m_MetadataList.Add(new MetadataList{Key = key, Group = group, Value = value});
                                    break;
                                default:
                                    Debug.LogError($"Unexpected type {metadataContainer.GetType()} for key {key}");
                                    break;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    m_AppMessaging.ShowException(exception);
                }

                // Show msg if no metadata
                if (m_MetadataList.Count == 0)
                {
                    m_AppMessaging.ShowWarning("The selected object doesn't have metadata information.");
                }
            }
  
            // Execute UI changes on the main thread
            mainThreadContext.Post(_ =>
            {
                m_FilterSingleUI.SetFilterOptions(m_MetadataList);
                m_FilterSingleUI.SetDefaultValueWithoutNotify();

                OnRefresh();
            }, null);
        }

        void SetupList(ListView listView, List<MetadataList> itemsSourceList)
        {
            Func<VisualElement> makeItem = () =>
            {
                var element = m_ListElementTemplate.Instantiate();

                // Disable hover by inline style
                if (!m_UseHover)
                    element.style.backgroundColor = Color.clear;

                return element;
            };

            Action<VisualElement, int> bindItem = (element, i) =>
            {
                if (listView.itemsSource[i] is MetadataList data)
                {
                    var parameterLabel = element.Q<Text>(k_TextListElementKey);
                    var parameterValue = element.Q<Text>(k_TextListElementValue);

                    parameterLabel.text = m_HighlightModule.IsHighlighted(nameof(MetadataList.Key), data.Key).Item2;
                    parameterValue.text = data.Value;
                }
            };

            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
            listView.itemsSource = itemsSourceList;
        }

        class MetadataList
        {
            public string Group = string.Empty;
            public string Key = string.Empty;
            public string Value = string.Empty;
        }
    }
}
