using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Tools;
using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class ToolUIMenuVR : MonoBehaviour
    {
        [SerializeField]
        protected List<ToolData> m_ToolData;
        readonly List<IToolUIModeHandler> m_Handlers = new();

        protected List<ActionButton> m_Buttons = new();
        IPanelManager m_PanelManager;
        protected IRigUIController m_RigUIController;

        protected IToolUIManager m_ToolUIManager;

        [Inject]
        void Setup(IToolUIManager toolUIManager, IRigUIController rigUIController, IPanelManager panelManager)
        {
            m_ToolUIManager = toolUIManager;
            m_RigUIController = rigUIController;
            m_PanelManager = panelManager;
        }

        void Start()
        {
            foreach (var toolData in m_ToolData)
            {
                if (toolData.ToolUIController.gameObject.activeSelf)
                {
                    var handler = AddTool(toolData);
                    m_Handlers.Add(handler);
                }
            }
        }

        public virtual void Activate(bool activate, bool clearPanel = true)
        {
            if (activate)
            {
                // Only one ToolUIMenuVR at a time
                m_RigUIController.ClearMainBar(clearPanel);
                m_RigUIController.InitMainBar(m_Buttons);

                foreach (var handler in m_Handlers)
                {
                    m_ToolUIManager.RegisterHandler(handler);
                }
            }
            else
            {
                m_RigUIController.ClearMainBar();
                foreach (var handler in m_Handlers)
                {
                    m_ToolUIManager.UnregisterHandler(handler);
                }
            }
        }

        protected virtual void OnButtonClicked(ToolData toolData, IToolUIModeHandler handler)
        {
            m_ToolUIManager.OpenTool(handler, toolData.CloseOtherTools);
        }

        IToolUIModeHandler AddTool(ToolData toolData)
        {
            var toolUIController = toolData.ToolUIController;
            var button = ToolUIMenu.CreateActionButton(toolUIController, toolData.ButtonStyleClass);
            button.quiet = true;
            m_Buttons.Add(button);
            var handler = toolData.ToolUIMode.CreateHandler();
            handler.CreateVisualTree(button, toolUIController);

            // Need to inject the handler indirectly
            if (handler is ToolUIModePanelVRHandler toolUIModePanelVRHandler)
            {
                toolUIModePanelVRHandler.PanelManager = m_PanelManager;
                toolUIModePanelVRHandler.RigUIController = m_RigUIController;
            }

            button.clickable.clicked += () => OnButtonClicked(toolData, handler);

            return handler;
        }
    }
}
