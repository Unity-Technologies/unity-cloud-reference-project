using System;
using Unity.ReferenceProject.Tools;

namespace Unity.ReferenceProject.VR
{
    public class ToolUISubMenuVR : ToolUIMenuVR
    {
        IToolUIModeHandler m_OpenedTool;

        public override void Activate(bool activate)
        {
            if (activate)
            {
                m_RigUIController.AddSecondaryBar(m_Buttons);
            }
            else
            {
                m_RigUIController.ClearSecondaryBar();
                if (m_OpenedTool != null)
                {
                    m_OpenedTool.CloseTool();
                    m_OpenedTool = null;
                }
            }
        }

        protected override void OnButtonClicked(ToolData toolData, IToolUIModeHandler handler)
        {
            // Avoid closing main bar button when selecting secondary bar tool
            if (toolData.CloseOtherTools)
            {
                if (m_OpenedTool != null)
                {
                    if (handler != m_OpenedTool)
                    {
                        m_OpenedTool.CloseTool();
                        m_OpenedTool = handler;
                    }
                    else
                    {
                        m_OpenedTool = null;
                    }
                }
                else
                {
                    m_OpenedTool = handler;
                }
            }

            m_ToolUIManager.OpenTool(handler, false);
        }
    }
}
