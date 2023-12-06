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
            SearchWordChanged = onSearchWordChanged;

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

            m_Searchbar.RegisterValueChangingCallback(OnSearchBarValueChanging);
        }

        public event Action SearchWordChanged;

        public void UnregisterCallbacks()
        {
            if (m_Searchbar != null)
                m_Searchbar.UnregisterValueChangingCallback(OnSearchBarValueChanging);
        }

        void OnSearchBarValueChanging(ChangingEvent<string> evt)
        {
            m_SearchModule.searchString = evt.newValue;
            SearchWordChanged?.Invoke();
        }
    }
}
