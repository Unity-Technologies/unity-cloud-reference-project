using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public class SortModuleUI
    {
        const string k_ArrowDecreaseStyle = "sort-arrow-decrease";
        const string k_ArrowIncreaseStyle = "sort-arrow-increase";
        const string k_ArrowDisableStyle = "sort-arrow-disabled";
        const string k_ArrowStyle = "sort-arrow";

        readonly List<VisualElement> m_ButtonList = new ();
        readonly Dictionary<string, VisualElement> m_ButtonDictionary = new ();

        readonly Dictionary<string, string> m_BindPathNames = new();

        readonly string m_DefaultSortName;

        readonly ISortModule m_SortModule;

        VisualElement m_LastClickedElement;

        public SortModuleUI(ISortModule sortModule, VisualElement root, Action onSortChanged, string defaultSort = "",
            params (string bindPathName, string uiElementName)[]
                keys)
        {
            OnSortChanged += onSortChanged;
            m_SortModule = sortModule;
            m_DefaultSortName = defaultSort;

            if(keys != null)
            {
                foreach (var key in keys)
                {
                    AddSortButton(root, key.bindPathName, key.uiElementName);
                }
            }
        }

        public event Action OnSortChanged;

        public void AddSortButton(VisualElement root, string bindPathName, string uiElementName)
        {
            var button = root.Q<VisualElement>(uiElementName);
            if (button == null)
            {
                Debug.LogError($"Can't find {nameof(VisualElement)} by name: {uiElementName}");
                return;
            }
                
            m_ButtonList.Add(button);
            m_ButtonDictionary.Add(uiElementName, button);

            button.RegisterCallback<ClickEvent>(OnClick);
            button.AddToClassList(k_ArrowDisableStyle);
            m_BindPathNames.Add(uiElementName, bindPathName);
            
            CreateArrow(button);
            
            if (!string.IsNullOrEmpty(m_DefaultSortName) && m_DefaultSortName == bindPathName)
            {
                PerformSort(bindPathName);
            }
        }

        void CreateArrow(VisualElement button)
        {
            var icon = new Icon();
            icon.name = "SortArrow";
            icon.iconName = "arrow-down";
            icon.AddToClassList(k_ArrowStyle);
            button.Add(icon);
        }
        
        public void UnregisterCallbacks()
        {
            if (m_ButtonList.Count > 0)
            {
                foreach (var button in m_ButtonList)
                {
                    button.UnregisterCallback<ClickEvent>(OnClick);
                }
            }
            m_ButtonDictionary.Clear();
        }

        void OnClick(ClickEvent evt)
        {
            if (evt.propagationPhase != PropagationPhase.AtTarget)
                return;

            if (evt.target is not VisualElement targetBlock)
            {
                Debug.LogError($"Can't get {nameof(VisualElement)} from picked UI element!");
                return;
            }
            PerformSort(targetBlock.name);
        }

        public void PerformSort(string uiElementName)
        {
            if(m_ButtonDictionary.TryGetValue(uiElementName, out var targetBlock))
            {
                if (targetBlock != m_LastClickedElement)
                    m_SortModule.SortOrder = SortOrder.Ascending; // picked new element
                else
                    m_SortModule.SortOrder = m_SortModule.SortOrder != SortOrder.Ascending
                        ? SortOrder.Ascending
                        : SortOrder.Descending; // picked same element
                
                if (m_BindPathNames.TryGetValue(uiElementName, out var bindPathName))
                    m_SortModule.CurrentSortPathName = bindPathName;

                // Change visual arrow
                if (m_LastClickedElement != null)
                {
                    m_LastClickedElement.RemoveFromClassList(k_ArrowDecreaseStyle);
                    m_LastClickedElement.RemoveFromClassList(k_ArrowIncreaseStyle);
                    m_LastClickedElement.AddToClassList(k_ArrowDisableStyle);
                }
                
                targetBlock.RemoveFromClassList(k_ArrowDisableStyle);
                targetBlock.AddToClassList(m_SortModule.SortOrder == SortOrder.Ascending
                    ? k_ArrowIncreaseStyle
                    : k_ArrowDecreaseStyle);

                m_LastClickedElement = targetBlock;

                OnSortChanged?.Invoke();
            }
        }
    }
}
