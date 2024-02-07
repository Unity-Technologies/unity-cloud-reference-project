using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cloud.Identity;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Unity.ReferenceProject.CustomKeyboard;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class OrganizationUIController : MonoBehaviour
    {
        [SerializeField]
        OrganizationSelector m_OrganizationSelector;

        readonly string k_OrganizationButton = "OrganizationButton";

        Button m_OrganizationButton;

        Popover m_Popover;

        public event Action<IOrganization> OrganizationSelected;

        void Awake()
        {
            m_OrganizationSelector.CreateVisualTree();
            m_OrganizationSelector.OrganizationSelected += organization => OrganizationSelected?.Invoke(organization);

            var keyboardHandler = GetComponentInParent<KeyboardHandler>();
            if (keyboardHandler != null)
            {
                keyboardHandler.RegisterRootVisualElement(m_OrganizationSelector.RootVisualElement);
            }
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_OrganizationButton = root.Q<Button>(k_OrganizationButton);
            var titleContainer = m_OrganizationButton.Q<VisualElement>(Button.titleContainerUssClassName);
            titleContainer.style.alignItems = Align.FlexStart;
            titleContainer.style.flexDirection = FlexDirection.ColumnReverse;
            m_OrganizationButton.clicked += OpenPopover;
            m_OrganizationButton.SetEnabled(false);
        }

        public void Populate(IEnumerable<IOrganization> organizations)
        {
            m_OrganizationSelector.Populate(organizations);

            m_OrganizationButton.SetEnabled(organizations != null && organizations.Any());
        }

        public void SelectOrganization(IOrganization organization)
        {
            m_OrganizationButton.title = organization.Name;
            m_Popover?.Dismiss();
            m_OrganizationSelector.SelectOrganization(organization);
        }

        void OpenPopover()
        {
            m_OrganizationSelector.ClearSearch();
            m_Popover = Popover.Build(m_OrganizationButton, m_OrganizationSelector.RootVisualElement)
                .SetPlacement(PopoverPlacement.Bottom);
            m_Popover.Show();
        }
    }
}
