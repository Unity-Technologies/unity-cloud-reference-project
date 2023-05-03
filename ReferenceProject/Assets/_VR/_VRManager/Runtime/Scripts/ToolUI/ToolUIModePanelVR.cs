using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Panel = UnityEngine.Dt.App.UI.Panel;

namespace Unity.ReferenceProject.VRManager
{
    [CreateAssetMenu(menuName = "ReferenceProject/ToolUIMode/ToolUIModePanelVR")]
    public class ToolUIModePanelVR : ToolUIMode
    {
        public List<StyleSheet> AdditionalStyles;

        public string PanelStyleClass;
        public Vector2 PanelSize;
        public Vector3 PanelPosition;
        public bool DisplayTitle;
        public bool DisplayIcon;
        public bool DisplayCloseButton;
        public bool DisplayDockButton;

        public override IToolUIModeHandler CreateHandler()
        {
            return new ToolUIModePanelVRHandler(this);
        }
    }

    class ToolUIModePanelVRHandler : ToolUIModeHandler
    {
        readonly List<StyleSheet> m_AdditionalStyles;
        readonly bool m_DisplayCloseButton;
        readonly bool m_DisplayDockButton;
        readonly bool m_DisplayIcon;
        readonly bool m_DisplayTitle;
        readonly Vector3 m_PanelPosition;
        readonly Vector2 m_PanelSize;

        readonly string m_StyleClass;

        PanelController m_PanelController;
        UIDocument m_UIDocument;

        public ToolUIModePanelVRHandler(ToolUIModePanelVR toolUIModePanelVR)
        {
            m_AdditionalStyles = toolUIModePanelVR.AdditionalStyles;
            m_StyleClass = toolUIModePanelVR.PanelStyleClass;
            m_PanelSize = toolUIModePanelVR.PanelSize;
            m_PanelPosition = toolUIModePanelVR.PanelPosition;
            m_DisplayTitle = toolUIModePanelVR.DisplayTitle;
            m_DisplayIcon = toolUIModePanelVR.DisplayIcon;
            m_DisplayCloseButton = toolUIModePanelVR.DisplayCloseButton;
            m_DisplayDockButton = toolUIModePanelVR.DisplayDockButton;
        }

        protected ToolPanelVR Panel { private set; get; }
        public IPanelManager PanelManager { get; set; }
        public IRigUIController RigUIController { get; set; }

        protected override VisualElement CreateVisualTreeInternal()
        {
            Panel = ToolPanelVR.Build(ToolUIController.RootVisualElement)
                .SetTitle(m_DisplayTitle ? ToolUIController.DisplayName : null)
                .SetIcon(m_DisplayIcon ? ToolUIController.Icon : null)
                .SetDismissable(m_DisplayCloseButton)
                .SetDockable(m_DisplayDockButton);

            foreach (var style in m_AdditionalStyles)
            {
                Panel.styleSheets.Add(style);
            }

            if (!string.IsNullOrWhiteSpace(m_StyleClass))
            {
                Panel.AddToClassList(m_StyleClass);
            }

            Panel.DismissRequested += CloseTool;
            Panel.DockRequested += OnDockButtonClicked;

            return Panel;
        }

        protected override void OnToolOpenedInternal()
        {
            if (m_PanelController != null)
            {
                m_PanelController.SetVisible(true);
            }
            else
            {
                var dockedPanel = PanelManager.CreatePanel<DockedPanelController>(m_PanelSize);
                dockedPanel.WorldSpaceUIDocument.OnPanelBuilt += OnPanelBuilt;

                var barTransform = RigUIController.DockPoint;
                dockedPanel.DockPoint = barTransform;
                dockedPanel.transform.localPosition = m_PanelPosition;

                m_PanelController = dockedPanel;
            }
        }

        protected override void OnToolClosedInternal()
        {
            if (m_PanelController != null)
            {
                m_PanelController.SetVisible(false);
            }
        }

        void OnPanelBuilt(UIDocument uiDocument)
        {
            m_UIDocument = uiDocument;
            UpdatePanel();
        }

        void UpdatePanel()
        {
            var root = m_UIDocument.rootVisualElement.Q<Panel>("appui-panel");
            var panel = CreateVisualTreeInternal();
            root.Add(panel);
            Panel.SetDocked(m_PanelController is DockedPanelController);
        }

        void OnDockButtonClicked()
        {
            m_PanelController = RigUIController.DockButtonClicked(m_PanelController, RigUIController.DockPoint, m_PanelPosition, OnPanelBuilt);
        }
    }
}
