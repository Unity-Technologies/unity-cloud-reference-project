using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Instructions
{
    public abstract class InstructionUIEntry : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        VisualTreeAsset m_TemplateUI;

        [SerializeField]
        string m_UIContainerName;
        VisualElement m_Container;

        VisualElement m_Instance;

        protected abstract bool IsSupportPlatform { get; }

        void OnEnable()
        {
            if (IsSupportPlatform)
            {
                m_Instance ??= m_TemplateUI.Instantiate();
                m_Container = m_UIDocument.rootVisualElement.Q<VisualElement>(m_UIContainerName) ?? m_UIDocument.rootVisualElement;
                m_Container.Add(m_Instance);
            }
        }

        void OnDisable()
        {
            if (m_Container != null && m_Instance != null)
            {
                m_Container.Remove(m_Instance);
                m_Container = null;
            }
        }
    }
}
