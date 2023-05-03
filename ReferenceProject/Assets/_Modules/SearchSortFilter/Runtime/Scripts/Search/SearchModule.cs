using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public interface ISearchModule
    {
        string searchString { get; set; }
        bool ContainsBindPathName(string bindPathName);
    }

    public class SearchModule<T> : ISearchModule
    {
        readonly Dictionary<string, ISearchBindModule<T>> m_AllSearchNodes = new();

        /// <summary>
        ///     will search by all bindPaths
        /// </summary>
        public SearchModule(params (string key, ISearchBindModule<T> bind)[] bindPath) => AddBindings(bindPath);

        public string searchString { get; set; }

        public bool ContainsBindPathName(string bindPathName) => m_AllSearchNodes.ContainsKey(bindPathName);

        public void AddBindings(params (string key, ISearchBindModule<T> bind)[] bindPath)
        {
            foreach (var item in bindPath)
            {
                if(!m_AllSearchNodes.TryAdd(item.key, item.bind))
                    Debug.LogError($"key: {item.key} already exist in dictionary: {nameof(m_AllSearchNodes)}");
            }
        }
        
        public Task PerformSearch(List<T> list)
        {
            if (string.IsNullOrEmpty(searchString) || list == null || list.Count == 0)
                return Task.CompletedTask;

            if (m_AllSearchNodes.Count == 0)
            {
                Debug.LogWarning($"Count of {nameof(ISearchBindModule<T>)}s is 0. Search can not be performed without bindings!");
                return Task.CompletedTask;
            }

            var keys = new List<ISearchBindModule<T>>();

            // Prepare bindPaths
            foreach (var searchNode in m_AllSearchNodes)
            {
                keys.Add(searchNode.Value);
            }

            var r = list.Count - 1; // Right pointer (Everything left before this - contains search word)

            // Search and move not visible elements to the end of list
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var isVisible = false;
                for (var k = 0; k < keys.Count; k++)
                {
                    isVisible |= keys[k].PerformSearch(list[i], searchString);
                }
                
                if (!isVisible)
                {
                    (list[r], list[i]) = (list[i], list[r]); // Swap not visible to the end of list
                    r--;
                }
            }

            // Clean Up everything that right after r
            for (var i = list.Count - 1; i > r; i--)
            {
                list.RemoveAt(i);
            }
            
            return Task.CompletedTask;
        }
    }

    public interface ISearchBindModule<in T>
    {
        bool PerformSearch(T element, string searchString);
    }
    
    public class SearchBindNode<T>: ISearchBindModule<T>
    {
        readonly Func<T, string> m_BindPath;
        readonly StringComparison m_StringComparison;

        public SearchBindNode(Func<T, string> bindPath, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            m_BindPath = bindPath;
            m_StringComparison = stringComparison;
        }

        public bool PerformSearch(T element, string searchString) =>
            m_BindPath(element).IndexOf(searchString, m_StringComparison) >= 0;
    }
}
