using System;
using System.Collections.Generic;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Clickable = Unity.AppUI.UI.Clickable;
using TextOverflow = UnityEngine.UIElements.TextOverflow;

namespace Unity.ReferenceProject.UITableListView
{
    public class FilterTableUI<T>
    {
        TableListView m_Table;
        VisualElement m_ContentContainer;

        readonly ActionButton m_Button;

        readonly FilterModule<T> m_FilterModule;
        readonly Action m_OnFilterChanged;

        readonly Dictionary<Checkbox, (string, int)> m_CheckboxMap = new();
        readonly Dictionary<string, Checkbox> m_HeaderCheckBoxMap = new();

        readonly Dictionary<string, List<string>> m_AllOptions = new();

        readonly List<int> m_PrimaryKey = new();

        readonly string[] m_ColumnStyles;

        public FilterTableUI(FilterModule<T> filterBindNode, VisualElement root, Action onFilterChanged,
            string buttonName = "", string[] columnStyles = null)
        {
            m_FilterModule = filterBindNode;
            m_OnFilterChanged = onFilterChanged;
            CreateTable();

            m_Button = root.Q<ActionButton>(buttonName);
            if (m_Button == null)
            {
                Debug.LogError($"Can't find {nameof(ActionButton)} with name: {buttonName} in {root.name}");
                return;
            }

            m_ColumnStyles = columnStyles;

            m_Button.clickable.clicked += OnShowFilterPopover;
        }

        void OnReset()
        {
            m_HeaderCheckBoxMap.Clear();
            m_CheckboxMap.Clear();
        }

        public void Unsubscribe()
        {
            if (m_Button != null)
            {
                m_Button.selected = false;
                m_Button.clickable.clicked -= OnShowFilterPopover;
            }
        }

        void CreateTable()
        {
            m_ContentContainer = new VisualElement();
            m_Table = new TableListView();
            m_Table.ListView.style.maxHeight = new StyleLength(StyleKeyword.None); // Because there is a bug with popover. When shadow element of popover takes this value and become 4k height
            m_Table.showTableHeader = true;
            m_ContentContainer.Add(m_Table);
        }

        void OnShowFilterPopover()
        {
            var popover = Popover.Build(m_Button, m_ContentContainer).SetPlacement(PopoverPlacement.LeftTop);
            RefreshHeadersState();
            m_Table.RefreshItems();

            m_Button.selected = true;
            popover.dismissed += (_, _) => m_Button.selected = false;
            popover.Show();
        }

        public void UpdateColumns(List<T> dataList, int inlineWidth = 0)
        {
            if (dataList == null || dataList.Count == 0)
                return;

            CreatePossibleOptions(dataList);
            var columns = new TableListColumnData[m_AllOptions.Count];
            var i = 0;
            foreach (var item in m_AllOptions)
            {
                var column = new TableListColumnData(item.Key, true, inlineWidth > 0, inlineWidth, m_ColumnStyles);
                column.MakeCell += OnMakeCell;
                column.BindCell += OnBindCell;
                column.CreateHeader += OnCreateHeader;
                column.Reset += OnReset;

                columns[i] = column;
                i++;
            }

            m_Table.SetColumns(columns);
            m_Table.ItemsSource = m_PrimaryKey;
            m_Table.RefreshItems();
        }

        public void SetOptionsState(bool isAllOptionsTrue)
        {
            m_FilterModule.ClearAllOptions();

            if (isAllOptionsTrue)
            {
                foreach (var optionSet in m_AllOptions)
                {
                    foreach (var option in optionSet.Value)
                    {
                        m_FilterModule.AddSelectedOption(optionSet.Key, option);
                    }
                }
            }

            m_OnFilterChanged();
        }

        void CreatePossibleOptions(List<T> dataList)
        {
            var maxOptionsCount = 0;
            var hashSet = new HashSet<string>();

            m_AllOptions.Clear();

            foreach (var item in m_FilterModule.AllFilterNodes)
            {
                var nodeName = item.Key;
                m_AllOptions[nodeName] = new List<string>();

                foreach (var data in dataList)
                {
                    var option = item.Value.bindPath(data);

                    if (hashSet.Add(option))
                    {
                        m_AllOptions[nodeName].Add(option);
                    }
                }

                maxOptionsCount = Mathf.Max(hashSet.Count, maxOptionsCount);
                hashSet.Clear();
            }

            m_PrimaryKey.Clear();
            for (var i = 0; i < maxOptionsCount; i++)
            {
                m_PrimaryKey.Add(i);
            }
        }

