using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class ColumnFilterServiceTable : MonoBehaviour, IService
    {
        [SerializeField]
        string m_ButtonName;

        [SerializeField]
        int m_ServicePriority;
        
        [Header("Styles")]
        [SerializeField]
        StyleSheet m_StyleSheet;
        
        [SerializeField]
        string m_PopoverStyle;
        
        [SerializeField]
        string m_FilterHeaderStyle;
        
        [SerializeField]
        string m_FilterRowStyle;
        
        [SerializeField]
        string[] m_ColumnStyles;

        FilterModule<object> m_FilterModule;
        FilterTableUI<object> m_FilterTableUI;
        
        List<object> m_ListKeys;
        
        public event Action Refresh;
        public int ServicePriority => m_ServicePriority;

        void OnDestroy() => ClearService();
        
        public void ClearService()
        {
            m_FilterTableUI?.Unsubscribe();
            m_FilterTableUI = null;
            m_FilterModule = null;
            m_ListKeys = null;
           
        }

        public void InitializeService(VisualElement root)
        {
            m_FilterModule = new FilterModule<object>();
            m_FilterTableUI = new FilterTableUI<object>(m_FilterModule, root, Refresh, m_ButtonName, m_StyleSheet, m_ColumnStyles);
            m_FilterTableUI.SetStylesToHeader(m_FilterHeaderStyle);
            m_FilterTableUI.SetStylesToRow(m_FilterRowStyle);
            m_FilterTableUI.SetStylesToPopover(m_PopoverStyle);
        }

        public void InitializeUIForData(VisualElement e, IServiceData data)
        {
            foreach (var serviceData in data.ServiceData)
            {
                if (serviceData is not IFilterBindNode<object> bindNode)
                    continue;

                m_FilterModule?.AddBindings((data.Name, bindNode));
            }
            
            m_FilterTableUI?.UpdateColumns(m_ListKeys);
        }

        public void OnPrimaryKeysCreated(List<object> list)
        {
            m_ListKeys = list;
            m_FilterTableUI?.UpdateColumns(list);
            m_FilterTableUI?.SetOptionsState(true);
        }

        public Task PerformService(List<object> list, CancellationToken cancellationToken = default) => m_FilterModule?.PerformFiltering(list, cancellationToken);
    }
}
