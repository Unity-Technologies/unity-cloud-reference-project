using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class TableListView : VisualElement
    {
        bool m_ShowTableHeader;

        public bool showTableHeader
        {
            get => m_ShowTableHeader;
            set
            {
                m_ShowTableHeader = value;
                if (m_Header != null)
                {
                    m_Header.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        CollectionVirtualizationMethod m_VirtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        public CollectionVirtualizationMethod VirtualizationMethod
        {
            get => m_VirtualizationMethod;
            set
            {
                m_VirtualizationMethod = value;
                if (m_ListView != null)
                {
                    m_ListView.virtualizationMethod = value;
                }
            }
        }

        static readonly int s_DefaultFixedItemHeight = 30;

        int m_FixedItemHeight = s_DefaultFixedItemHeight;

        public int FixedItemHeight
        {
            get => m_FixedItemHeight;
            set
            {
                m_FixedItemHeight = value;
                if (m_ListView != null)
                {
                    m_ListView.fixedItemHeight = value;
                }
            }
        }

        readonly VisualElement m_Header;
        readonly ListView m_ListView;

        public ListView ListView => m_ListView;

        readonly HashSet<IColumnEventData> m_Columns = new HashSet<IColumnEventData>();

        readonly HashSet<string> m_HeaderStyles = new HashSet<string>();
        readonly HashSet<string> m_RowStyles = new HashSet<string>();

        public event Action<MouseEnterEvent> MouseEnterListElementEvent;
        public event Action<MouseLeaveEvent> MouseLeaveListElementEvent;
        public event Action<VisualElement> ItemClicked;

        readonly List<VisualElement> m_Rows = new List<VisualElement>();

        public IList ItemsSource
        {
            get => m_ListView.itemsSource;
            set
            {
                m_ListView.itemsSource = value;
                m_ListView.RefreshItems();
            }
        }

        public TableListView()
        {
            m_Header = new VisualElement
            {
                name = "table-header",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0,
                    display = showTableHeader ? DisplayStyle.Flex : DisplayStyle.None
                }
            };

            hierarchy.Add(m_Header);

            m_ListView = new ListView
            {
                fixedItemHeight = FixedItemHeight,
                virtualizationMethod = VirtualizationMethod
            };

            hierarchy.Add(m_ListView);

            m_ListView.bindItem = BindItem;
            m_ListView.unbindItem = UnbindItem;
            m_ListView.makeItem = MakeItem;
            m_ListView.onSelectionChange += OnSelectionChange;
        }

        void OnSelectionChange(IEnumerable<object> selected)
        {
            var obj = selected.FirstOrDefault();
            if (obj != null)
            {
                foreach (var column in m_Columns)
                {
                    column.InvokeSelectionChanged(obj);
                }
            }
        }

        public void RefreshItems() => m_ListView.RefreshItems();

        public void SetColumns(params IColumnEventData[] columnArray)
        {
            foreach (var column in m_Columns)
            {
                column?.InvokeReset();
            }

            m_Columns.Clear();

            foreach (var column in columnArray)
            {
                if (column == null)
                    continue;
                
                if (column.IsVisible)
                {
                    m_Columns.Add(column);
                    MouseEnterListElementEvent += column.InvokeMouseEnterListElementEvent;
                    MouseLeaveListElementEvent += column.InvokeMouseLeaveListElementEvent;
                }
            }

            RefreshHeaders();
            RemakeAllRows();
        }

        void RefreshHeaders()
        {
            m_Header.Clear();

            foreach (var column in m_Columns)
            {
                CreateHeader(column);
            }

            RemakeAllRows();
        }

        void CreateHeader(IColumnEventData column)
        {
            var headerContainer = new VisualElement() { name = column.Name };

            if (column.UseInlineWidth)
            {
                headerContainer.style.width = column.Width;
            }

            if (column.ColumnStyles != null)
            {
                foreach (var style in column.ColumnStyles)
                {
                    headerContainer.AddToClassList(style);
                }
            }

            foreach (var style in m_HeaderStyles)
            {
                headerContainer.AddToClassList(style);
            }

            m_Header.Add(headerContainer);
            
            column.InvokeCreateHeader(headerContainer, column);
        }

        VisualElement MakeItem()
        {
            var rowContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row },
                name = $"Row-{m_Rows.Count}"
            };

            rowContainer.RegisterCallback<MouseEnterEvent>(x => MouseEnterListElementEvent?.Invoke(x));
            rowContainer.RegisterCallback<MouseLeaveEvent>(x => MouseLeaveListElementEvent?.Invoke(x));
            
            rowContainer.AddManipulator(new Pressable(() => ItemClicked?.Invoke(rowContainer)));

            foreach (var style in m_RowStyles)
            {
                rowContainer.AddToClassList(style);
            }

            m_Rows.Add(rowContainer);

            MakeRow(rowContainer);

            return rowContainer;
        }

        void MakeRow(VisualElement rowContainer)
        {
            foreach (var column in m_Columns)
            {
                var cell = new VisualElement();
                if (column.UseInlineWidth)
                {
                    cell.style.width = column.Width;
                }

                if (column.ColumnStyles != null)
                {
                    foreach (var style in column.ColumnStyles)
                    {
                        cell.AddToClassList(style);
                    }
                }
                
                column.InvokeMakeCell(cell, column);
                rowContainer.Add(cell);
            }
        }

        void RemakeAllRows()
        {
            foreach (var row in m_Rows)
            {
                row.Clear();
                MakeRow(row);
            }

            m_ListView.RefreshItems();
        }

        void BindItem(VisualElement e, int id)
        {
            if (id < 0 || id >= m_ListView.itemsSource.Count)
                return;

            var data = m_ListView.itemsSource[id];

            foreach (var column in m_Columns)
            {
                column.InvokeBindCell(e, column, data);
            }
        }

        void UnbindItem(VisualElement e, int id)
        {
            if (id < 0 || id >= m_ListView.itemsSource.Count)
                return;

            var data = m_ListView.itemsSource[id];

            foreach (var column in m_Columns)
            {
                column.InvokeUnbindCell(e, column, data);
            }
        }

        public void AddStyleToHeader(string additionalStyle)
        {
            if (string.IsNullOrEmpty(additionalStyle))
                return;

            if (m_HeaderStyles.Add(additionalStyle))
            {
                var headers = m_Header.Children();
                foreach (var headerStyle in m_HeaderStyles)
                {
                    foreach (var header in headers)
                    {
                        header.AddToClassList(headerStyle);
                    }
                }
            }
        }

        public void RemoveStyleFromHeader(string additionalStyle)
        {
            if (string.IsNullOrEmpty(additionalStyle))
                return;

            if (m_HeaderStyles.Remove(additionalStyle))
            {
                var headers = m_Header.Children();
                foreach (var style in m_HeaderStyles)
                {
                    foreach (var header in headers)
                    {
                        header.RemoveFromClassList(style);
                    }
                }
            }
        }

        public void AddStyleToRow(string additionalStyle)
        {
            if (string.IsNullOrEmpty(additionalStyle))
                return;

            if (m_RowStyles.Add(additionalStyle))
            {
                foreach (var rowStyle in m_RowStyles)
                {
                    foreach (var row in m_Rows)
                    {
                        row.AddToClassList(rowStyle);
                    }
                }
            }
        }

        public void RemoveStyleFromRow(string additionalStyle)
        {
            if (string.IsNullOrEmpty(additionalStyle))
                return;

            if (m_RowStyles.Remove(additionalStyle))
            {
                foreach (var style in m_RowStyles)
                {
                    foreach (var row in m_Rows)
                    {
                        row.RemoveFromClassList(style);
                    }
                }
            }
        }

        public new class UxmlFactory : UxmlFactory<TableListView, UxmlTraits>
        {
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_ShowTableHeader = new UxmlBoolAttributeDescription
                { name = "show-table-header", defaultValue = false };

            readonly UxmlIntAttributeDescription m_FixedItemHeight = new UxmlIntAttributeDescription()
                { name = "fixed-item-height", defaultValue = TableListView.s_DefaultFixedItemHeight };

            readonly UxmlEnumAttributeDescription<CollectionVirtualizationMethod> m_VirtualizationMethod =
                new UxmlEnumAttributeDescription<CollectionVirtualizationMethod>()
                {
                    name = "virtualization-method", defaultValue = CollectionVirtualizationMethod.FixedHeight
                };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var num = TableListView.s_DefaultFixedItemHeight;
                var listTableView = (TableListView)ve;
                listTableView.showTableHeader = m_ShowTableHeader.GetValueFromBag(bag, cc);
                if (m_FixedItemHeight.TryGetValueFromBag(bag, cc, ref num))
                    listTableView.FixedItemHeight = num;
                listTableView.VirtualizationMethod = m_VirtualizationMethod.GetValueFromBag(bag, cc);
            }
        }
    }
}
