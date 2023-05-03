using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     Controls the visual aspects of a ray interactor and its interaction line based on what the interactor is hovering
    ///     and selecting.
    /// </summary>
    public class RayVisualController : MonoBehaviour
    {
        [SerializeField, Tooltip("The ray line visual.")]
        RayInteractionLine m_RayInteractionLine;

        [SerializeField, Tooltip("When hovering over UI, this specifies the visual style of the line.")]
        InteractionLineSettings m_LineSettingsUI;

        [SerializeField, Tooltip("When selecting UI, this specifies the visual style of the line.")]
        InteractionLineSettings m_LineSettingsUIClick;

        [SerializeField, Tooltip("When hovering over 3D interactables, this specifies the visual style of the line.")]
        InteractionLineSettings m_LineSettings3D;

        [SerializeField, Tooltip("When selecting 3D interactables, this specifies the visual style of the line.")]
        InteractionLineSettings m_LineSettings3DSelected;
        bool m_Hovering;

        bool m_Selected;

        /// <summary>
        ///     The ray line visual.
        /// </summary>
        public RayInteractionLine RayInteractionLine
        {
            get => m_RayInteractionLine;
            set => m_RayInteractionLine = value;
        }

        /// <summary>
        ///     When hovering over UI, this specifies the visual style of the line.
        /// </summary>
        public InteractionLineSettings LineSettingsUI
        {
            get => m_LineSettingsUI;
            set => m_LineSettingsUI = value;
        }

        /// <summary>
        ///     When selecting UI, this specifies the visual style of the line.
        /// </summary>
        public InteractionLineSettings LineSettingsUIClick
        {
            get => m_LineSettingsUIClick;
            set => m_LineSettingsUIClick = value;
        }

        /// <summary>
        ///     When hovering over 3D interactables, this specifies the visual style of the line.
        /// </summary>
        public InteractionLineSettings LineSettings3D
        {
            get => m_LineSettings3D;
            set => m_LineSettings3D = value;
        }

        /// <summary>
        ///     When selecting 3D interactables, this specifies the visual style of the line.
        /// </summary>
        public InteractionLineSettings LineSettings3DSelected
        {
            get => m_LineSettings3DSelected;
            set => m_LineSettings3DSelected = value;
        }

        void Start()
        {
            m_Selected = false;
            m_Hovering = false;
            var interactor = m_RayInteractionLine.RayInteractor;
            if (interactor != null)
            {
                interactor.hoverEntered.AddListener(HoverEnterVisuals);
                interactor.hoverExited.AddListener(HoverExitVisuals);
                interactor.selectEntered.AddListener(SelectEnteredVisuals);
                interactor.selectExited.AddListener(SelectExitedVisuals);
            }
        }

        void LateUpdate()
        {
            var interactor = m_RayInteractionLine.RayInteractor;
            if (interactor != null)
            {
                var isVisible = m_RayInteractionLine.RayInteractor.gameObject.activeSelf;
                m_RayInteractionLine.Hidden = !isVisible;
                if (!isVisible)
                    return;
            }

            var selecting = m_RayInteractionLine.Selected;
            if (selecting)
            {
                if (!m_Selected)
                {
                    m_RayInteractionLine.LineSettings = m_LineSettingsUIClick;
                }
            }
            else
            {
                if (!m_Selected && !m_Hovering)
                {
                    m_RayInteractionLine.LineSettings = m_LineSettingsUI;
                }
            }
        }

        void HoverEnterVisuals(HoverEnterEventArgs eventArgs)
        {
            m_Hovering = true;
            if (!m_Selected)
            {
                m_RayInteractionLine.LineSettings = m_LineSettings3D;
            }
        }

        void HoverExitVisuals(HoverExitEventArgs eventArgs)
        {
            m_Hovering = false;
            if (!m_Selected)
            {
                m_RayInteractionLine.LineSettings = m_LineSettingsUI;
            }
        }

        void SelectEnteredVisuals(SelectEnterEventArgs eventArgs)
        {
            m_Selected = true;
            m_RayInteractionLine.LineSettings = m_LineSettings3DSelected;
        }

        void SelectExitedVisuals(SelectExitEventArgs eventArgs)
        {
            m_Selected = false;
            if (m_Hovering)
            {
                m_RayInteractionLine.LineSettings = m_LineSettings3D;
            }
            else
            {
                m_RayInteractionLine.LineSettings = m_LineSettingsUI;
            }
        }
    }
}
