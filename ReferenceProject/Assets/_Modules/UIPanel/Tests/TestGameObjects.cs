using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.ReferenceProject.UIPanel.Tests
{
    class TestGameObjects
    {
        GameObject m_Root;

        public void Cleanup()
        {
            if (m_Root != null)
            {
                Object.DestroyImmediate(m_Root);
            }
        }

        public GameObject NewGameObject(string name = null)
        {
            if (m_Root == null)
            {
                m_Root = new GameObject("Test Root");
            }

            var go = new GameObject(name)
            {
                transform = { parent = m_Root.transform }
            };
            return go;
        }

        public MainUIPanel NewMainUIPanel(PanelSettings panelSettings)
        {
            if (m_Root == null)
            {
                m_Root = new GameObject("Test Root");
            }

            var mainUIPanel = new MainUIPanel(panelSettings);
            var uiDocument = mainUIPanel.UIDocument;
            uiDocument.transform.parent = m_Root.transform;
            return mainUIPanel;
        }
    }
}
