using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasureItem : IMeasureListItem
    {
        readonly MeasureLineData m_Data;
        readonly VisualElement m_Container;
        readonly ActionButton m_ViewButton;

        static readonly string k_ShownIcon = "eye";
        static readonly string k_HiddenIcon = "eye-slash";

        public string Id { get; set; }
        public bool IsShown { get; private set; }
        public VisualElement Root { get; }
        public MeasureLineData Data => m_Data;

        public Action<MeasureLineData> OnClick { get; set; }
        public Action<MeasureLineData, bool> OnView { get; set; }
        public Action<MeasureLineData> OnEdit { get; set; }
        public Action<MeasureLineData> OnDelete { get; set; }

        public MeasureItem(MeasureLineData data, VisualTreeAsset template)
        {
            m_Data = data;
            Id = data.Id;
            Root = template.Instantiate();

            m_Container = Root.Q("MeasureLineItem");
            var name = Root.Q<Text>("LineItemName");
            m_ViewButton = Root.Q<ActionButton>("LineItemView");
            var editButton = Root.Q<ActionButton>("LineItemEdit");
            var deleteButton = Root.Q<ActionButton>("LineItemDelete");

            m_Container.AddManipulator(new Pressable(OnClicked));

            name.text = data.Name;
            name.tooltip = data.Name;
            m_ViewButton.clicked += OnViewClicked;
            editButton.clicked += OnEditClicked;
            deleteButton.clicked += OnDeleteClicked;
        }

        public void Select(bool value)
        {
            if (value)
            {
                m_ViewButton.icon = k_ShownIcon;
                m_ViewButton.AddToClassList("is-disabled");
                m_ViewButton.pickingMode = PickingMode.Position;
                m_Container.AddToClassList("selected");
            }
            else
            {
                UpdateViewIcon();
                m_ViewButton.RemoveFromClassList("is-disabled");
                m_Container.RemoveFromClassList("selected");
            }
        }

        public void Show(bool value)
        {
            IsShown = value;
            UpdateViewIcon();

            OnView?.Invoke(m_Data, IsShown);
        }

        void UpdateViewIcon()
        {
            m_ViewButton.icon = IsShown ? k_ShownIcon : k_HiddenIcon;
        }

        void OnClicked()
        {
            OnClick?.Invoke(m_Data);
        }

        void OnViewClicked()
        {
            if(m_ViewButton.ClassListContains("is-disabled"))
                return;

            IsShown = !IsShown;
            Show(IsShown);
        }

        void OnEditClicked()
        {
            m_Data.IsEditable = true;
            OnEdit?.Invoke(m_Data);
        }

        void OnDeleteClicked()
        {
            OnDelete?.Invoke(m_Data);
        }
    }
}
