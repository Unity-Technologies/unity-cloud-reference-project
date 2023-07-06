using System;
using UnityEngine;
using Unity.AppUI.UI;

namespace Unity.ReferenceProject.Tools
{
    [CreateAssetMenu(menuName = "ReferenceProject/ToolUIMode/ToolUIModePopover")]
    public class ToolUIModePopover : ToolUIModePanel
    {
        [SerializeField]
        PopoverPlacement m_PopoverPlacement;

        public PopoverPlacement PopoverPlacement => m_PopoverPlacement;

        public override IToolUIModeHandler CreateHandler()
        {
            return new ToolUIModePopoverHandler(this);
        }
    }

    class ToolUIModePopoverHandler : ToolUIModePanelHandler
    {
        readonly PopoverPlacement m_PopoverPlacement;
        Popover m_Popover;

        public ToolUIModePopoverHandler(ToolUIModePopover toolUIModePopover)
            : base(toolUIModePopover)
        {
            m_PopoverPlacement = toolUIModePopover.PopoverPlacement;
        }

        protected override void OnToolOpenedInternal()
        {
            m_Popover = Popover.Build(Button, Panel)
                .SetPlacement(m_PopoverPlacement);

            m_Popover.dismissed += (_, _) =>
            {
                m_Popover = null;
                CloseTool();
            };

            Panel.SetVisible(true);
            m_Popover.Show();
        }

        protected override void OnToolClosedInternal()
        {
            m_Popover?.Dismiss();
            m_Popover = null;
        }
    }
}
