using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Clickable = Unity.AppUI.UI.Clickable;

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
                   m_Header.style.height = value ? StyleKeyword.Auto : 0;
                   m_Header.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
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

        ScrollerVisibility m_HorizontalScrollerVisibility;

        public ScrollerVisibility HorizontalScrollerVisibility
        {
            get => m_HorizontalScrollerVisibility;
            set
            {
                m_HorizontalScrollerVisibility = value;
                var scrollView = m_ListView.Q<ScrollView>();
                scrollView.horizontalScrollerVisibility = value;
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
        readonly List<VisualElement> m_Rows = new List<VisualElement>();

        readonly HashSet<string> m_HeaderStyles = new HashSet<string>();
        readonly HashSet<string> m_RowStyles = new HashSet<string>();

        public event Action<PointerEnterEvent> PointerEnterListElementEvent;
        public event Action<PointerLeaveEvent> PointerLeaveListElementEvent;
        
        public event Action<object> itemClicked;
        public event Action<int> itemClickedId;
        
        /// <summary>
        /// Callback for binding a data item to the VisualElement.
        /// </summary>
        public event Action<VisualElement, int> bindItem;

        /// <summary>
        /// Callback for unbinding a data item from the VisualElement.
        /// </summary>
        public event Action<VisualElement, int> unbindItem;
        
        static readonly string k_ListContentViewportName = "unity-content-viewport";
        
        static string GetCellName(IColumnEventData column) => $"Cell-{column.Name}";

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
                }
            };

            hierarchy.Add(m_Header);
            
            showTableHeader = m_ShowTableHeader;

            m_ListView = new ListView
            {
                fixedItemHeight = FixedItemHeight,
                virtualizationMethod = VirtualizationMethod
            };

            hierarchy.Add(m_ListView);
            
            HorizontalScrollerVisibility = m_HorizontalScrollerVisibility;
            
            // Makes header align with m_ListView content width
            m_ListView.Q<VisualElement>(k_ListContentViewportName).RegisterCallback<GeometryChangedEvent>(eventData =>
            {
                // When only width has been changed
                if (Mathf.FloorToInt(eventData.newRect.width) != Mathf.FloorToInt(eventData.oldRect.width))
                {
                    m_Header.style.width = eventData.newRect.width;
                }
            });

            m_ListView.bindItem = OnBindItem;
            m_ListView.unbindItem = OnUnbindItem;
            m_ListView.makeItem = OnMakeItem;
            m_ListView.onSelectionChange += OnSelectionChange;

            RegisterCallback<DetachFromPanelEvent>(OnDetachFromHierarchy);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        void OnDetachFromHierarchy(DetachFromPanelEvent evt)
        {
            ResetColumns();
        }
        
        void OnGeometryChange(GeometryChangedEvent geometryChangedEvent)
        {
            // Check if element has been disabled. When visualElement.style.display = DisplayStyle.None - newRect become Rect.zero
            if (geometryChangedEvent.newRect == Rect.zero)
            {
                ResetColumns();
            }
        }

        void ResetColumns()
        {
            foreach (var column in m_Columns)
            {
                column?.InvokeReset();
            }
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
            ResetColumns();

            m_Columns.Clear();

            foreach (var column in columnArray)
            {
                if (column == null)
                    continue;
                
                if (column.IsVisible)
                {
                    m_Columns.Add(column);
                    PointerEnterListElementEvent += column.InvokePointerEnterListElementEvent;
                    PointerLeaveListElementEvent += column.InvokePointerLeaveListElementEvent;
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
        }

        void CreateHeader(IColumnEventData column)
        {
            var headerContainer = new VisualElement() { name = column.Name };
            headerContainer.RegisterCallback<GeometryChangedEvent>(eventData =>
            {
                // When only width has been changed
                if (Mathf.FloorToInt(eventData.newRect.width) != Mathf.FloorToInt(eventData.oldRect.width))
                {
                    RecalculateAllCellsWidth(column);
                }
            });

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

        VisualElement OnMakeItem()
        {
            var rowContainer = new VisualElement
            {
                name = $"Row-{m_Rows.Count}",
                style = { flexDirection = FlexDirection.Row }
            };
            
            rowContainer.RegisterCallback<PointerEnterEvent>(x => PointerEnterListElementEvent?.Invoke(x));
            rowContainer.RegisterCallback<PointerLeaveEvent>(x => PointerLeaveListElementEvent?.Invoke(x));
            
            rowContainer.AddManipulator(new Clickable(() => OnItemClicked(rowContainer)));

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
                var cell = new VisualElement()
                {
                    name = GetCellName(column),
                    style = { height = Length.Percent(100) }
                };

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
            
            RecalculateCellsWidthForRow(rowContainer);
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

        void OnBindItem(VisualElement e, int id)
        {
            if (id < 0 || id >= m_ListView.itemsSource.Count)
                return;

            var data = m_ListView.itemsSource[id];

            foreach (var column in m_Columns)
            {
                column.InvokeBindCell(e, column, data);
            }
            
            e.userData = id;
            bindItem?.Invoke(e, id);
        }

        void OnUnbindItem(VisualElement e, int id)
        {
            if (id < 0 || id >= m_ListView.itemsSource.Count)
                return;

            var data = m_ListView.itemsSource[id];

            foreach (var column in m_Columns)
            {
                column.InvokeUnbindCell(e, column, data);
            }
            
            e.userData = -1;
            unbindItem?.Invoke(e, id);
        }

        void OnItemClicked(VisualElement rowContainer)
        {
            if (rowContainer.userData is int index and >= 0)
            {
                var item = ListView.itemsSource[index];
                itemClicked?.Invoke(item);
                itemClickedId?.Invoke(index);
            }
        }

        /// <summary>
        /// Makes cells width equal to header column width in a particular row
        /// </summary>
        void RecalculateCellsWidthForRow(VisualElement rowContainer)
        {
            foreach (var column in m_Columns)
            {
                var headerCell = m_Header.Q<VisualElement>(column.Name);
                var cell = rowContainer.Q<VisualElement>(GetCellName(column));
                cell.style.width = headerCell.resolvedStyle.width;
            }
        }
        
        /// <summary>
        /// Recalculates the width for all cells at the table and makes them equal to the header column width
        /// </summary>
        void RecalculateAllCellsWidth(IColumnEventData column)
        {
            var headerCell = m_Header.Q<VisualElement>(column.Name);
            foreach (var row in m_Rows)
            {
                var cell = row.Q<VisualElement>(GetCellName(column));
                cell.style.width = headerCell.resolvedStyle.width;
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
            readonly UxmlBoolAttributeDescription m_ShowTableHeader = new ()
                { name = "show-table-header", defaultValue = false };

            readonly UxmlIntAttributeDescription m_FixedItemHeight = new ()
                { name = "fixed-item-height", defaultValue = TableListView.s_DefaultFixedItemHeight };

            readonly UxmlEnumAttributeDescription<CollectionVirtualizationMethod> m_VirtualizationMethod = new ()
                { name = "virtualization-method", defaultValue = CollectionVirtualizationMethod.FixedHeight };
            
            readonly UxmlEnumAttributeDescription<ScrollerVisibility> m_HorizontalScrollerVisibility = new ()
                { name = "horizontal-scroller-visibility", defaultValue = ScrollerVisibility.Auto };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var num = TableListView.s_DefaultFixedItemHeight;
                var listTableView = (TableListView)ve;
                listTableView.showTableHeader = m_ShowTableHeader.GetValueFromBag(bag, cc);
                if (m_FixedItemHeight.TryGetValueFromBag(bag, cc, ref num))
                    listTableView.FixedItemHeight = num;
                listTableView.VirtualizationMethod = m_VirtualizationMethod.GetValueFromBag(bag, cc);
                listTableView.HorizontalScrollerVisibility = m_HorizontalScrollerVisibility.GetValueFromBag(bag, cc);
            }
        }
    }
}
