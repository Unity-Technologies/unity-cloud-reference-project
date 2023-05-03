using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Manipulator used to display a Tooltip UI component anchored to the currently hovered UI element.
    /// </summary>
    class TooltipManipulator : Manipulator
    {
        const int k_TooltipDelayMs = 750;

        VisualElement m_AnchorElement;

        IVisualElementScheduledItem m_ScheduledItem;

        Tooltip m_Tooltip;

        protected override void RegisterCallbacksOnTarget()
        {
            m_Tooltip = Tooltip.Build(target);
            m_ScheduledItem = target.schedule.Execute(StartFadeIn);
            m_ScheduledItem.Pause();
            target.RegisterCallback<PointerMoveEvent>(OnPointerMoved);
            target.RegisterCallback<PointerDownEvent>(OnClick, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMoved);
            target.UnregisterCallback<PointerDownEvent>(OnClick, TrickleDown.TrickleDown);
        }

        void OnClick(PointerDownEvent evt)
        {
            HideTooltip();
        }

        void OnPointerMoved(PointerMoveEvent evt)
        {
            // 1 - pick tooltip below cursor
            var pickedElement = target.panel?.Pick(evt.localPosition);
            if (pickedElement == null)
            {
                HideTooltip();
                return;
            }

            // 2 - If the picked tooltip is same as currently displayed, nothing to do
            var tooltipElement = GetTooltipElement(pickedElement);
            if (m_AnchorElement == tooltipElement)
                return;
            m_AnchorElement = tooltipElement;

            // 3 - New tooltip to display, hide the visual tooltip first
            HideTooltip();
            m_Tooltip.SetText(m_AnchorElement?.tooltip);

            // 4 - If the tne new tooltip is not null, start delay
            if (!string.IsNullOrEmpty(m_Tooltip.text))
                ShowTooltip();
        }

        void ShowTooltip()
        {
            m_ScheduledItem?.ExecuteLater(k_TooltipDelayMs);
        }

        void StartFadeIn()
        {
            m_Tooltip?.SetAnchor(m_AnchorElement);
            m_Tooltip?.SetPlacement(m_AnchorElement.GetPreferredTooltipPlacement());
            m_Tooltip?.Show();
        }

        void HideTooltip()
        {
            m_ScheduledItem?.Pause();
            m_Tooltip?.Dismiss();
        }

        static VisualElement GetTooltipElement(VisualElement element)
        {
            var ret = element;
            var parent = element.parent;

            while (string.IsNullOrEmpty(ret.tooltip) && parent != null)
            {
                ret = parent;
                parent = parent.parent;
            }

            return string.IsNullOrEmpty(ret.tooltip) ? null : ret;
        }
    }
}
