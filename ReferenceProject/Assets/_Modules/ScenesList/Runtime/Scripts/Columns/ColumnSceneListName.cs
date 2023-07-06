using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListName : TableListColumn
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;
        
        [SerializeField]
        string m_TextName;
        
        [SerializeField]
        bool m_IsUseEllipsisText;
        
        [SerializeField]
        string m_EllipsisTextStyle;

        IHighlightModule m_HighlightNode;

        protected override void AddServices(List<object> services)
        {
            base.AddServices(services);
            services.Add(new SearchBindNode<object>(x => x is IScene data ? data.Name : null));
            services.Add(new SortBindNodeString<object>(x => x is IScene data ? data.Name : null));
            m_HighlightNode = new HighlightModuleNode();
            services.Add(m_HighlightNode);
        }

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildTextHeader(e, columnData);
        }

        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var container = m_VisualTreeAsset.Instantiate();
            if (m_IsUseEllipsisText && !string.IsNullOrEmpty(m_EllipsisTextStyle))
            {
                var text = container.Q<Text>(m_TextName);
                text?.AddToClassList(m_EllipsisTextStyle);
            }

            e.Add(container);
        }

        protected override void OnBindCell(VisualElement e, IColumnData columnData, object data)
        {
            if (data is not IScene dataSet)
            {
                Debug.LogWarning(
                    $"Wrong type of data pasted to the column `{columnData.Name}`. Expected type is {nameof(IScene)}");
                return;
            }

            var text = e.Q<Text>(m_TextName);

            if (text != null)
            {
                text.text = m_HighlightNode.IsHighlighted(dataSet.Name).Item2;
            }
        }
    }
}
