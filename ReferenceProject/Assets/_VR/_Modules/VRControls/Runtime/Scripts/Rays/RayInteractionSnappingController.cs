using UnityEngine;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    /// Component that marks a collider as a volume for ray interaction renderers (cursors and bendable rays) to snap to when a parent collider for an interactable is hovered.
    /// If there is no collider specified, than the transform poisition itself is used as the closest point
    /// </summary>
    public class RayInteractionSnappingController : MonoBehaviour
    {
        [SerializeField, Tooltip("(Optional) The collider that will be used to determine the closest point for a ray to snap to. If null, the transform's position will be the snap point always.")]
        Collider m_SnappingCollider;

        /// <summary>
        /// The collider that will be used to find the closest point to snap to. If this is null, then this gameObject's transform position will be used always.
        /// </summary>
        public Collider snappingCollider
        {
            get => m_SnappingCollider;
            set => m_SnappingCollider = value;
        }

        /// <summary>
        /// Calculates the closest point on the volume and the normal of the collider surface
        /// </summary>
        /// <param name="point">The input point that should snap to the volume.</param>
        /// <param name="normal">The surface normal of the collider volume at the snapped position.
        /// If the input point is exactly on the volume then the normal vector will not be modified, because a valid raycast is needed to get a new surface normal.</param>
        /// <returns>The snapped position that is the closest point on the collider to the input point.</returns>
        public Vector3 ClosestPoint(Vector3 point, ref Vector3 normal)
        {
            if (m_SnappingCollider == null)
                return transform.position;

            var closestPoint = m_SnappingCollider.ClosestPoint(point);
            if(closestPoint == point)
                return point;

            var vectorToClosestPoint = closestPoint - point;

            var normalizedDirection = vectorToClosestPoint.normalized;
            if (m_SnappingCollider.Raycast(new Ray(point, normalizedDirection), out var hit, vectorToClosestPoint.magnitude * 2f))
            {
                normal = hit.normal;
            }

            return closestPoint;
        }
    }
}
