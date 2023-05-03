using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Tools
{
    [Serializable]
    public struct ToolData
    {
        public ToolUIMode ToolUIMode;
        public string ToolbarElementName;
        public string ButtonStyleClass;
        public bool CloseOtherTools;
        public ToolUIController ToolUIController;
    }

    [RequireComponent(typeof(UIDocument))]
    public class ToolUIMenu : MonoBehaviour
    {
        [SerializeField]
        List<StyleSheet> m_AdditionalStyles;

        [SerializeField]
        List<ToolData> m_ToolData;

        VisualElement m_Root;

        IToolUIManager m_ToolUIManager;

        [Inject]
        void Setup(IToolUIManager toolUIManager)
        {
            m_ToolUIManager = toolUIManager;
        }

        void Awake()
        {
            m_Root = GetComponent<UIDocument>().rootVisualElement;

            foreach (var style in m_AdditionalStyles)
            {
                m_Root.styleSheets.Add(style);
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

            m_Root.Add(panelContainer);

            foreach (var toolData in m_ToolData)
            {
                var buttonContainer = string.IsNullOrEmpty(toolData.ToolbarElementName) ? null : m_Root.Q<VisualElement>(toolData.ToolbarElementName);
                var handler = AddTool(panelContainer, buttonContainer, toolData);
                m_ToolUIManager.RegisterHandler(handler);
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
            var button = new ActionButton();
            button.accent = true;
            var icon = (Icon)button.hierarchy.ElementAt(0);
            icon.size = IconSize.L;

            Action<Sprite> onIconChange = sprite =>
            {
                if (sprite != null)
                {
                    button.icon = "notnull";
                    icon.sprite = sprite;
                }
                else
                {
                    button.icon = "warning";
                }
            };
            toolUIController.IconChanged += onIconChange;
            onIconChange.Invoke(toolUIController.Icon); // Set current icon

            button.focusable = false;
            button.tooltip = toolUIController.DisplayName;

            if (!string.IsNullOrWhiteSpace(buttonStyleClass))
            {
                button.AddToClassList(buttonStyleClass);
            }

            return button;
        }
    }
}
