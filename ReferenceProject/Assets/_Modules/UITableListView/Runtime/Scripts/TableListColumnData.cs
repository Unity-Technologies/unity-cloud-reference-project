using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class TableListColumnData: IServiceData, IColumnEventData
    {
        readonly string m_Name;
        
        readonly bool m_IsVisible;
        
        readonly bool m_UseInlineWidth;
        
        readonly int m_Width;
        
        readonly string[] m_ColumnStyles;
        
        List<object> m_ServiceData = new List<object>();
        
        List<object> IServiceData.ServiceData
        {
            get
            {
                if (m_ServiceData == null)
                {
                    m_ServiceData = new List<object>();
                }
                return m_ServiceData;
            }
        }
        
        public event Action<VisualElement, IColumnData> CreateHeader;
        public event Action<VisualElement, IColumnData> MakeCell;
        public event Action<VisualElement, IColumnData, object> BindCell;
        public event Action<VisualElement, IColumnData, object> UnbindCell;
        public event Action<PointerEnterEvent> PointerEnterListElementEvent;
        public event Action<PointerLeaveEvent> PointerLeaveListElementEvent;
        
        /// <summary>
        /// Calls when row has been selected on the table
        /// </summary>
        public event Action<object> SelectionChanged;
        
        /// <summary>
        /// Calls to reset cached data before column adds to a new table,
        /// when table has been detached from hierarchy,
        /// when table has been disabled
        /// </summary>
        public event Action Reset;
        
        string IServiceData.Name => m_Name;
        string IColumnData.Name => m_Name;
        bool IColumnData.IsVisible => m_IsVisible;
        bool IColumnData.UseInlineWidth => m_UseInlineWidth;
        float IColumnData.Width => m_Width;
        string[] IColumnData.ColumnStyles => m_ColumnStyles;

        public TableListColumnData(string name, bool isVisible, params string[] columnStyles)
        {
            m_Name = name;
            m_IsVisible = isVisible;
            m_ColumnStyles = columnStyles;
        }
        
        public TableListColumnData(string name, bool isVisible, bool useInlineWidth, int columnWidth, params string[] columnStyles) : this(name, isVisible, columnStyles)
        {
            m_UseInlineWidth = useInlineWidth;
            m_Width = columnWidth;
        }

        void IColumnEventData.InvokeCreateHeader(VisualElement element, IColumnData columnData) => CreateHeader?.Invoke(element, columnData);
        void IColumnEventData.InvokeMakeCell(VisualElement element, IColumnData columnData) => MakeCell?.Invoke(element, columnData);
        void IColumnEventData.InvokeBindCell(VisualElement element, IColumnData columnData, object data) => BindCell?.Invoke(element, columnData, data);
        void IColumnEventData.InvokeUnbindCell(VisualElement element, IColumnData columnData, object data) => UnbindCell?.Invoke(element, columnData, data);
        void IColumnEventData.InvokePointerEnterListElementEvent(PointerEnterEvent pointerEventData) => PointerEnterListElementEvent?.Invoke(pointerEventData);
        void IColumnEventData.InvokePointerLeaveListElementEvent(PointerLeaveEvent pointerEventData) => PointerLeaveListElementEvent?.Invoke(pointerEventData);
        void IColumnEventData.InvokeSelectionChanged(object selection) => SelectionChanged?.Invoke(selection);
        void IColumnEventData.InvokeReset() => Reset?.Invoke();
        
        public static void BuildTextHeader(VisualElement e, IColumnData column)
        {
            e.Add(new Text()
            {
                name = $"text-{column.Name}", 
                text = column.Name,
                pickingMode = PickingMode.Ignore
            });
        }
        
        public static void BuildIconHeader(VisualElement e, IColumnData column)
        {
            e.Add(new Icon()
            {
                name = $"icon-{column.Name}",
                iconName = column.Name,
                pickingMode = PickingMode.Ignore
            });
        }
    }
}
