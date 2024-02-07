using System;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Settings
{
    public class DropdownSetting : BaseSetting
    {
        readonly Action<int> m_Action;
        readonly Func<int> m_SelectedValue;

        readonly string[] m_SourceItems;
        Dropdown m_Dropdown;

        public DropdownSetting(string label, string[] sourceItems, Action<int> action, Func<int> selectedValue = null, Func<bool> enabledValue = null)
            : base(label, enabledValue)
        {
            m_SourceItems = sourceItems;
            m_Action = action;
            m_SelectedValue = selectedValue;
        }

        public override VisualElement CreateVisualTree()
        {
            var visualElement = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween
                }
            };

            var label = new Heading(m_Label)
            {
                size = HeadingSize.XS,
            };
            visualElement.Add(label);

            var dropdown = new Dropdown
            {
                bindItem = (item, i) => item.label = m_SourceItems[i],
                sourceItems = m_SourceItems
            };
            dropdown.RegisterValueChangedCallback(b => m_Action.Invoke(b.newValue.First()));
            m_Dropdown = dropdown;

            if (m_SelectedValue != null)
            {
                dropdown.value = new []{ m_SelectedValue.Invoke() };
            }

            visualElement.Add(dropdown);

            label.AddToClassList(GlobalSettingsStyleClasses.EntryHeaderStyle);
            dropdown.AddToClassList(GlobalSettingsStyleClasses.EntryValueStyle);

            return visualElement;
        }

        public void SetValue(int index)
        {
            m_Dropdown.value = new []{ index };
        }
    }
}
