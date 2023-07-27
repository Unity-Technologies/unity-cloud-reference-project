using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class ColumnSearchService : MonoBehaviour, IService
    {
        [SerializeField]
        int m_ServicePriority;
        
        [SerializeField]
        string m_SearchBarName = "searchbar";

        SearchUI m_SearchUI;
        SearchModule<object> m_SearchModule;
        HighlightModule m_HighlightModule;
        
        public event Action OnRefresh;
        public int ServicePriority => m_ServicePriority;
        
        void OnDestroy() => ClearService();

        public void ClearService()
        {
            m_SearchUI?.UnregisterCallbacks();
        }

        public void InitializeService(VisualElement root)
        {
            m_SearchModule = new SearchModule<object>();
            
            // Search UI setup
            m_SearchUI = new SearchUI(m_SearchModule, root, OnRefresh, null, m_SearchBarName);
            // Highlight setup
            m_HighlightModule = new HighlightModule(m_SearchModule);
        }
        public void InitializeUIForData(VisualElement e, IServiceData data)
        {
            foreach (var serviceData in data.ServiceData)
            {
                if (serviceData is SearchBindNode<object> bindNode)
                {
                    m_SearchModule.AddBindings((data.Name, bindNode));
                }
            }
            
            foreach (var serviceData in data.ServiceData)
            {
                if (serviceData is HighlightModuleNode node)
                {
                    node.HighlightModule = m_HighlightModule;
                }
            }
        }
        
        public void OnPrimaryKeysCreated(List<object> list)
        {
            // Empty
        }

        public async Task PerformService(List<object> list, CancellationToken cancellationToken = default)
        {
            if (m_SearchModule != null)
            {
                await m_SearchModule.PerformSearch(list, cancellationToken);
            }
        }
    }
}
