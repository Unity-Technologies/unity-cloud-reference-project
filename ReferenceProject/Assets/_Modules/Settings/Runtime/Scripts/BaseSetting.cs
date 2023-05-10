using System;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Settings
{
    public abstract class BaseSetting : ISetting
    {
        protected readonly Func<bool> m_EnabledValue;
        protected readonly string m_Label;

        protected BaseSetting(string label, Func<bool> enabledValue)
        {
            m_Label = label;
            m_EnabledValue = enabledValue;
        }

        public abstract VisualElement CreateVisualTree();

        public bool IsEnabled => m_EnabledValue == null || m_EnabledValue.Invoke();
    }
}
