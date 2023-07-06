using System;
using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.AccessHistory;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListLastAccess : TableListColumn
    {
        [SerializeField]
        bool m_IsUseEllipsisText;
        
        [SerializeField]
        string m_EllipsisTextStyle;
        
        IAccessHistoryController m_AccessHistoryController;
        
        [Inject]
        void Setup(IAccessHistoryController accessHistoryController)
        {
            m_AccessHistoryController = accessHistoryController;
        }

        protected override void AddServices(List<object> services)
        {
            base.AddServices(services);
            services.Add(new SortBindNodeLong<object>(x =>
                x is IScene scene && m_AccessHistoryController.Accessed(scene.Id, out var accessDate)
                    ? accessDate.Ticks
                    : long.MaxValue));
        }

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildTextHeader(e, columnData);
        }
        
        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var text = new Text();
            text.name = GetTextName(columnData);
            if (m_IsUseEllipsisText && !string.IsNullOrEmpty(m_EllipsisTextStyle))
            {
                text.AddToClassList(m_EllipsisTextStyle);
            }
            
            e.Add(text);
        }

        protected override void OnBindCell(VisualElement e, IColumnData columnData, object data)
        {
            if (data is not IScene scene)
            {
                Debug.LogWarning(
                    $"Wrong type of data pasted to the column `{columnData.Name}`. Expected type is {nameof(IScene)}");
                return;
            }

            var text = e.Q<Text>(GetTextName(columnData));

            if (text != null)
            {
                text.text = m_AccessHistoryController.Accessed(scene.Id, out DateTime accessDate)
                    ? LastAccessStringBuilder(accessDate)
                    : null;
            }
        }
        
        string GetTextName(IColumnData columnData) => $"text-{columnData.Name}";

        static string LastAccessStringBuilder(DateTime accessDate)
        {
            return  accessDate.Day == DateTime.Now.Day
                ? accessDate.ToShortTimeString()
                : accessDate.ToShortDateString();
        }
    }
}
