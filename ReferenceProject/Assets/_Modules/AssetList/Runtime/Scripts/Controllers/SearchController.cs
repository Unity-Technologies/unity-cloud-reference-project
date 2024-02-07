using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public class SearchController : MonoBehaviour
    {
        string m_CurrentSearchValue;
        public string CurrentSearchValue => m_CurrentSearchValue;

        public event Action<string> SearchValueChanged;

        public void InitUIToolkit(VisualElement root)
        { 
            var searchBar = root.Q<SearchBar>("SearchBar");
            searchBar.validateValue += OnValidateSearchValue;
        }

        bool OnValidateSearchValue(string arg)
        { 
            m_CurrentSearchValue = arg; 
            SearchValueChanged?.Invoke(m_CurrentSearchValue);
            return true;
        }
    }
}
