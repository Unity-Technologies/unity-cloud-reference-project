using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public abstract class TableListColumn : MonoBehaviour
    {
        [SerializeField]
        string m_ColumnName;

        [SerializeField]
        bool m_IsVisible;

        [SerializeField]
        bool m_UseInlineWidth;

        [SerializeField]
        int m_Width;

        [SerializeField]
        string[] m_ColumnStyles;

        TableListColumnData m_Column;

        protected string ColumnName => m_ColumnName;

        public IColumnEventData Column
        {
            get
            {
                if (m_Column == null)
                {
                    m_Column = new TableListColumnData(m_ColumnName, m_IsVisible, m_UseInlineWidth, m_Width, m_ColumnStyles);
                    m_Column.CreateHeader += OnCreateHeader;
                    m_Column.MakeCell += OnMakeCell;
                    m_Column.BindCell += OnBindCell;
                    m_Column.UnbindCell += OnUnbindCell;
                    m_Column.PointerEnterListElementEvent += OnPointerEnterListElementEvent;
                    m_Column.PointerLeaveListElementEvent += OnPointerLeaveListElementEvent;
                    m_Column.SelectionChanged += OnSelectionChanged;
                    m_Column.Reset += OnReset;

                    if (m_Column is IServiceData serviceData)
                    {
                        AddServices(serviceData.ServiceData);
                    }
                }

                return m_Column;
            }
        }

        protected abstract void OnCreateHeader(VisualElement e, IColumnData columnData);

        protected abstract void OnMakeCell(VisualElement e, IColumnData columnData);

        protected abstract void OnBindCell(VisualElement e, IColumnData columnData, object data);

        protected virtual void OnUnbindCell(VisualElement e, IColumnData columnData, object data)
        {
            // Implemented by inheritance
        }

        protected virtual void OnPointerEnterListElementEvent(PointerEnterEvent pointerEventData)
        {
            // Implemented by inheritance
        }

        protected virtual void OnPointerLeaveListElementEvent(PointerLeaveEvent pointerEventData)
        {
            // Implemented by inheritance
        }

        protected virtual void OnSelectionChanged(object selection)
        {
            // Implemented by inheritance
        }

        protected virtual void OnReset()
        {
            // Implemented by inheritance
        }

        protected virtual void AddServices(List<object> services)
        {
            // Implemented by inheritance
        }
    }
}
