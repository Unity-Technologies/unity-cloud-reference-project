using System;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public class SearchUI
    {
        readonly string k_SearchBarName = "searchbar";

        readonly SearchBar m_Searchbar;
        readonly ISearchModule m_SearchModule;

        public SearchUI(ISearchModule searchModule, VisualElement root, Action onSearchWordChanged,
            string searchPlaceHolder = null, string searchbarName = null)
        {
            m_SearchModule = searchModule;
            OnSearchWordChanged = onSearchWordChanged;

            if (string.IsNullOrEmpty(searchbarName))
                searchbarName = k_SearchBarName;

            m_Searchbar = root.Q<SearchBar>(searchbarName);

            if (m_Searchbar == null)
            {
                Debug.LogError($"Can't find {nameof(SearchBar)} with name: {searchbarName}");
                return;
            }

            if (!string.IsNullOrEmpty(searchPlaceHolder))
                m_Searchbar.placeholder = searchPlaceHolder;

            m_Searchbar.RegisterValueChangedCallback(OnSearchBarValueChanged);
        }

        public event Action OnSearchWordChanged;

        public void UnregisterCallbacks()
        {
            if (m_Searchbar != null)
                m_Searchbar.UnregisterValueChangedCallback(OnSearchBarValueChanged);
        }

        void OnSearchBarValueChanged(ChangeEvent<string> evt)
        {
            m_SearchModule.searchString = evt.newValue;
            OnSearchWordChanged?.Invoke();
        }
    }
}
