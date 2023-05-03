using System;
using Unity.ReferenceProject.Instructions;
using Unity.ReferenceProject.VRManager;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    [RequireComponent(typeof(InstructionsUIController))]
    public class InstructionUIControllerVR : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_InstructionPanelTemplate;

        [SerializeField]
        VisualTreeAsset m_InstructionsTemplate;

        [SerializeField]
        string UIContainerName;

        [SerializeField]
        StyleSheet m_StyleSheet;

        [SerializeField]
        Vector2 m_PanelSize;

        [SerializeField]
        Vector3 m_PositionOffset;

        InstructionsUIController m_InstructionsUIController;
        IRigUIController m_RigUIController;
        IPanelManager m_PanelManager;
        PanelController m_Panel;
        bool m_IsShow;

        [Inject]
        void Setup(IRigUIController rigUIController, IPanelManager panelManager)
        {
            m_RigUIController = rigUIController;
            m_PanelManager = panelManager;
        }

        void Awake()
        {
            m_InstructionsUIController = GetComponent<InstructionsUIController>();
            m_InstructionsUIController.OnInstructionPanelEnabled += OnInstructionPanelEnabled;
        }

        void OnEnable()
        {
            m_InstructionsUIController.SetVisiblePanel(m_InstructionsUIController.GetCheckboxValue() == CheckboxState.Unchecked);
        }

        void OnDestroy()
        {
            DestroyPanel();
        }

        void DestroyPanel()
        {
            if (m_Panel != null)
            {
                m_PanelManager.DestroyPanel(m_Panel);
            }
        }

        void OnInstructionPanelEnabled(bool show)
        {
            if (show)
            {
                if (m_Panel == null)
                {
                    var dockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(m_PanelSize);
                    dockedPanel.name = "InstructionPanel";
                    dockedPanel.WorldSpaceUIDocument.OnPanelBuilt += OnPanelBuilt;
                    dockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
                    dockedPanel.transform.localPosition = m_PositionOffset;
                    m_Panel = dockedPanel;
                }
            }
            else
            {
                DestroyPanel();
            }
        }

        void OnPanelBuilt(UIDocument document)
        {
            var root = document.rootVisualElement;
            var panel = root.Q<Panel>();
            panel.Add(m_InstructionPanelTemplate.CloneTree());
            m_InstructionsUIController.InitializeUI(document);

            var uiContainer = root.Q<VisualElement>(UIContainerName);
            uiContainer.Add(m_InstructionsTemplate.CloneTree());

            if (m_StyleSheet != null)
            {
                root.styleSheets.Add(m_StyleSheet);
            }

            var dockButton = root.Q<ActionButton>("Button-dock");
            dockButton.clickable.clicked += OnDockButtonClicked;
            dockButton.selected = m_Panel is FloatingPanelController;
        }

        void OnDockButtonClicked()
        {
            m_Panel = m_RigUIController.DockButtonClicked(m_Panel, m_RigUIController.DockPoint, m_PositionOffset, OnPanelBuilt);
        }
    }
}
