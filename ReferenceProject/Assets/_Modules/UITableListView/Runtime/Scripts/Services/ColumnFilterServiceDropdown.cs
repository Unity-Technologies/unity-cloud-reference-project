using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class ColumnFilterServiceDropdown : MonoBehaviour, IService
    {
        [SerializeField]
        int m_ServicePriority;
        
        [SerializeField]
        string m_DropdownName;
        
        [SerializeField]
        string m_DefaultValue;
        
        [SerializeField]
        string m_FilterColumnName;

        FilterSingleUI<object> m_FilterSingleUI;
        FilterModule<object> m_FilterModule;
        
        List<object> m_ListKeys;

        public event Action OnRefresh;
        public int ServicePriority => m_ServicePriority;
        
        void OnDestroy() => ClearService();

        public void ClearService()
        {
            m_FilterSingleUI?.UnregisterCallbacks();
            m_FilterSingleUI = null;
            
            m_FilterModule?.ClearAllOptions();
            m_FilterModule = null;
        }

        public void InitializeService(VisualElement root)
        {
            m_FilterModule = new FilterModule<object>();
            
            // Filter UI setup
            m_FilterSingleUI = new FilterSingleUI<object>(m_FilterModule, root,
                OnRefresh, null, m_DefaultValue, m_DropdownName, m_FilterColumnName);
        }
        
        public void InitializeUIForData(VisualElement e, IServiceData data) 
        {
            foreach (var serviceData in data.ServiceData)
            {
                if (serviceData is IFilterBindNode<object> bindNode)
                {
                    m_FilterModule?.AddBindings((data.Name, bindNode));
                }
            }
            
            m_FilterSingleUI?.SetFilterOptions(m_ListKeys);
        }
        
        public void OnPrimaryKeysCreated(List<object> list)
        {
            m_ListKeys = list;
            m_FilterSingleUI?.SetFilterOptions(list);
        }

        public async Task PerformService(List<object> list, CancellationToken cancellationToken = default)
        {
            if (m_FilterModule != null)
            {
                await m_FilterModule.PerformFiltering(list, cancellationToken);
            }
        }
    }
}
