using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using TextOverflow = UnityEngine.UIElements.TextOverflow;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public class FilterSingleUI<T>
    {
        readonly string k_DropdownText = "appui-dropdown__title";

        readonly string m_DefaultValue;

        readonly IFilterBindNode<T> m_FilterBindNode;
        readonly Dropdown m_Dropdown;

        List<string> m_Options;

        public FilterSingleUI(IFilterBindNode<T> filterBindNode, VisualElement root, Action onFilterChanged,
            List<T> dataList, string defaultValue = null, string dropDownKey = "FilterDropdown")
        {
            m_FilterBindNode = filterBindNode;
            OnDropDownChanged += onFilterChanged;

            m_Dropdown = root.Q<Dropdown>(dropDownKey);

            if (m_Dropdown == null)
            {
                Debug.LogError($"Can't find {nameof(Dropdown)} with name {dropDownKey} at children of {nameof(VisualElement)}. {GetType().Name} has been disabled.");
                return;
            }

            // Make dropdown text elided
            var dropdownText = m_Dropdown.Q<LocalizedTextElement>(k_DropdownText);
            if (dropdownText != null)
            {
                dropdownText.style.textOverflow = new StyleEnum<TextOverflow>(TextOverflow.Ellipsis);
                dropdownText.style.whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.NoWrap);
                dropdownText.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
            }

            m_Dropdown.bindItem = (item, i) =>
            {
                if (i < m_Dropdown.sourceItems.Count)
                {
                    item.label = (string)m_Dropdown.sourceItems[i];
                }
            };

            if (!string.IsNullOrEmpty(defaultValue))
                m_DefaultValue = defaultValue;

            SetFilterOptions(dataList);

            m_Dropdown.SetValueWithoutNotify(0);
            m_Dropdown.RegisterValueChangedCallback(OnDropDownValueChanged);
        }

        public event Action OnDropDownChanged;

        public void UnregisterCallbacks()
        {
            if (m_Dropdown != null)
                m_Dropdown.UnregisterValueChangedCallback(OnDropDownValueChanged);
        }

        void OnDropDownValueChanged(ChangeEvent<int> evt)
        {
            if (m_Options == null || evt.newValue >= m_Options.Count)
                return;

            m_FilterBindNode.SelectedOption =
                m_Options[evt.newValue].Equals(m_DefaultValue) ? null : m_Options[evt.newValue];
            OnDropDownChanged?.Invoke();
        }

        public void SetDefaultValueWithoutNotify()
        {
            m_FilterBindNode.SelectedOption = null;
            m_Dropdown.SetValueWithoutNotify(0);
        }

        public void SetFilterOptions(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                SetDropdownOptions(null);
                return;
            }

            var bindPath = m_FilterBindNode.bindPath;

            var filterOptions = new HashSet<string>();

            foreach (var item in list)
            {
                var line = bindPath(item);
                if(!string.IsNullOrEmpty(line))
                    filterOptions.Add(line);
            }

            SetDropdownOptions(filterOptions.ToList());
        }

        void SetDropdownOptions(List<string> options)
        {
            m_Options = options ?? new List<string>();

            if (m_Options.Count > 0 && m_Options[0] != m_DefaultValue || m_Options.Count == 0)
                m_Options.Insert(0, m_DefaultValue);

            if (m_Dropdown == null)
                return;

            m_Dropdown.sourceItems = m_Options;
        }
    }
}
