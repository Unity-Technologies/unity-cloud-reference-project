using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public abstract class AppStateWorldSpacePanel : AppStateListener
    {
        IRigUIController m_RigUIController;
        IPanelManager m_PanelManager;

        PanelController m_Panel;

        protected abstract string PanelName { get; }

        protected abstract Vector2 PanelSize { get; }

        [Inject]
        void Setup(IRigUIController rigUIController, IPanelManager panelManager)
        {
            m_RigUIController = rigUIController;
            m_PanelManager = panelManager;
        }

        protected override void StateEntered()
        {
            var dockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(PanelSize);
            dockedPanel.name = PanelName;
            dockedPanel.DockPoint = m_RigUIController.DockPoint;
            dockedPanel.transform.localPosition += (PanelSize.y / 2f) / 1000f * Vector3.up + 0.01f * Vector3.forward;
            OnPanelBuilt(dockedPanel.UIDocument);
            m_Panel = dockedPanel;
        }

        protected override void StateExited()
        {
            if (m_Panel != null)
            {
                m_PanelManager.DestroyPanel(m_Panel);
            }
        }

        protected abstract void OnPanelBuilt(UIDocument document);
    }
}