        void OnBindCell(VisualElement e, IColumnData column, object data)
        {
            if (data is not int id)
                return;

            var checkbox = e.Q<Checkbox>(GetCheckBoxName(column.Name));
            if (checkbox != null && m_AllOptions.TryGetValue(column.Name, out var optionList))
            {
                if (id >= 0 && id < optionList.Count)
                {
                    // Show option
                    checkbox.label = optionList[id];
                    checkbox.tooltip = optionList[id];
                    checkbox.style.display = DisplayStyle.Flex;
                    m_CheckboxMap[checkbox] = (column.Name, id);

                    checkbox.SetValueWithoutNotify(m_FilterModule.ContainsOption(column.Name, optionList[id])
                        ? CheckboxState.Checked
                        : CheckboxState.Unchecked);
                }
                else
                {
                    // Hide checkbox
                    checkbox.style.display = DisplayStyle.None;
                }
            }
        }

        void OnMakeCell(VisualElement e, IColumnData column)
        {
            var checkbox = new Checkbox
            {
                name = GetCheckBoxName(column.Name)
            };
            checkbox.style.flexShrink = 1;

            checkbox.AddManipulator(new Clickable((_) => OnCheckBoxClicked(checkbox)));

            EllipsisText(checkbox);

            e.Add(checkbox);
        }

        void OnCreateHeader(VisualElement e, IColumnData column)
        {
            var name = column.Name;
            var checkbox = new Checkbox()
            {
                name = GetCheckBoxName(name),
                label = column.Name,
                tooltip = column.Name,
                style =
                {
                    flexShrink = 1,
                },
            };

            EllipsisText(checkbox);
            checkbox.clickable.clicked += () => OnCheckBoxClicked(checkbox);
            m_CheckboxMap[checkbox] = (name, -1);

            e.Add(checkbox);

            m_HeaderCheckBoxMap.Add(column.Name, checkbox);
        }

        static void EllipsisText(Checkbox checkbox)
        {
            var text = checkbox.Q<LocalizedTextElement>();
            text.style.textOverflow = TextOverflow.Ellipsis;
            text.style.whiteSpace = WhiteSpace.NoWrap;
        }

        void OnCheckBoxClicked(Checkbox checkBox)
        {
            if (!m_CheckboxMap.TryGetValue(checkBox, out var value))
                return;

            var columnName = value.Item1;

            if (!m_AllOptions.TryGetValue(columnName, out var list))
                return;

            var id = value.Item2;
            if (id >= 0 && id < list.Count)
            {
                if (checkBox.value == CheckboxState.Checked)
                    m_FilterModule.AddSelectedOption(columnName, list[id]);
                else
                    m_FilterModule.RemoveSelectedOption(columnName, list[id]);

                RefreshHeadersState();
            }
            else if (id == -1) // Clicked on header
            {
                SelectAllOptions(list, columnName, checkBox.value == CheckboxState.Checked);
            }

            m_OnFilterChanged?.Invoke();
        }

        void SelectAllOptions(List<string> optionsList, string columnName, bool isSelected)
        {
            if (isSelected)
            {
                // all true
                foreach (var option in optionsList)
                {
                    m_FilterModule.AddSelectedOption(columnName, option);
                }
            }
            else
            {
                // all false
                foreach (var option in optionsList)
                {
                    m_FilterModule.RemoveSelectedOption(columnName, option);
                }
            }

            m_Table.RefreshItems();
        }

        /// <summary>
        /// Checks if all options were selected and change headers checkbox
        /// </summary>
        void RefreshHeadersState()
        {
            foreach (var checkboxSet in m_HeaderCheckBoxMap)
            {
                if (m_AllOptions.TryGetValue(checkboxSet.Key, out var list))
                {
                    checkboxSet.Value?.SetValueWithoutNotify(m_FilterModule.CountSelectedOptions(checkboxSet.Key) == list.Count
                        ? CheckboxState.Checked
                        : CheckboxState.Unchecked);
                }
            }
        }

        public void SetStylesToPopover(params string[] popoverStyles)
        {
            foreach (var headerStyle in popoverStyles)
            {
                if (string.IsNullOrEmpty(headerStyle))
                    continue;
                m_ContentContainer?.AddToClassList(headerStyle);
            }
        }

        public void SetStylesToHeader(params string[] headerStyles)
        {
            foreach (var headerStyle in headerStyles)
            {
                if (string.IsNullOrEmpty(headerStyle))
                    continue;
                m_Table.AddStyleToHeader(headerStyle);
            }
        }

        public void SetStylesToRow(params string[] rowStyles)
        {
            foreach (var rowStyle in rowStyles)
            {
                if (string.IsNullOrEmpty(rowStyle))
                    continue;
                m_Table.AddStyleToRow(rowStyle);
            }
        }

        string GetCheckBoxName(string columnName) => $"checkbox{columnName}";
    }
}
