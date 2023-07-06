using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Tools
{
    [Serializable]
    public struct ToolData
    {
        [SerializeField]
        ToolUIMode m_ToolUIMode;
        
        [SerializeField]
        string m_ToolbarElementName;
        
        [SerializeField]
        string m_ButtonStyleClass;
        
        [SerializeField]
        bool m_CloseOtherTools;
        
        [SerializeField]
        ToolUIController m_ToolUIController;
        
        public ToolUIMode ToolUIMode => m_ToolUIMode;
        public string ToolbarElementName => m_ToolbarElementName;
        public string ButtonStyleClass => m_ButtonStyleClass;
        public bool CloseOtherTools => m_CloseOtherTools;
        public ToolUIController ToolUIController => m_ToolUIController;
    }

    [RequireComponent(typeof(UIDocument))]
    public class ToolUIMenu : MonoBehaviour
    {
        [SerializeField]
        List<StyleSheet> m_AdditionalStyles;

        [SerializeField]
        List<ToolData> m_ToolData;

        IToolUIManager m_ToolUIManager;
        readonly List<IToolUIModeHandler> m_Handlers = new();

        [Inject]
        void Setup(IToolUIManager toolUIManager)
        {
            m_ToolUIManager = toolUIManager;
        }

        void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            foreach (var style in m_AdditionalStyles)
            {
                root.styleSheets.Add(style);
            }

            var panelContainer = new VisualElement
            {
                name = "panel-container",
                style =
                {
                    flexGrow = 1 // Fix: make children panels be able to fit full screen height by auto height
                },
                pickingMode = PickingMode.Ignore // Ignore because now it is fullscreen
            };

            root.Add(panelContainer);

            foreach (var toolData in m_ToolData)
            {
                var buttonContainer = string.IsNullOrEmpty(toolData.ToolbarElementName) ? null : root.Q<VisualElement>(toolData.ToolbarElementName);
                var handler = AddTool(panelContainer, buttonContainer, toolData);
                m_Handlers.Add(handler);
                m_ToolUIManager.RegisterHandler(handler);
            }
        }

        void OnDestroy()
        {
            foreach (var handler in m_Handlers)
            {
                m_ToolUIManager.UnregisterHandler(handler);
            }
        }

        IToolUIModeHandler AddTool(VisualElement panelContainer, VisualElement buttonContainer, ToolData toolData)
        {
            var toolUIController = toolData.ToolUIController;

            var button = CreateActionButton(toolUIController, toolData.ButtonStyleClass);
            buttonContainer.Add(button);

            var handler = toolData.ToolUIMode.CreateHandler();

            var panel = handler.CreateVisualTree(button, toolUIController);

            panelContainer.Add(panel);

            button.clickable.clicked += () => m_ToolUIManager.OpenTool(handler, toolData.CloseOtherTools);

            return handler;
        }

        public static ActionButton CreateActionButton(ToolUIController toolUIController, string buttonStyleClass)
        {
            var iconElement = toolUIController.GetButtonContent();
            var button = new ActionButton();
            button.accent = true;
            button.focusable = false;
            button.tooltip = toolUIController.DisplayName;
            button.hierarchy.Add(iconElement);
            
            if (!string.IsNullOrWhiteSpace(buttonStyleClass))
            {
                button.AddToClassList(buttonStyleClass);
            }

            return button;
        }
    }
}
