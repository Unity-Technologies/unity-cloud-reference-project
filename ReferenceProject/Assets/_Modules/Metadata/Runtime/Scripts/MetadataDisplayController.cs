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
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Metadata;
using Unity.ReferenceProject.AssetManager;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Messaging;
using UnityEngine.UIElements;
using Zenject;
using Utils = Unity.ReferenceProject.Common.Utils;

namespace Unity.ReferenceProject.Metadata
{
    public class MetadataDisplayController : MonoBehaviour
    {
        static readonly string k_ListViewParameterList = "ParameterList";
        static readonly string k_TextListElementKey = "ParameterLabel";
        static readonly string k_TextListElementValue = "ParameterValue";
        static readonly string k_TextEmptySelection = "EmptySelection";
        static readonly string k_NoMetadataContainer = "NoMetadataContainer";
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
        VisualElement m_NoMetadataContainer;
        ListView m_ParameterListView;
        SearchBar m_SearchBar;
        SearchModule<MetadataList> m_SearchModule;
        SearchUI m_SearchUI;
        bool m_HasSelected;

        CancellationTokenSource m_CancellationTokenSource;

        IMetadataRepository m_MetadataRepository;

        IAssetEvents m_AssetEvents;
        IServiceHttpClient m_ServiceHttpClient;
        IServiceHostResolver m_ServiceHostResolver;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        ObjectSelectionHighlightActivator m_ObjectSelectionHighlightActivator;
        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;
        IAppMessaging m_AppMessaging;

        [Inject]
        void Setup(IAssetEvents assetEvents, IServiceHttpClient serviceHttpClient, IServiceHostResolver serviceHostResolver,
            PropertyValue<IObjectSelectionInfo> objectSelectionProperty, ObjectSelectionActivator objectSelectionActivator,
            ObjectSelectionHighlightActivator objectSelectionHighlightActivator, IAppMessaging appMessaging)
        {
            m_AssetEvents = assetEvents;
            m_ServiceHttpClient = serviceHttpClient;
            m_ServiceHostResolver = serviceHostResolver;
            m_ObjectSelectionProperty = objectSelectionProperty;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_ObjectSelectionHighlightActivator = objectSelectionHighlightActivator;
            m_AppMessaging = appMessaging;
        }

        void Awake()
        {
            m_AssetEvents.AssetLoaded += OnAssetLoaded;
        }

        void OnDestroy()
        {
            m_AssetEvents.AssetLoaded -= OnAssetLoaded;
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
            m_HasSelected = false;

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
            {
                Debug.LogError($"Can't find Text with name {k_TextEmptySelection}");
            }

            m_NoMetadataContainer = rootVisualElement.Q(k_NoMetadataContainer);
            if (m_NoMetadataContainer == null)
            {
                Debug.LogError($"Can't find element with name {k_NoMetadataContainer}");
            }

            SetupList(m_ParameterListView, m_CacheMetadataList);

            // Search setup
            m_SearchModule = new SearchModule<MetadataList>(
                (nameof(MetadataList.Key), new SearchBindNode<MetadataList>((x) => x.Key)),
                (nameof(MetadataList.Value), new SearchBindNode<MetadataList>((x) => x.Value))
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

        async void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            var data = dataset.Descriptor;
            await OnAssetLoadedAsync(data.ProjectId, data.AssetId, data.DatasetId);
        }

        async Task OnAssetLoadedAsync(ProjectId projectId, AssetId assetId, DatasetId datasetId)
        {
            await InitializeMetadataProviderAsync(projectId, assetId, datasetId);
        }

        Task InitializeMetadataProviderAsync(ProjectId projectId, AssetId assetId, DatasetId datasetId)
        {
            m_MetadataRepository = new MetadataRepository(m_ServiceHttpClient, m_ServiceHostResolver, projectId, assetId, datasetId);
            return Task.CompletedTask;
        }

        void OnSelectionChanged(IObjectSelectionInfo gameObjectSelectionInfo)
        {
            var instanceId = gameObjectSelectionInfo?.SelectedInstanceId ?? InstanceId.None;
            m_HasSelected = gameObjectSelectionInfo?.HasIntersected ?? false;
            SetInstanceId(instanceId);
        }

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

            var task = RefreshAsync(cancellationTokenSource.Token);
            yield return new WaitWhile(() => !task.IsCompleted);

            // Manage token
            if (m_CancellationTokenSource == cancellationTokenSource)
            {
                m_CancellationTokenSource = null;
            }

            cancellationTokenSource.Dispose();

            // Show Exception if it exists
            if (task.Exception != null)
            {
                Debug.LogError($"Exception: {task.Exception.Message}");
            }

            // Show cancellation if it exists
            if (task.IsCanceled)
            {
                Debug.LogWarning($"Operation has been canceled");
                yield break;
            }

            m_ParameterListView.RefreshItems();
            bool hasMetadata = m_MetadataList != null && m_MetadataList.Count != 0;
            Utils.SetVisible(m_ParameterListView, m_CacheMetadataList.Count != 0);
            if (m_LabelEmptySelection != null)
            {
                Utils.SetVisible(m_LabelEmptySelection, !m_HasSelected);
            }

            if (m_NoMetadataContainer != null)
            {
                Utils.SetVisible(m_NoMetadataContainer, m_HasSelected && !hasMetadata);
            }
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
                    await QueryMetadataForInstance(id);
                }
                catch (Exception exception)
                {
                    m_AppMessaging.ShowException(exception);
                }

                // Show msg if no metadata
                if (m_MetadataList.Count == 0)
                {
                    m_AppMessaging.ShowWarning("@MetadataDisplay:NoMetadata");
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

        async Task QueryMetadataForInstance(InstanceId id)
        {
            var query = m_MetadataRepository.Query();
            query.SelectAll();
            query.IncludedIn(id);
            var result = await query.ExecuteAsync();

            if (!result.TryGetValue(id, out MetadataObject metadataObjectList))
                return;

            foreach (var key in metadataObjectList.Keys)
            {
                if (metadataObjectList.TryGetValue(key, out var metadataContainer))
                {
                    switch (metadataContainer)
                    {
                        case MetadataObject metadataObject:
                            string group = metadataObject["group"].ToString();
                            string value = metadataObject["value"].ToString();
                            m_MetadataList.Add(new MetadataList { Key = key, Group = group, Value = value });
                            break;
                        default:
                            Debug.LogError($"Unexpected type {metadataContainer.GetType()} for key {key}");
                            break;
                    }
                }
            }
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
                    parameterValue.text = m_HighlightModule.IsHighlighted(nameof(MetadataList.Value), data.Value).Item2;
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
