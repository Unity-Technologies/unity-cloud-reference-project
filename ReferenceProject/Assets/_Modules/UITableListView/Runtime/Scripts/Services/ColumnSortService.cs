using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class ColumnSortService : MonoBehaviour, IService
    {
        [SerializeField]
        int m_ServicePriority;

        [SerializeField]
        string m_DefaultSortColumnName;

        SortModuleUI m_SortModuleUI;
        SortModule<object> m_SortModule;

        public event Action OnRefresh;
        public int ServicePriority => m_ServicePriority;
        
        void OnDestroy() => ClearService();

        public void ClearService()
        {
            m_SortModuleUI?.UnregisterCallbacks();
        }

        public void InitializeService(VisualElement root)
        {
            m_SortModule = new SortModule<object>();

            m_SortModuleUI = new SortModuleUI(m_SortModule, root, OnRefresh, m_DefaultSortColumnName);
        }
        public void InitializeUIForData(VisualElement e, IServiceData data)
        {
            foreach (var serviceData in data.ServiceData)
            {
                if (serviceData is ISortBindNode<object> bindNode)
                {
                    m_SortModule.AddNode((data.Name, bindNode));

                    // Add to the UI
                    m_SortModuleUI.AddSortButton(e, data.Name, data.Name);
                }
            }
        }
        
        public void OnPrimaryKeysCreated(List<object> list)
        {
            // Empty
        }

        public async Task PerformService(List<object> list)
        {
            if (m_SortModule != null)
            {
                await m_SortModule.PerformSort(list);
            }
        }
    }
}
