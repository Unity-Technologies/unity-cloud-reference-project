using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.ScenesList
{
    public class SceneListToolUIController : ToolUIController
    {
        [SerializeField]
        SceneListUIController m_SceneListUIController;

        public SceneListUIController SceneListUIController => m_SceneListUIController;
        
        Action<IScene> CloseAction;

        protected override void Awake()
        {
            base.Awake();
            CloseAction = _ => CloseSelf();
            
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

            m_SceneListUIController.ProjectSelected += CloseAction;
        }

        public override void OnToolClosed()
        {
            m_SceneListUIController.SetVisibility(false);
            
            m_SceneListUIController.ProjectSelected -= CloseAction;
        }
    }
}
