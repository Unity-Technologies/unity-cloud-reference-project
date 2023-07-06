using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    [RequireComponent(typeof(StatsDisplay))]
    public class StatDisplayVR : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [SerializeField]
        Vector2 m_Size;

        [SerializeField]
        Vector3 m_Position;

        StatsDisplay m_StatsDisplay;
        PanelController m_Panel;

        IRigUIController m_RigUIController;
        IPanelManager m_PanelManager;

        [Inject]
        void Setup(IRigUIController rigUIController, IPanelManager panelManager)
        {
            m_RigUIController = rigUIController;
            m_PanelManager = panelManager;
        }

        void Awake()
        {
            m_StatsDisplay = GetComponent<StatsDisplay>();
            m_StatsDisplay.OnShowPanel += OnShowPanel;
            OnShowPanel(m_StatsDisplay.IsEnabled);
        }

        void OnDestroy()
        {
            m_StatsDisplay.OnShowPanel -= OnShowPanel;
        }

        void OnShowPanel(bool isVisible)
        {
            if (isVisible)
            {
                if (m_Panel == null)
                {
                    var dockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(m_Size);
                    dockedPanel.name = "StatDisplayPanel";
                    dockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
                    dockedPanel.transform.localPosition = m_Position;
                    m_Panel = dockedPanel;
                    OnPanelBuilt(dockedPanel.UIDocument);
                }
            }
            else if (m_Panel != null)
            {
                m_PanelManager.DestroyPanel(m_Panel);
            }
        }

        void OnPanelBuilt(UIDocument document)
        {
            var root = document.rootVisualElement;
            var appUIPanel = root.Q<Panel>();

            var panel = ToolPanelVR.Build(m_VisualTreeAsset.Instantiate())
                .SetDismissable(true)
                .SetDockable(true)
                .SetVisible(true)
                .SetDocked(m_Panel is DockedPanelController);
            panel.DismissRequested += OnCloseButtonClicked;
            panel.DockRequested += OnDockButtonClicked;
            appUIPanel.Add(panel);

            var fpsEntries = root.Q("fps-entries");
            fpsEntries.style.bottom = fpsEntries.style.right = new StyleLength(0f);
            fpsEntries.style.position = Position.Relative;
            fpsEntries.style.marginLeft = fpsEntries.style.marginRight = new StyleLength(0f);
            m_StatsDisplay.InitUIToolkit(document);
        }

        void OnCloseButtonClicked()
        {
           m_StatsDisplay.ClosePanel();
        }

        void OnDockButtonClicked()
        {
            m_Panel = m_RigUIController.DockButtonClicked(m_Panel, m_RigUIController.PermanentDockPoint, m_Position);
            OnPanelBuilt(m_Panel.UIDocument);
        }
    }
}
