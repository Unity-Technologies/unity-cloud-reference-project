using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Unity.ReferenceProject.UITableListView
{
    public interface IService
    {
        event Action OnRefresh;
        int ServicePriority { get; }
        void ClearService();
        void InitializeService(VisualElement root);
        void InitializeUIForData(VisualElement e, IServiceData data);
        void OnPrimaryKeysCreated(List<object> list);
        Task PerformService(List<object> list, CancellationToken cancellationToken = default);
    }
    
    public class ListTableServiceController
    {
        TableListView m_Table;
        VisualElement m_LoadingIndicator;
        
        List<object> m_ItemsSource;
        readonly List<object> m_ItemsSourceCache = new ();
        readonly SortedList<int, IService> m_Services = new ();
        readonly MonoBehaviour m_MonoBehaviour;
        
        CancellationTokenSource m_CancellationTokenSource;

        public List<object> itemsSource
        {
            get => m_ItemsSourceCache;
            set
            {
                CreateCachedItemsSource(value);
                foreach (var service in m_Services)
                {
                    service.Value?.OnPrimaryKeysCreated(m_ItemsSource);
                }
                m_Table.ItemsSource = m_ItemsSourceCache;
            }
        }
        
        public ListTableServiceController(MonoBehaviour monoBehaviour, IService[] services)
        {
            m_MonoBehaviour = monoBehaviour;
            foreach (var service in services)
            {
                AddService(service);
            }
        }
        
        public void Initialize(VisualElement root, TableListView table, VisualElement loading, params IColumnEventData[] columns)
        {
            m_Table = table;
            m_LoadingIndicator = loading;
            
            foreach (var service in m_Services.Select(service => service.Value))
            {
                service?.ClearService();
                service?.InitializeService(root);
            }

            SetColumns(columns);
        }

        void CreateCachedItemsSource(List<object> itemsSource)
        {
            m_ItemsSource = itemsSource ?? new List<object>();
            m_ItemsSourceCache.Clear();
            foreach (var item in m_ItemsSource)
            {
                m_ItemsSourceCache.Add(item);
            }
        }

        void SetColumns(IColumnEventData[] m_Columns)
        {
            m_Table.SetColumns(m_Columns);

            foreach (var column in m_Columns)
            {
                if (column == null || !column.IsVisible || column is not IServiceData data) 
                    continue;
                
                foreach (var service in m_Services)
                {
                    service.Value?.InitializeUIForData(m_Table, data);
                }
            }
        }

        void AddService(IService service)
        {
            if (!m_Services.ContainsValue(service))
            {
                m_Services.Add(service.ServicePriority, service);
                service.OnRefresh += RefreshServices;
            }
        }

        public void RefreshServices()
        {
            m_MonoBehaviour.StartCoroutine(RefreshServicesCoroutine());
        }

        IEnumerator RefreshServicesCoroutine()
        {
            // Show loading indicator
            SetVisible(m_LoadingIndicator, true);  
            
            // Hide list
            SetVisible(m_Table, false);  
            
            // Make cancellation token
            var cancellationTokenSource = new CancellationTokenSource();
            
            // If there is an old cancellation token source then cancel it
            m_CancellationTokenSource?.Cancel();
            m_CancellationTokenSource = cancellationTokenSource;

            CreateCachedItemsSource(m_ItemsSource);
            
            var m_Task = RefreshServicesAsync(cancellationTokenSource.Token);
            yield return new WaitWhile(() => !m_Task.IsCompleted);
            
            // Manage token
            if (m_CancellationTokenSource == cancellationTokenSource)
            {
                m_CancellationTokenSource = null;
            }
            
            cancellationTokenSource.Dispose();
            
            // Show Exception if it exists
            if (m_Task.Exception != null)
            {
                Debug.LogError($"Exception: {m_Task.Exception.Message}");
            }
            
            // Show cancellation if it exists
            if (m_Task.IsCanceled)
            {
                Debug.LogWarning($"Operation has been canceled");
                yield break;
            }

            // Hide loading indicator
            SetVisible(m_LoadingIndicator, false);
            
            // Show list
            m_Table.RefreshItems();
            SetVisible(m_Table, true);
        }
        
        async Task RefreshServicesAsync(CancellationToken cancellationToken)
        {
            foreach (var service in m_Services)
            {
                await service.Value.PerformService(m_ItemsSourceCache, cancellationToken);
            }
        }
        
        void SetVisible(VisualElement e, bool isVisible)
        {
            if(e == null)
                return;
            
            e.style.visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
