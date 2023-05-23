using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public interface IExtraColumn
    {
        IColumnEventData Column { get; }
    }
    
    public interface IColumnData
    {
        string Name { get; }
        bool IsVisible { get; }
        bool UseInlineWidth { get; }
        float Width { get; }
        string[] ColumnStyles { get; }
    }

    public interface IColumnEventData: IColumnData
    {
        void InvokeCreateHeader(VisualElement element, IColumnData columnData);
        void InvokeMakeCell(VisualElement element, IColumnData columnData);
        void InvokeBindCell(VisualElement element, IColumnData columnData, object data);
        void InvokeUnbindCell(VisualElement element, IColumnData columnData, object data);
        void InvokePointerEnterListElementEvent(PointerEnterEvent pointerEventData);
        void InvokePointerLeaveListElementEvent(PointerLeaveEvent pointerEventData);
        void InvokeSelectionChanged(object selection);
        void InvokeReset();
    }
    
    public interface IServiceData
    {
        public string Name { get; }
        public List<object> ServiceData { get;}
    }
}
