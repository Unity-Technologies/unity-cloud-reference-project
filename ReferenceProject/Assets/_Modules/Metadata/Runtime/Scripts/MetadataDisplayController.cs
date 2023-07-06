using System;
using System.Collections.Generic;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.SearchSortFilter;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Unity.AppUI.UI;
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

        FilterBindNode<MetadataList> m_FilterModule;
        FilterSingleUI<MetadataList> m_FilterSingleUI;
        Dropdown m_GroupsDropdown;
        HighlightModule m_HighlightModule;

        Text m_LabelEmptySelection;

        ObjectSelectionActivator m_ObjectSelectionActivator;

        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;

        ListView m_ParameterListView;
        SearchBar m_SearchBar;

        SearchModule<MetadataList> m_SearchModule;

        SearchUI m_SearchUI;

        [Inject]
        void Setup(PropertyValue<IObjectSelectionInfo> objectSelectionProperty, ObjectSelectionActivator objectSelectionActivator)
        {
            m_ObjectSelectionProperty = objectSelectionProperty;
            m_ObjectSelectionActivator = objectSelectionActivator;
        }

        void OnDestroy()
        {
            m_SearchUI?.UnregisterCallbacks();
            m_FilterSingleUI?.UnregisterCallbacks();
        }

        public void OpenTool()
        {
            // Activate Selection tool
            if (m_ObjectSelectionActivator != null)
                m_ObjectSelectionActivator.Subscribe(this);
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
            m_FilterModule = new FilterBindNode<MetadataList>(x => x.Group, FilterCompareType.Equals);

            // Filter UI setup
            m_FilterSingleUI = new FilterSingleUI<MetadataList>(m_FilterModule, rootVisualElement, OnRefresh, null, m_StringAllInformation, k_DropdownGroups);

            OnRefresh();
        }

        void OnSelectionChanged(IObjectSelectionInfo gameObjectSelectionInfo) => SetGameObject(gameObjectSelectionInfo?.SelectedGameObject);

        void OnRefresh()
        {
            m_CacheMetadataList.Clear();
            m_CacheMetadataList.AddRange(m_MetadataList);

            m_FilterModule.PerformFiltering(m_CacheMetadataList);
            m_SearchModule.PerformSearch(m_CacheMetadataList);

            m_ParameterListView.RefreshItems();

            m_ParameterListView.style.display = new StyleEnum<DisplayStyle>(m_CacheMetadataList.Count != 0 ? DisplayStyle.Flex : DisplayStyle.None);

            m_LabelEmptySelection.style.display = new StyleEnum<DisplayStyle>(m_MetadataList != null && m_MetadataList.Count != 0 ? DisplayStyle.None : DisplayStyle.Flex);
        }

        void SetGameObject(GameObject target)
        {
            m_MetadataList.Clear();

            if (target != null)
            {
                // TODO: use Metadata package
                Debug.LogWarning("No metadata on selection");
            }

            m_FilterSingleUI.SetFilterOptions(m_MetadataList);
            m_FilterSingleUI.SetDefaultValueWithoutNotify();

            OnRefresh();
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
