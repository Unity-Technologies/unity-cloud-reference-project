using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools
{
    public class AdaptiveMenu : MonoBehaviour
    {
        [SerializeField]
        ToolUIMenu m_ToolUIMenu;
        
        [SerializeField]
        ToolData m_ExpandMenuToolData;
        
        List<ActionButton> m_VisibleActionButtons;
        List<ActionButton> m_PopoverActionButtons;
        List<ActionButton> m_ButtonsToolbar;
        
        VisualElement m_PanelContainer;
        VisualElement m_Root;
        VisualElement m_ToolbarSection;
        ActionButton m_ExpandButton;
        AdaptiveMenuToolUIController m_AdaptiveMenuToolUIController;
        Dictionary<ActionButton, int> m_ButtonInitialOrder;
        float m_SizeButton;
        bool m_ExpandButtonAdded;
        
        const string k_FirstChildStyle = "unity-first-child";
        const string k_LastChildStyle = "unity-last-child";
        const string k_ActionGroupInBetweenStyle = "appui-actiongroup__inbetween-item";
        const string k_ActionGroupItemStyle = "appui-actiongroup__item";
        const string k_AdaptiveButtonStyle = "adaptive-popover-button";
        
        void Awake()
        {
            m_VisibleActionButtons = new List<ActionButton>();
            m_PopoverActionButtons = new List<ActionButton>();
            m_ButtonsToolbar = new List<ActionButton>();
            m_ToolUIMenu.UICreated += OnUICreated;
            m_AdaptiveMenuToolUIController = (AdaptiveMenuToolUIController)m_ExpandMenuToolData.ToolUIController;
            m_SizeButton = 0f;
        }
        
        void OnUICreated()
        {
            m_Root = m_ToolUIMenu.Root;
            m_ButtonsToolbar = m_ToolUIMenu.ButtonsToolbar;
            m_PanelContainer = m_ToolUIMenu.PanelContainer;
            m_ToolbarSection = m_Root.Q<VisualElement>(m_ExpandMenuToolData.ToolbarElementName);
            
            m_Root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_ToolbarSection.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            
            m_ButtonInitialOrder = new Dictionary<ActionButton, int>();
            foreach (var button in m_ButtonsToolbar)
            {
                m_ButtonInitialOrder.Add(button, m_ButtonsToolbar.IndexOf(button));
                button.clickable.clicked += () => UpdateListOnClick(button);
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_ExpandButtonAdded)
            {
                m_AdaptiveMenuToolUIController.ClosePopOver();
                m_ExpandButton = null;
                m_ExpandButtonAdded = false;
            }
            
            AdaptToolbar();
            UpdateStyle();
        }
        
        void AdaptToolbar()
        {
            if (!m_ExpandButtonAdded  && m_ToolbarSection != null)
            {
                SortButtons();

                if (m_PopoverActionButtons.Count > 0)
                {
                    var buttonContainer = string.IsNullOrEmpty(m_ExpandMenuToolData.ToolbarElementName) ? 
                        null : m_Root.Q<VisualElement>(m_ExpandMenuToolData.ToolbarElementName);
                    var handler = m_ToolUIMenu.AddTool(m_PanelContainer, buttonContainer, m_ExpandMenuToolData);
                    
                    m_ToolUIMenu.AddHandler(handler);
                    m_ExpandButton = handler.GetButton();
                    m_ButtonsToolbar.Remove(m_ExpandButton);
                    
                    m_ExpandButton.AddToClassList(k_ActionGroupItemStyle);
                    m_ExpandButton.visible = true;
                    m_ExpandButtonAdded = true;
                }
                
                m_AdaptiveMenuToolUIController.UpdateExpandTool(m_PopoverActionButtons);
                UpdateMenuUI();
            }
        }
        
        void SortButtons()
        {
            var sizeMax = m_ToolbarSection.layout.height;
            var currentSize = 0f;
            var defaultHeight = m_ButtonsToolbar.Select(button => button.layout.height).Prepend(0f).Max();
            m_SizeButton = Math.Max(m_SizeButton, defaultHeight);

            m_PopoverActionButtons.Clear();
            m_VisibleActionButtons.Clear();

            foreach (var button in m_ButtonsToolbar)
            {
                currentSize += m_SizeButton;
                    
                if (currentSize + m_SizeButton > sizeMax)
                {
                    if (button.selected && m_VisibleActionButtons.Count > 0)
                    {
                        var visibleDelete = m_VisibleActionButtons[m_VisibleActionButtons.Count-1];
                        m_VisibleActionButtons.RemoveAt(m_VisibleActionButtons.Count-1);
                        m_PopoverActionButtons.Add(visibleDelete);
                        m_VisibleActionButtons.Add(button);
                    }
                    else
                    {
                        m_PopoverActionButtons.Add(button);
                    }
                }
                else
                {
                    m_VisibleActionButtons.Add(button);
                }
            }
        }
        
        void UpdateMenuUI()
        {
            m_ToolbarSection.Clear();
            RestoreInitialOrder(m_VisibleActionButtons);
            foreach (var button in m_VisibleActionButtons)
            {
                m_ToolbarSection.Add(button);
            }
            
            if (m_ExpandButton != null)
            {
                m_ToolbarSection.Add(m_ExpandButton);
            }
            
            UpdateStyle();
        }
        
        void UpdateStyle()
        {
            for (var i = 0; i <= m_VisibleActionButtons.Count-1; i++)
            {
                var button = m_VisibleActionButtons[i];
                button.RemoveFromClassList(k_ActionGroupInBetweenStyle);
                button.RemoveFromClassList(k_FirstChildStyle);
                button.RemoveFromClassList(k_LastChildStyle);
                button.RemoveFromClassList(k_AdaptiveButtonStyle);
                button.AddToClassList(k_ActionGroupItemStyle);
                button.label = "";
                
                if (i == 0)
                {
                    button.AddToClassList(k_FirstChildStyle);
                    if (m_VisibleActionButtons.Count == 1 && m_ExpandButton == null)
                    {
                        button.AddToClassList(k_LastChildStyle);
                    }
                }
                else 
                {
                    button.AddToClassList(k_ActionGroupInBetweenStyle);
                    if (i == m_VisibleActionButtons.Count - 1 && m_ExpandButton == null)
                    {
                        button.AddToClassList(k_LastChildStyle);
                        
                    }
                }
            }
            
            if (m_ExpandButton != null)
            {
                m_ExpandButton.RemoveFromClassList(k_FirstChildStyle);
                m_ExpandButton.AddToClassList(k_LastChildStyle);
                    
                if(m_VisibleActionButtons.Count == 0)
                {
                    m_ExpandButton.AddToClassList(k_FirstChildStyle);
                }
            }
        }
        
        void UpdatePopoverList(ActionButton button)
        {
            if (m_PopoverActionButtons.Contains(button) && m_VisibleActionButtons.Count > 1) 
            {
                var visibleDelete = m_VisibleActionButtons[^1];
                m_PopoverActionButtons.Remove(button);
                m_VisibleActionButtons.RemoveAt(m_VisibleActionButtons.Count - 1);
                m_PopoverActionButtons.Add(visibleDelete);
                m_VisibleActionButtons.Insert(0, button);

                m_ButtonsToolbar.Remove(button);
                m_ButtonsToolbar.Insert(0, button);

                RestoreInitialOrder( m_ButtonsToolbar);
                RestoreInitialOrder(m_PopoverActionButtons);
                m_AdaptiveMenuToolUIController.UpdateExpandTool(m_PopoverActionButtons);

                UpdateStyle();
                UpdateMenuUI();
            }
        }
        
        void RestoreInitialOrder(List<ActionButton> buttonList)
        {
            buttonList.Sort((button1, button2) => m_ButtonInitialOrder[button1].CompareTo(m_ButtonInitialOrder[button2]));
        }
        
        void UpdateListOnClick(ActionButton button)
        {
            button.label = "";
            UpdatePopoverList(button);
        }
    }
}
