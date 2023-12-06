using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.ReferenceProject.SearchSortFilter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UITableListView
{
    public class ColumnSortService : MonoBehaviour, IService
    {
        public const string PlayerPrefSortPathName = "SortModule.SortPath";
        public const string PlayerPrefSortOrder = "SortModule.SortOrder";

        [SerializeField]
        int m_ServicePriority;

        [SerializeField]
        string m_DefaultSortColumn;

        SortModuleUI m_SortModuleUI;
        SortModule<object> m_SortModule;

        public event Action Refresh;
        public int ServicePriority => m_ServicePriority;

        void OnDestroy() => ClearService();

        public void ClearService()
        {
            m_SortModuleUI?.UnregisterCallbacks();
        }

        public void InitializeService(VisualElement root)
        {
            m_SortModule = new SortModule<object>();

            var initialSorting = PlayerPrefs.GetString(PlayerPrefSortPathName);
            var sortOrder = PlayerPrefs.GetString(PlayerPrefSortOrder);

            m_SortModule.CurrentSortPathName = string.IsNullOrEmpty(initialSorting) ? m_DefaultSortColumn: initialSorting;
            m_SortModule.SortOrder = Enum.TryParse<SortOrder>(sortOrder, out var value ) ? value : SortOrder.Ascending;

            m_SortModuleUI = new SortModuleUI(m_SortModule, root, Refresh);
            m_SortModuleUI.SortChanged += WritePlayerPrefs;
        }

        void WritePlayerPrefs()
        {
            PlayerPrefs.SetString(PlayerPrefSortPathName, m_SortModule.CurrentSortPathName);
            PlayerPrefs.SetString(PlayerPrefSortOrder, m_SortModule.SortOrder.ToString());
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

        public async Task PerformService(List<object> list, CancellationToken cancellationToken = default)
        {
            if (m_SortModule != null)
            {
                await m_SortModule.PerformSort(list, cancellationToken);
            }
        }
    }
}
