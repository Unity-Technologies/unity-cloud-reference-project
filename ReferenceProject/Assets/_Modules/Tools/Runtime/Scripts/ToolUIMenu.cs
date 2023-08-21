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

    public class ToolUIMenu : MonoBehaviour
    {
        [SerializeField]
        List<StyleSheet> m_AdditionalStyles;

        [SerializeField]
        List<ToolData> m_ToolData;
        
        VisualElement m_Root;
        List<ActionButton> m_ButtonsToolbar;
        VisualElement m_PanelContainer;
        
        const string k_PanelContainer = "panel-container";
        public VisualElement Root => m_Root;
        public List<ActionButton> ButtonsToolbar => m_ButtonsToolbar;
        public VisualElement PanelContainer => m_PanelContainer;
        public event Action UICreated;

        IToolUIManager m_ToolUIManager;
        readonly List<IToolUIModeHandler> m_Handlers = new();
        
        [Inject]
        void Setup(IToolUIManager toolUIManager)
        {
            m_ToolUIManager = toolUIManager;
        }

        void Awake()
        {
            m_Root = GetComponentInParent<UIDocument>().rootVisualElement;
            m_ButtonsToolbar = new List<ActionButton>();
            
            foreach (var style in m_AdditionalStyles)
            {
                m_Root.styleSheets.Add(style);
            }

            m_PanelContainer = m_Root.Q(k_PanelContainer) ?? new VisualElement
            {
                name = k_PanelContainer,
                style =
                {
                    flexGrow = 1 // Fix: make children panels be able to fit full screen height by auto height
                },
                pickingMode = PickingMode.Ignore // Ignore because now it is fullscreen
            };

            m_Root.Add(m_PanelContainer);

            foreach (var toolData in m_ToolData)
            {
                var buttonContainer = string.IsNullOrEmpty(toolData.ToolbarElementName) ? null : m_Root.Q<VisualElement>(toolData.ToolbarElementName);
                var handler = AddTool(m_PanelContainer, buttonContainer, toolData);
                m_Handlers.Add(handler);
                m_ToolUIManager.RegisterHandler(handler);
            }
            
            UICreated?.Invoke();
        }
        
        void OnDestroy()
        {
            foreach (var handler in m_Handlers)
            {
                m_ToolUIManager.UnregisterHandler(handler);
            }
        }

        public IToolUIModeHandler AddTool(VisualElement panelContainer, VisualElement buttonContainer, ToolData toolData)
        {
            var toolUIController = toolData.ToolUIController;

            var button = CreateActionButton(toolUIController, toolData.ButtonStyleClass);
            button.name = toolUIController.DisplayName;
            buttonContainer.Add(button);

            var handler = toolData.ToolUIMode.CreateHandler();

            var panel = handler.CreateVisualTree(button, toolUIController);

            panelContainer.Add(panel);

            button.clickable.clicked += () => m_ToolUIManager.OpenTool(handler, toolData.CloseOtherTools);
            
            m_ButtonsToolbar.Add(button);

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
        
        public void AddHandler(IToolUIModeHandler handler)
        {
            m_Handlers.Add(handler);
            m_ToolUIManager.RegisterHandler(handler);
        }
    }
}
