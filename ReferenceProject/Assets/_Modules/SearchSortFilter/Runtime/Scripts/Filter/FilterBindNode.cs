using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public class FilterModule<T>
    {
        readonly Dictionary<string, HashSet<string>> SelectedOptions = new();

        readonly Dictionary<string, IFilterBindNode<T>> m_AllFilterNodes = new();

        public Dictionary<string, IFilterBindNode<T>> AllFilterNodes => m_AllFilterNodes;

        public bool ContainsOption(string key, string value)
        {
            return SelectedOptions.ContainsKey(key) && SelectedOptions[key].Contains(value);
        }

        public void ClearAllOptions() => SelectedOptions.Clear();

        public int CountSelectedOptions(string key) => !SelectedOptions.ContainsKey(key) ? 0 : SelectedOptions[key].Count;

        public void AddSelectedOption(string key, string value)
        {
            if(!SelectedOptions.ContainsKey(key))
                SelectedOptions.Add(key, new HashSet<string>());
            SelectedOptions[key].Add(value);
        }

        public void RemoveSelectedOption(string key, string value)
        {
            if(SelectedOptions.ContainsKey(key))
                SelectedOptions[key].Remove(value);
        }

        public FilterModule(params (string key, IFilterBindNode<T> bind)[] bindPath) => AddBindings(bindPath);

        public void AddBindings(params (string key, IFilterBindNode<T> bind)[] bindPath)
        {
            foreach (var item in bindPath)
            {
                if(!m_AllFilterNodes.TryAdd(item.key, item.bind))
                    Debug.LogError($"key: {item.key} already exist in dictionary: {nameof(m_AllFilterNodes)}");
            }
        }

        public Task PerformFiltering(List<T> list)
        {
            if (SelectedOptions.Count == 0 || list == null || list.Count == 0)
                return Task.CompletedTask;

            if (m_AllFilterNodes.Count == 0)
            {
                Debug.LogWarning($"Count of {nameof(IFilterBindNode<T>)}s is 0. Filtering can not be performed without bindings!");
                return Task.CompletedTask;
            }

            int r = list.Count - 1; // Right pointer (Everything right after this - should be removed)

            for (int i = list.Count - 1; i >= 0; i--)
            {
                foreach (var filterNode in m_AllFilterNodes)
                {
                    if (SelectedOptions.TryGetValue(filterNode.Key, out var filterSet) && !filterNode.Value.PerformFiltering(list[i], filterSet))
                    {
                        (list[r], list[i]) = (list[i], list[r]); // Swap not visible to the end of list
                        r--;
                        break;
                    }
                }
            }

            // Clean Up everything that right after r
            for (int i = list.Count - 1; i > r; i--)
            {
                list.RemoveAt(i);
            }

            return Task.CompletedTask;
        }
    }

    public interface IFilterBindNode<T>
    {
        string SelectedOption { get; set; }
        Func<T, string> bindPath { get; }
        Task PerformFiltering(List<T> list);
        bool PerformFiltering(T element, HashSet<string> filterStringSet);
    }

    [Serializable]
    public enum FilterCompareType
    {
        Equals = 0,
        NotEquals = 1,
    }

    public class FilterBindNode<T> : IFilterBindNode<T>
    {
        Predicate<(string, string)> m_CompareFormula;

        readonly FilterCompareType m_CompareType;

        public FilterBindNode(Func<T, string> bindPath, FilterCompareType compareType)
        {
            this.bindPath = bindPath;
            SetCompareType(compareType);
            m_CompareType = compareType;
        }

        public string SelectedOption { get; set; } // if null then no filtering
        public Func<T, string> bindPath { get; }

        public void SetCompareType(FilterCompareType compareType)
        {
            switch (compareType)
            {
                case FilterCompareType.Equals:
                    m_CompareFormula = x => x.Item2.Equals(x.Item1);
                    break;
                case FilterCompareType.NotEquals:
                    m_CompareFormula = x => !x.Item2.Equals(x.Item1);
                    break;
            }
        }

        public Task PerformFiltering(List<T> list)
        {
            if (string.IsNullOrEmpty(SelectedOption) || list == null || list.Count == 0)
                return Task.CompletedTask;

            var r = list.Count - 1; // Right pointer (Everything right after this - should be removed)

            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (!m_CompareFormula((bindPath(list[i]), SelectedOption)))
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

        public bool PerformFiltering(T element, HashSet<string> filterStringSet)
        {
            bool contains = filterStringSet.Contains(bindPath(element));

            switch (m_CompareType)
            {
                case FilterCompareType.Equals:
                    return contains;
                case FilterCompareType.NotEquals:
                    return !contains;
            }
            return contains;
        }
    }
}
