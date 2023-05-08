using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class ColumnFilterService : MonoBehaviour, IService
    {
        [SerializeField]
        int m_ServicePriority;
        
        [SerializeField]
        string m_DropdownName;
        
        [SerializeField]
        string m_DefaultValue;

        FilterSingleUI<object> m_FilterSingleUI;
        IFilterBindNode<object> m_FilterBindNode;

        VisualElement m_Root;

        public event Action OnRefresh;
        public int ServicePriority => m_ServicePriority;
        
        void OnDestroy() => ClearService();

        public void ClearService()
        {
            m_FilterSingleUI?.UnregisterCallbacks();
            m_FilterSingleUI = null;
            if (m_FilterBindNode != null)
            {
                m_FilterBindNode.SelectedOption = null;
            }
            
            m_FilterBindNode = null;
            m_Root = null;
        }

        public void InitializeService(VisualElement root)
        {
            m_Root = root;
        }
        public void InitializeUIForData(VisualElement e, IServiceData data) 
        {
            foreach (var serviceData in data.ServiceData)
            {
                if (serviceData is IFilterBindNode<object> filterNode)
                {
                    m_FilterBindNode = filterNode;
                    m_FilterSingleUI = new FilterSingleUI<object>(m_FilterBindNode, m_Root ?? e,
                        OnRefresh, null, m_DefaultValue, m_DropdownName);
                }
            }
        }
        
        public void OnPrimaryKeysCreated(List<object> list)
        {
            m_FilterSingleUI?.SetFilterOptions(list);
        }

        public async Task PerformService(List<object> list)
        {
            if (m_FilterBindNode != null)
            {
                await m_FilterBindNode.PerformFiltering(list);
            }
        }
    }
}
