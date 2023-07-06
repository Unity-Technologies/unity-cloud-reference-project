using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     A base class for controlling visuals for an XR ray interactor.
    /// </summary>
    public abstract class RayInteractionRenderer : MonoBehaviour
    {
        [SerializeField, Tooltip("The Ray Interactor to render")]
        protected XRRayInteractor m_RayInteractor;

        [SerializeField]
        protected ActionBasedController m_XRController;

        [SerializeField, Tooltip("Whether visuals should be hidden while the target interactor is not hovering over anything.")]
        bool m_HideIfNotHovering;

        [SerializeField, Tooltip("The default length of the ray interaction line.")]
        protected float m_DefaultLineLength = 5f;

        [SerializeField, Tooltip("If enabled, the ray interaction endpoint will attempt to snap to Ray Interaction Snapping Controller components on the collider is hit.")]
        bool m_SnapEndpointIfAvailable;

        [SerializeField, Tooltip("Whether visuals should be hidden while the target interactor is snapped to an interactable.")]
        bool m_HideIfSnapping;

        [SerializeField]
        LayerMask m_LayerMask;

        /// <summary>
        ///     An event that is fired when the renderer changes visibility. The boolean value is true when shown, and false when
        ///     hidden.
        /// </summary>
        public event Action<bool> Showed;

        Vector3 m_CurrentHitNormal;
        Vector3 m_CurrentHitOrSelectPoint;
        float m_CurrentRayLength;
        bool m_Hovering;
        Vector3[] m_LinePoints = new Vector3[2];
        Vector3 m_ObjectLocalSelectPoint;
        Transform m_SelectedObjectTransform;
        bool m_Snapping;
        Collider m_PreviousCollider;
        RayInteractionSnappingController m_InteractionSnappingController;

        /// <summary>
        ///     The ray interactor that is used when updating visuals.
        /// </summary>
        public XRRayInteractor RayInteractor
        {
            get => m_RayInteractor;
            set
            {
                if (value != m_RayInteractor)
                {
                    if (m_RayInteractor != null)
                        UnbindToInteractor(m_RayInteractor);

                    m_RayInteractor = value;
                    BindToInteractor(m_RayInteractor);
                }
            }
        }

        /// <summary>
        ///     Length (in meters) of the current target ray interactor. This is equivalent to the hit distance of the interactor's
        ///     current raycast result if there was a game object hit, or the interactor's maximum raycast distance if there
        ///     was not a game object hit.
        /// </summary>
        public float CurrentRayLength
        {
            get => m_CurrentRayLength;
            private set => m_CurrentRayLength = Mathf.Max(0f, value);
        }

        /// <summary>
        ///     If the interactor is currently selecting, then this is the point on the selected object where the ray end point was
        ///     when the selection started. Otherwise this is the current end point of the ray.
        /// </summary>
        public Vector3 CurrentHitOrSelectPoint => m_CurrentHitOrSelectPoint;

        /// <summary>
        ///     The normal of the surface at the point where the ray is currently hitting.
        /// </summary>
        public Vector3 CurrentHitNormal => m_CurrentHitNormal;

        /// <summary>
        ///     Whether visuals should be hidden while the target interactor is not hovering over anything.
        /// </summary>
        public bool HideIfNotHovering
        {
            get => m_HideIfNotHovering;
            set
            {
                m_HideIfNotHovering = value;
                if (!m_HideIfNotHovering)
                    Show(true);
                else if (!m_Hovering)
                    Show(false);
            }
        }

        /// <summary>
        ///     Reference to the current selected object transform
        /// </summary>
        public Transform SelectedObjectTransform => m_SelectedObjectTransform;

        /// <summary>
        ///     Sets an override ray length of the ray renderer that can be less than the actual current length
        /// </summary>
        public float? OverrideRayLength { get; set; }

        public bool Selected { get; set; }

        protected virtual void LateUpdate()
        {
            // Update visuals in LateUpdate so they are not a frame behind.
            if (RayInteractor != null)
            {
                ((ILineRenderable)RayInteractor).GetLinePoints(ref m_LinePoints, out _);
                var rayOrigin = RayInteractor.attachTransform;
                var startPoint = rayOrigin.position;

                if (m_SelectedObjectTransform != null)
                {
                    m_CurrentHitOrSelectPoint = m_SelectedObjectTransform.TransformPoint(m_ObjectLocalSelectPoint);
                    CurrentRayLength = Vector3.Distance(m_CurrentHitOrSelectPoint, startPoint);
                    Selected = true;
                }
                else
                {
                    var isValidTarget = UpdateCurrentHitInfo(rayOrigin);
                    m_Hovering = isValidTarget;
                    Selected = isValidTarget && m_XRController.uiPressAction.action.IsPressed();
                    UpdateShowOrHideState();
                }

                UpdateVisuals();
            }
        }

        protected virtual void OnEnable()
        {
            if (m_RayInteractor != null)
                BindToInteractor(m_RayInteractor);

            UpdateShowOrHideState();
        }

        protected virtual void OnDisable()
        {
            m_Hovering = false;
            if (m_RayInteractor != null)
                UnbindToInteractor(m_RayInteractor);
        }

        void BindToInteractor(XRBaseInteractor interactor)
        {
            interactor.selectEntered.AddListener(OnSelectEntered);
            interactor.selectExited.AddListener(OnSelectExited);
        }

        void UnbindToInteractor(XRBaseInteractor interactor)
        {
            interactor.selectEntered.RemoveListener(OnSelectEntered);
            interactor.selectExited.RemoveListener(OnSelectExited);
        }

        bool UpdateCurrentHitInfo(Transform rayOrigin)
        {
            Collider hitCollider = null;
            m_Snapping = false;
            var startPoint = rayOrigin.position;

            // Get Hit Info
            if (RayInteractor.TryGetHitInfo(out m_CurrentHitOrSelectPoint, out m_CurrentHitNormal, out _, out var isValidTarget) &&
                m_CurrentHitNormal != Vector3.zero)
            {
                CurrentRayLength = Vector3.Distance(m_CurrentHitOrSelectPoint, startPoint);
                CheckUIHit(ref hitCollider, ref isValidTarget);
            }
            else
            {
                var defaultLineLength = m_DefaultLineLength;
                CurrentRayLength = OverrideRayLength ?? defaultLineLength;
                var rayDirection = rayOrigin.forward;
                m_CurrentHitNormal = -rayDirection;
                m_CurrentHitOrSelectPoint = startPoint + rayDirection * CurrentRayLength;
            }

            if (hitCollider == null)
            {
                m_InteractionSnappingController = null;
            }

            m_PreviousCollider = hitCollider;

            return isValidTarget;
        }

        void CheckUIHit(ref Collider hitCollider, ref bool isValidTarget)
        {
            if (RayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit)
                && raycastHit.transform != null
                && Utils.IsInLayerMask(m_LayerMask, raycastHit.transform.gameObject.layer))
            {
                if(!isValidTarget)
                {
                    isValidTarget = true;
                }
                else if (m_SnapEndpointIfAvailable)
                {
                    hitCollider = raycastHit.collider;
                    CheckSnapPoint(hitCollider);
                }
            }
        }

        void CheckSnapPoint(Collider hitCollider)
        {
            if (hitCollider != m_PreviousCollider && hitCollider != null)
            {
                m_InteractionSnappingController = hitCollider.GetComponentInChildren<RayInteractionSnappingController>();
            }

            if (m_InteractionSnappingController != null)
            {
                var snappedEndpoint = m_InteractionSnappingController.ClosestPoint(m_CurrentHitOrSelectPoint, ref m_CurrentHitNormal);
                m_CurrentHitOrSelectPoint = snappedEndpoint;
                m_Snapping = true;
            }
        }

        /// <summary>
        ///     Updates visuals based on the state of the target ray interactor.
        ///     Called every frame in which the target ray interactor is non-null.
        /// </summary>
        protected abstract void UpdateVisuals();

        protected virtual void OnSelectEntered(SelectEnterEventArgs args)
        {
            m_SelectedObjectTransform = args.interactableObject.transform;
            var rayOrigin = args.interactableObject.GetAttachTransform(m_RayInteractor);
            UpdateCurrentHitInfo(rayOrigin); // Update the hit info so that capture point is not behind by 1 frame
            m_ObjectLocalSelectPoint = m_SelectedObjectTransform.InverseTransformPoint(m_CurrentHitOrSelectPoint);
        }

        protected virtual void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactableObject == null || args.interactableObject.transform == m_SelectedObjectTransform)
            {
                m_SelectedObjectTransform = null;
            }
        }

        void UpdateShowOrHideState()
        {
            var hide = false;

            if (m_HideIfNotHovering)
                hide = !m_Hovering;

            if (m_HideIfSnapping)
                hide = hide || m_Snapping;

            Show(!hide);
        }

        void Show(bool show)
        {
            Showed?.Invoke(show);
        }
    }
}
