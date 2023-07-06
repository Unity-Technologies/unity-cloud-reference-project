using System;
using Unity.ReferenceProject.WorldSpaceUIDocumentExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VR.RigUI
{
    public class PanelController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        WorldSpaceUIDocumentSize m_WorldSpaceUIDocumentSize;

        [SerializeField]
        WorldSpaceUIDocumentCustomFunction m_WorldSpaceUIDocumentCustomFunction;

        [SerializeField]
        Collider m_Collider;

        public UIDocument UIDocument => m_UIDocument;

        public VisualTreeAsset VisualTreeAsset
        {
            get => m_VisualTreeAsset;
            set
            {
                m_VisualTreeAsset = value;
                m_VisualTreeAsset.CloneTree(m_UIDocument.rootVisualElement);
            }
        }

        public virtual Camera XRCamera
        {
            get => m_Camera;
            set
            {
                m_Camera = value;
            }
        }

        public virtual Vector2 PanelSize
        {
            get => m_WorldSpaceUIDocumentSize.Size;
            set => m_WorldSpaceUIDocumentSize.Size = value;
        }

        public VisualElement Root => m_UIDocument.rootVisualElement;

        void Awake()
        {
            if (m_Camera != null)
            {
                XRCamera = m_Camera;
            }

            if (m_VisualTreeAsset != null)
            {
                VisualTreeAsset = m_VisualTreeAsset;
            }

            m_WorldSpaceUIDocumentSize = m_UIDocument.gameObject.GetComponent<WorldSpaceUIDocumentSize>();
        }

        public virtual void SetVisible(bool isVisible)
        {
            Root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            m_Collider.enabled = isVisible;
            m_WorldSpaceUIDocumentSize.enabled = isVisible;
        }
    }
}
