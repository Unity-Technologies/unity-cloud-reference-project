using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class FilterUIController : MonoBehaviour
    {
        [SerializeField]
        AssetListFilterData m_FilterData;

        [SerializeField]
        VisualTreeAsset m_FilterSelectionTemplate;

        [SerializeField]
        StyleSheet m_StyleSheet;

        VisualElement m_FilterChipContainer;
        VisualElement m_FilterTypeList;
        ActionButton m_StreamableButton;
        Button m_FilterButton;
        Button m_ClearAll;
        Button m_ApplyButton;
        Popover m_FilterTypePopover;

        readonly List<IAsset> m_Assets = new();
        readonly List<IFilter> m_Filters = new();

        static readonly string k_StreamableKey = "StreamableFilter";
        public static readonly string k_LocalizedAssetList = "@AssetList:";

        public AssetSearchFilter SearchFilter { get; set; }

        public event Action<bool> StreamableFilterChanged;
        public event Action FilterChanged;

        void Start()
        {
            var filterTypeList = m_FilterData.FilterTypes.Where(f => f.IsSelected);
            foreach (var filterType in filterTypeList)
            {
                var filter = FilterCreator.Create(filterType);
                filter.ItemChosen += OnItemChosen;
                m_Filters.Add(filter);
            }
        }

        void OnDestroy()
        {
            foreach (var filter in m_Filters)
            {
                filter.ItemChosen -= OnItemChosen;
            }
            m_Filters.Clear();
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_FilterChipContainer = root.Q<VisualElement>("FilterChipContainer");

            m_StreamableButton = root.Q<ActionButton>("FilterStreamableButton");
            m_StreamableButton.clicked += OnStreamableButtonClicked;

            m_FilterButton = root.Q<Button>("FilterButton");
            m_FilterButton.clicked += OnFilterButtonClicked;

            m_ClearAll = root.Q<Button>("ClearFilterButton");
            Utils.SetVisible(m_ClearAll, false);
            m_ClearAll.clicked += () =>
            {
                Clear();
                FilterChanged?.Invoke();
            };

            m_FilterTypeList = new VisualElement();
            m_FilterTypeList.styleSheets.Add(m_StyleSheet);
            m_FilterTypeList.AddToClassList("container__filter-type-list");

            foreach (var filter in m_Filters)
            {
                var filterTypeButton = new Button
                {
                    title = filter.Label
                };
                filterTypeButton.AddToClassList("button__filter-type-list-item");
                filterTypeButton.clicked += () =>
                {
                    m_FilterTypePopover?.Dismiss();
                    Utils.SetVisible(filterTypeButton, false);
                    var filterChip = new Chip
                    {
                        label = filter.Label,
                        deleteIcon = "x"
                    };
                    filterChip.AddToClassList("chip__filter");
                    filterChip.deleted += (_,_) =>
                    {
                        Utils.SetVisible(filterTypeButton, true);
                        m_FilterChipContainer.Remove(filterChip);
                        Utils.SetVisible(m_ClearAll, m_FilterChipContainer.Children().Any());

                        // If nothing is selected, don't need to clear the filter
                        if (filter.IsUsed)
                        {
                            filter.ClearFilter(SearchFilter);
                            FilterChanged?.Invoke();
                        }
                    };
                    filterChip.clicked += (_,_) =>
                    {
                        FilterChipClicked(filter, filterChip);
                    };
                    m_FilterChipContainer.Add(filterChip);
                    Utils.SetVisible(m_ClearAll, true);
                    FilterChipClicked(filter, filterChip);
                };
                m_FilterTypeList.Add(filterTypeButton);
            }

            var streamableFilter = PlayerPrefs.GetInt(k_StreamableKey, 1);
            if (streamableFilter == 1)
            {
                m_StreamableButton.selected = true;
                StreamableFilterChanged?.Invoke(true);
            }
        }

        public void Clear()
        {
            m_Assets.Clear();
            m_FilterChipContainer.Clear();
            foreach (var button in m_FilterTypeList.Children())
            {
                Utils.SetVisible(button, true);
            }

            foreach (var filter in m_Filters)
            {
                filter.ClearFilter(SearchFilter);
            }

            Utils.SetVisible(m_ClearAll, false);
        }

        public void AddAsset(IAsset asset)
        {
            m_Assets.Add(asset);
        }

        void FilterChipClicked(IFilter filter, Chip filterChip)
        {
            VisualElement popoverContent = m_FilterSelectionTemplate.CloneTree()?.Q("RootVisualElement");
            m_ApplyButton = popoverContent?.Q<Button>("ApplyButton");
            var selectionContent = popoverContent?.Q("SelectionContent");

            filter.FillContent(selectionContent, m_Assets, SearchFilter);

            popoverContent?.styleSheets.Add(m_StyleSheet);

            var popover = Popover.Build(filterChip, popoverContent)
                .SetPlacement(PopoverPlacement.Bottom);
            popover.Show();

            m_ApplyButton.SetEnabled(false);
            m_ApplyButton.clicked += () =>
            {
                filter.ApplyFilter(SearchFilter, filterChip);

                popover?.Dismiss();
                FilterChanged?.Invoke();
            };
        }

        void OnItemChosen()
        {
            m_ApplyButton?.SetEnabled(true);
        }

        void OnFilterButtonClicked()
        {
            m_FilterTypePopover = Popover.Build(m_FilterButton, m_FilterTypeList).SetPlacement(PopoverPlacement.Bottom);
            m_FilterTypePopover.Show();
        }

        void OnStreamableButtonClicked()
        {
            m_StreamableButton.selected = !m_StreamableButton.selected;
            StreamableFilterChanged?.Invoke(m_StreamableButton.selected);
            PlayerPrefs.SetInt(k_StreamableKey, m_StreamableButton.selected ? 1 : 0);
        }
    }
}
