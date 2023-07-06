using System;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Settings
{
    public class ActionSetting : BaseSetting
    {
        readonly Action m_Action;

        public ActionSetting(string label, Action action, Func<bool> enabledValue = null)
            : base(label, enabledValue)
        {
            m_Action = action;
        }

        public override VisualElement CreateVisualTree()
        {
            var button = new ActionButton { label = m_Label };
            button.clickable.clicked += m_Action;

            return button;
        }
    }
}
