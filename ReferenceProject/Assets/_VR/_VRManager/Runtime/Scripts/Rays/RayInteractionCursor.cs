using System;
using UnityEngine;

namespace Unity.ReferenceProject.VRManager
{
    /// <summary>
    ///     A cursor renderer that uses a ray interactor to drive its visuals.
    /// </summary>
    public class RayInteractionCursor : RayInteractionRenderer
    {
        [SerializeField, Tooltip("Whether this game object should scale 1-1 with its distance from the camera. " +
             "Useful for keeping the cursor from popping to a larger or smaller size after it leaves a surface.")]
        bool m_ScaleWithDistanceFromCamera = true;

        [SerializeField, Tooltip("Whether this object's forward vector should mirror the normal of any surface it hits.")]
        bool m_AlignToHitNormal = true;

        [SerializeField, Tooltip("Whether this object's up vector should follow the up vector of the ray origin.")]
        bool m_AlignToRayOriginUp;

        [SerializeField]
        Camera m_Camera;
        bool m_CameraFound;
        Transform m_MainCameraTransform;

        Vector3 m_OriginalScale;

        protected void Awake()
        {
            m_OriginalScale = transform.localScale;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_Camera != null)
                m_MainCameraTransform = m_Camera.transform;

            m_CameraFound = m_MainCameraTransform != null;
        }

        /// <summary>
        ///     Updates the cursor based on the state of the target ray detector.
        ///     Called every frame in which the target ray detector is non-null.
        /// </summary>
        protected override void UpdateVisuals()
        {
            var rayOrigin = RayInteractor.attachTransform;

            var thisTransform = transform;
            thisTransform.position = CurrentHitOrSelectPoint;

            if (m_ScaleWithDistanceFromCamera && m_CameraFound)
            {
                var distanceFromCamera = (thisTransform.position - m_MainCameraTransform.position).magnitude;
                thisTransform.localScale = m_OriginalScale * distanceFromCamera;
            }

            var newForward = m_AlignToHitNormal ? -CurrentHitNormal : rayOrigin.forward;

            if (m_AlignToRayOriginUp)
            {
                var newRot = thisTransform.rotation;
                newRot.SetLookRotation(newForward, rayOrigin.up);
                thisTransform.rotation = newRot;
            }
            else
            {
                thisTransform.forward = newForward;
            }
        }
    }
}
