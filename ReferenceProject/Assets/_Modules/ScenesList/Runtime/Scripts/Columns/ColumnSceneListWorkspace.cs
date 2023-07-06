using System.Collections.Generic;
using Unity.Cloud.Common;
using Unity.ReferenceProject.UITableListView;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.ScenesList
{
    public class ColumnSceneListWorkspace : TableListColumn
    {
        [SerializeField]
        bool m_IsUseEllipsisText;
        
        [SerializeField]
        string m_EllipsisTextStyle;

        IHighlightModule m_HighlightNode;
        
        SceneWorkspaceProvider m_SceneWorkspaceProvider;
        
        [Inject]
        public void Setup(SceneWorkspaceProvider sceneWorkspaceProvider)
        {
            m_SceneWorkspaceProvider = sceneWorkspaceProvider;
        }
        
        protected override void AddServices(List<object> services)
        {
            base.AddServices(services);
            services.Add(new SearchBindNode<object>(x => x is IScene data ? m_SceneWorkspaceProvider.GetWorkspaceName(data.WorkspaceId) : null));
            services.Add(new SortBindNodeString<object>(x => x is IScene data ? m_SceneWorkspaceProvider.GetWorkspaceName(data.WorkspaceId) : null));
            m_HighlightNode = new HighlightModuleNode();
            services.Add(m_HighlightNode);
            services.Add(new FilterBindNode<object>(x => x is IScene data ? m_SceneWorkspaceProvider.GetWorkspaceName(data.WorkspaceId) : null, FilterCompareType.Equals));
        }

        protected override void OnCreateHeader(VisualElement e, IColumnData columnData)
        {
            TableListColumnData.BuildTextHeader(e, columnData);
        }

        protected override void OnMakeCell(VisualElement e, IColumnData columnData)
        {
            var text = new Text() { name = $"text-{columnData.Name}" };
            if (m_IsUseEllipsisText && !string.IsNullOrEmpty(m_EllipsisTextStyle))
            {
                text.AddToClassList(m_EllipsisTextStyle);
            }
            
            e.Add(text);
        }

        protected override void OnBindCell(VisualElement e, IColumnData columnData, object data)
        {
            if (data is not IScene dataSet)
            {
                Debug.LogWarning(
                    $"Wrong type of data pasted to the column `{columnData.Name}`. Expected type is {nameof(IScene)}");
                return;
            }

            var text = e.Q<Text>($"text-{columnData.Name}");

            if (text != null)
            {
                string workspaceName = m_SceneWorkspaceProvider != null
                    ? m_SceneWorkspaceProvider.GetWorkspaceName(dataSet.WorkspaceId)
                    : dataSet.WorkspaceId.ToString();
                    
                text.text = m_HighlightNode.IsHighlighted(workspaceName).Item2;
            }
        }
    }
}
