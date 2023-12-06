using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.Tools
{
    public interface IToolUIManager
    {
        void RegisterHandler(IToolUIModeHandler toolHandler);
        void UnregisterHandler(IToolUIModeHandler toolHandler);
        void OpenTool(IToolUIModeHandler toolHandler, bool closeOtherTools);
        void CloseAllTools();
    }

    public class ToolUIManager : MonoBehaviour, IToolUIManager
    {
        readonly List<IToolUIModeHandler> m_ToolHandlers = new();

        public void RegisterHandler(IToolUIModeHandler toolHandler)
        {
            m_ToolHandlers.Add(toolHandler);
        }

        public void UnregisterHandler(IToolUIModeHandler toolHandler)
        {
            m_ToolHandlers.Remove(toolHandler);
        }

        public void OpenTool(IToolUIModeHandler toolHandler, bool closeOtherTools)
        {
            if (toolHandler.IsOpened)
            {
                toolHandler.CloseTool();
            }
            else
            {
                if (closeOtherTools)
                {
                    CloseAllTools();
                }

                toolHandler.OpenTool();
            }
        }

        public void CloseAllTools()
        {
            foreach (var toolHandler in m_ToolHandlers)
            {
                if (!toolHandler.KeepOpened)
                {
                    toolHandler.CloseTool();
                }
            }
        }
    }
}
