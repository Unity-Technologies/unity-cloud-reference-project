using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.VR.RigUI
{
    public class PanelController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        TrackedDevicePhysicsRaycaster m_DeviceRaycaster;

        [SerializeField]
        WorldSpaceUIToolkit.WorldSpaceUIToolkit m_WorldSpaceUIToolkit;

        [SerializeField]
        Collider m_Collider;

        public VisualTreeAsset VisualTreeAsset
        {
            get => m_VisualTreeAsset;
            set
            {
                m_VisualTreeAsset = value;
                m_WorldSpaceUIToolkit.VisualTreeAsset = m_VisualTreeAsset;
            }
        }

        public virtual Camera XRCamera
        {
            get => m_Camera;
            set
            {
                m_Camera = value;
                m_DeviceRaycaster.SetEventCamera(m_Camera);
            }
        }

        public virtual Vector2 PanelSize
        {
            get => m_WorldSpaceUIToolkit.PanelSize;
            set => m_WorldSpaceUIToolkit.PanelSize = value;
        }

        public VisualElement Root { get; private set; }

        public WorldSpaceUIToolkit.WorldSpaceUIToolkit WorldSpaceUIToolkit => m_WorldSpaceUIToolkit;

        void Awake()
        {
            if (m_Camera != null)
            {
                XRCamera = m_Camera;
            }

            if (m_VisualTreeAsset != null)
            {
                m_WorldSpaceUIToolkit.VisualTreeAsset = m_VisualTreeAsset;
            }

            m_WorldSpaceUIToolkit.OnPanelBuilt += OnPanelBuilt;
        }

        public virtual void SetVisible(bool isVisible)
        {
            Root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            m_Collider.enabled = isVisible;
        }

        void OnPanelBuilt(UIDocument document)
        {
            Root = document.rootVisualElement;
        }
    }
}
