using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.Cloud.Identity;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    [Serializable]
    public class OrganizationSelector
    {
        [SerializeField]
        StyleSheet m_PopOverStyleSheet;

        [SerializeField]
        VisualTreeAsset m_OrganizationListItemTemplate;
        
        public event Action<IOrganization> OrganizationSelected; 

        public VisualElement RootVisualElement => m_Container;
        
        VisualElement m_OrganizationList;
        VisualElement m_Container;
        VisualElement m_NoResultElement;
        SearchBar m_SearchBar;
        readonly Dictionary<IOrganization, VisualElement> m_OrganizationItems = new ();

        public void CreateVisualTree()
        {
            m_Container = new VisualElement();
            m_Container.styleSheets.Add(m_PopOverStyleSheet);
            m_Container.AddToClassList("container__organization-list");

            m_SearchBar = new SearchBar
            {
                placeholder = "@AssetList:Search"
            };
            m_SearchBar.validateValue += OnValidateSearchValue;
            
            m_Container.Add(m_SearchBar);

            m_OrganizationList = new ScrollView();
            m_OrganizationList.AddToClassList("scroll-view__organization-list");
            
            m_Container.Add(m_OrganizationList);

            m_NoResultElement = new Heading("@AssetList:NoResult")
            {
                size = HeadingSize.XS,
            };
            m_NoResultElement.AddToClassList("text__organization-list-no-result");
            
            m_Container.Add(m_NoResultElement);
        }

        bool OnValidateSearchValue(string arg)
        {
            ApplySearchValue(arg);
            return true;
        }

        void ApplySearchValue(string search)
        {
            var noneVisible = true;
            foreach (var item in m_OrganizationItems)
            {
                var visible = string.IsNullOrEmpty(search) || item.Key.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                Common.Utils.SetVisible(item.Value, visible);
                noneVisible &= !visible;
            }
            
            Common.Utils.SetVisible(m_NoResultElement, noneVisible);
        }

        public void Populate(IEnumerable<IOrganization> organizations)
        {
            m_OrganizationList.Clear();
            m_OrganizationItems.Clear();

            foreach (var organization in organizations)
            {
                var item = m_OrganizationListItemTemplate.CloneTree();
                var button = item.Q<Button>();
                button.title = organization.Name;
                button.clicked += () =>
                {
                    SelectOrganization(organization);
                    OrganizationSelected?.Invoke(organization);
                };

                m_OrganizationList.Add(item);
                m_OrganizationItems.Add(organization, item);
            }
            
            Common.Utils.SetVisible(m_NoResultElement, m_OrganizationItems.Count <= 0);
            
            UnSelectAll();
        }
        
        public void SelectOrganization(IOrganization organization)
        {
            UnSelectAll();

            if(m_OrganizationItems.TryGetValue(organization, out var item))
            {
                CheckMarkVisible(item, true);
            }
        }

        void UnSelectAll()
        {
            foreach(var item in m_OrganizationItems.Values)
            {
                CheckMarkVisible(item, false);
            }
        }

        static void CheckMarkVisible(VisualElement item, bool visible)
        {
            var check = item.Q(Button.trailingIconUssClassName);
            Common.Utils.SetVisible(check, visible);
        }

        public void ClearSearch()
        {
            m_SearchBar.value = string.Empty;
        }
    }
}
