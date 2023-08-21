using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools
{
    public class AdaptiveMenuToolUIController : ToolUIController
    {
        List<ActionButton> m_CurrentButtons = new List<ActionButton>();
        
        VisualElement m_Root;

        const string k_AdaptiveButtonStyle = "adaptive-popover-button";
        
        public override void OnToolOpened()
        {
            UpdateUI();
        }

        public void UpdateExpandTool(List<ActionButton> list)
        {
            m_CurrentButtons = list;
            UpdateUI();
        }

        void UpdateUI()
        {
            if (m_Root != null)
            {
                m_Root.Clear();
                foreach (var button in m_CurrentButtons)
                {
                    button.label = button.name;
                    button.AddToClassList(k_AdaptiveButtonStyle);
                    m_Root.Add(button);
                }
            }
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            m_Root = base.CreateVisualTree(template);

            foreach (var button in m_CurrentButtons)
            {
                button.label = button.name;
                button.AddToClassList(k_AdaptiveButtonStyle);
                m_Root.Add(button);
            }
            return m_Root;
        }

        public void ClosePopOver()
        {
            CloseSelf();
        }
    }
}
