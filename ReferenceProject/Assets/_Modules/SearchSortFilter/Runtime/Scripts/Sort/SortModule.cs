using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public interface ISortModule
    {
        string CurrentSortPathName { set; }
        SortOrder SortOrder { get; set; }
    }

    public class SortModule<T> : ISortModule
    {
        readonly Dictionary<string, ISortBindNode<T>> m_SortNodes = new();

        public SortModule(params (string key, ISortBindNode<T> module)[] sortBy)
        {
            foreach (var item in sortBy)
                m_SortNodes.Add(item.key, item.module);
        }

        public string CurrentSortPathName { private get; set; }
        public SortOrder SortOrder { get; set; }
        
        public void AddNode(params (string key, ISortBindNode<T> module)[] sortBy)
        {
            foreach (var item in sortBy)
                m_SortNodes.Add(item.key, item.module);
        }

        public async Task PerformSort(List<T> list) => await PerformSort(list, CurrentSortPathName, SortOrder);

        Task PerformSort(List<T> list, string pathName, SortOrder sortOrder)
        {
            if (string.IsNullOrEmpty(pathName))
                return Task.CompletedTask;

            if (m_SortNodes.TryGetValue(pathName, out var node))
                node.PerformSort(list, sortOrder);
            
            return Task.CompletedTask;
        }
    }

    [Serializable]
    public enum SortOrder
    {
        Ascending = 1,
        Descending = -1,
    }

    public interface ISortBindNode<T>
    {
        void PerformSort(List<T> list, SortOrder sortOrder);
    }

    public abstract class SortBindNodeBase<T, K>
    {
        protected readonly Func<T, K> m_BindPath;

        protected SortBindNodeBase(Func<T, K> bindPath) => m_BindPath = bindPath;
    }
    
    /// <summary>
    /// If BindPath result string is null or empty string, sort will always put it to the end of list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortBindNodeString<T> : SortBindNodeBase<T, string>, ISortBindNode<T>
    {
        readonly StringComparison m_Comparison;
        public SortBindNodeString(Func<T, string> bindPath, StringComparison comparison = StringComparison.Ordinal) : base(bindPath)
        {
            m_Comparison = comparison;
        }

        public void PerformSort(List<T> list, SortOrder sortOrder)
        {
            if (list == null || list.Count == 0)
                return;
            
            list.Sort((x, y) =>
            {
                var xString = m_BindPath(x);
                
                if (string.IsNullOrEmpty(xString))
                    return 1;
                
                var yString = m_BindPath(y);

                if (string.IsNullOrEmpty(yString))
                    return -1;
                
                return sortOrder.GetHashCode() * string.Compare(xString, yString, m_Comparison);
            });
        }
    }

    /// <summary>
    /// If BindPath result value is int.MaxValue, sort will always put it to the end of list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortBindNodeInt<T> : SortBindNodeBase<T, int>, ISortBindNode<T>
    {
        public SortBindNodeInt(Func<T, int> bindPath) : base(bindPath) { }

        public void PerformSort(List<T> list, SortOrder sortOrder)
        {
            if (list == null || list.Count == 0)
                return;
            
            list.Sort((x, y) =>
            {
                var xValue = m_BindPath(x);
                
                if (xValue == int.MaxValue)
                    return 1;
                
                var yValue = m_BindPath(y);
                
                if (yValue == int.MaxValue)
                    return -1;
                
                if (xValue == yValue)
                    return 0;
                
                return sortOrder.GetHashCode() * (xValue > yValue ? 1 : -1);
            });
        }
    }
    
    /// <summary>
    /// If BindPath result value is long.MaxValue, sort will always put it to the end of list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortBindNodeLong<T> : SortBindNodeBase<T, long>, ISortBindNode<T>
    {
        public SortBindNodeLong(Func<T, long> bindPath) : base(bindPath) { }

        public void PerformSort(List<T> list, SortOrder sortOrder)
        {
            if (list == null || list.Count == 0)
                return;
            
            list.Sort((x, y) =>
            {
                var xValue = m_BindPath(x);
                
                if (xValue == long.MaxValue)
                    return 1;
                
                var yValue = m_BindPath(y);
                
                if (yValue == long.MaxValue)
                    return -1;
                
                if (xValue == yValue)
                    return 0;
                
                return sortOrder.GetHashCode() * (xValue > yValue ? 1 : -1);
            });
        }
    }
}
