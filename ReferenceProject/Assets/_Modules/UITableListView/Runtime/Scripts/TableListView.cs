using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public CollectionVirtualizationMethod virtualizationMethod
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

        static int defaultfixedItemHeight => 30;

        int m_fixedItemHeight = defaultfixedItemHeight;

        public int fixedItemHeight
        {
            get => m_fixedItemHeight;
            set
            {
                m_fixedItemHeight = value;
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

        public event Action<MouseEnterEvent> OnMouseEnterListElementEvent;
        public event Action<MouseLeaveEvent> OnMouseLeaveListElementEvent;

        readonly List<VisualElement> m_Rows = new List<VisualElement>();

        public IList itemsSource
        {
            set
            {
                m_ListView.itemsSource = value;
                m_ListView.RefreshItems();
            }
            get => m_ListView.itemsSource;
        }

        public TableListView()
        {
            m_Header = new VisualElement();
            m_Header.name = "table-header";
            m_Header.style.flexDirection = FlexDirection.Row;
            m_Header.style.flexShrink = 0;
            m_Header.style.display = showTableHeader ? DisplayStyle.Flex : DisplayStyle.None;
            
            hierarchy.Add(m_Header);

            m_ListView = new ListView();
            m_ListView.fixedItemHeight = fixedItemHeight;
            m_ListView.virtualizationMethod = virtualizationMethod;

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
                    OnMouseEnterListElementEvent += column.InvokeMouseEnterListElementEvent;
                    OnMouseLeaveListElementEvent += column.InvokeMouseLeaveListElementEvent;
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
            var rowContainer = new VisualElement();
            rowContainer.style.flexDirection = FlexDirection.Row;

            rowContainer.name = $"Row-{m_Rows.Count}";

            rowContainer.RegisterCallback<MouseEnterEvent>(x => OnMouseEnterListElementEvent?.Invoke(x));
            rowContainer.RegisterCallback<MouseLeaveEvent>(x => OnMouseLeaveListElementEvent?.Invoke(x));

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
                { name = "fixed-item-height", defaultValue = TableListView.defaultfixedItemHeight };

            readonly UxmlEnumAttributeDescription<CollectionVirtualizationMethod> m_VirtualizationMethod =
                new UxmlEnumAttributeDescription<CollectionVirtualizationMethod>()
                {
                    name = "virtualization-method", defaultValue = CollectionVirtualizationMethod.FixedHeight
                };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var num = TableListView.defaultfixedItemHeight;
                var listTableView = (TableListView)ve;
                listTableView.showTableHeader = m_ShowTableHeader.GetValueFromBag(bag, cc);
                if (m_FixedItemHeight.TryGetValueFromBag(bag, cc, ref num))
                    listTableView.fixedItemHeight = num;
                listTableView.virtualizationMethod = m_VirtualizationMethod.GetValueFromBag(bag, cc);
            }
        }
    }
}
