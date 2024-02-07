using Unity.ReferenceProject.Tools;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetListToolUIController : ToolUIController
    {
        [SerializeField]
        protected AssetListUIController m_AssetListUIController;

        protected override void Awake()
        {
            base.Awake();

            if (m_AssetListUIController != null && m_AssetListUIController.RootVisualElement != null)
            {
                SetRootVisualElement(m_AssetListUIController.RootVisualElement);
            }
        }

        public override void OnToolOpened()
        {
            if (!m_AssetListUIController.gameObject.activeSelf)
            {
                m_AssetListUIController.gameObject.SetActive(true);
            }

            m_AssetListUIController.SetVisibility(true);
            m_AssetListUIController.BringToFront();
        }

        public override void OnToolClosed()
        {
            m_AssetListUIController.SetVisibility(false);
        }
    }
}
