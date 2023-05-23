using System;
using Unity.ReferenceProject.Tools;
using UnityEngine;

namespace Unity.ReferenceProject.ScenesList
{
    public class SceneListToolUIController : ToolUIController
    {
        [SerializeField]
        SceneListUIController m_SceneListUIController;

        public SceneListUIController SceneListUIController => m_SceneListUIController;

        protected override void Awake()
        {
            base.Awake();

            if (m_SceneListUIController.UIDocument != null)
            {
                SetRootVisualElement(m_SceneListUIController.UIDocument.rootVisualElement);
            }
        }

        public override void OnToolOpened()
        {
            if (!m_SceneListUIController.gameObject.activeSelf)
            {
                m_SceneListUIController.gameObject.SetActive(true);
            }

            m_SceneListUIController.SetVisibility(true);
            m_SceneListUIController.Refresh();

            m_SceneListUIController.ProjectSelected += _ => CloseSelf();
        }

        public override void OnToolClosed()
        {
            m_SceneListUIController.SetVisibility(false);
        }
    }
}
