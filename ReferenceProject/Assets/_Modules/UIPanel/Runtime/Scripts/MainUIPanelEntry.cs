using System;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.UIPanel
{
    [RequireComponent(typeof(UIDocument))]
    public class MainUIPanelEntry : MonoBehaviour
    {
        IMainUIPanel m_MainUIPanel;

        [Inject]
        void Setup(IMainUIPanel mainUIPanel)
        {
            m_MainUIPanel = mainUIPanel;
        }

        void Start()
        {
            var uiDocument = gameObject.GetComponent<UIDocument>();
            m_MainUIPanel.Add(uiDocument);
        }
    }
}
