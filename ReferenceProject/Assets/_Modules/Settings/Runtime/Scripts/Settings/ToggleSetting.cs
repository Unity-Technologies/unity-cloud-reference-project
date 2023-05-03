using System;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.Dt.App.UI.Toggle;

namespace Unity.ReferenceProject.Settings
{
    public class ToggleSetting : BaseSetting
    {
        readonly Action<bool> m_Action;
        readonly Func<bool> m_ToggledValue;
        Toggle m_Toggle;

        public ToggleSetting(string label, Action<bool> action, Func<bool> toggledValue = null, Func<bool> enabledValue = null)
            : base(label, enabledValue)
        {
            m_Action = action;
            m_ToggledValue = toggledValue;
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

            var label = new Header(m_Label)
            {
                size = HeaderSize.XS,
            };
            visualElement.Add(label);

            m_Toggle = new Toggle
            {
                label = null,
                emphasized = true,
                size = Size.L
            };

            m_Toggle.RegisterValueChangedCallback(b => m_Action.Invoke(b.newValue));

            if (m_ToggledValue != null)
            {
                m_Toggle.value = m_ToggledValue.Invoke();
            }

            visualElement.Add(m_Toggle);

            label.AddToClassList(GlobalSettingsStyleClasses.EntryHeaderStyle);
            m_Toggle.AddToClassList(GlobalSettingsStyleClasses.EntryValueStyle);

            return visualElement;
        }

        public void SetValueWithoutNotify(bool isTrue)
        {
            if (m_Toggle != null)
                m_Toggle.SetValueWithoutNotify(isTrue);
        }
    }
}
