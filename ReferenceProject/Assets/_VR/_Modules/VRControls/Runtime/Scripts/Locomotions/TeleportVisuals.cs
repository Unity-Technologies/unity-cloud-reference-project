using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     Controls the visuals and trajectory calculations for <see cref="Teleport" />
    /// </summary>
    public class TeleportVisuals : MonoBehaviour
    {
        [SerializeField, Tooltip("The max teleport lateral distance.")]
        float m_TeleportDistance = 15f;

        [SerializeField, Tooltip("The distance between each segment of the arc ray.")]
        float m_TimeStep = 0.75f;

        [SerializeField, Tooltip("The maximum number of segments in the arc ray.")]
        int m_MaxProjectileSteps = 150;

        [SerializeField, Tooltip("The time it takes for the ray to reach its normal teleport distance when aiming begins.")]
        float m_TransitionTime = 0.03f;

        [SerializeField, Tooltip("The GameObject that will be positioned at the target position.")]
        GameObject m_TargetRigPoseVisual;

        [SerializeField, Tooltip("The GameObject that will be rotated to indicate the center of the cameras field of view.")]
        GameObject m_TargetCameraForwardVisual;

        [SerializeField, Tooltip("The color gradient to use for the teleport ray.")]
        Gradient m_Gradient;

        [SerializeField, Tooltip("The color gradient to use for the teleport ray when there is no valid target position.")]
        Gradient m_InvalidGradient;

        [SerializeField, Tooltip("The amount of dampening applied to the forward direction of the teleport ray. Lower values will cause it to lag behind more.")]
        float m_LineDampening = 10f;

        [SerializeField, Tooltip("The teleport aim line width")]
        float m_LineWidth = 0.075f;

        [SerializeField, Tooltip("The teleport aim line width curve from the origin to the hit point. A curve value of 1 will correspond to full width as determined by the Line Width value.")]
        AnimationCurve m_WidthCurve;

        bool m_AnimPlaying;
        Vector3 m_EasedForward = Vector3.forward;
        Vector3[] m_EasedLineVertexPositions;
        Vector3 m_EasedStartPosition = Vector3.zero;
        Vector3 m_HitNormal;
        int m_LastHitIndex;

        LineRenderer m_LineRenderer;
        Vector3[] m_LineVertexPositions;
        bool m_Picking;
        Vector3[] m_Positions;
        bool m_ResetAimEasing;
        bool m_FinishedComputeFirstRayHit;

        IObjectPicker m_TeleportPicker;
        float m_TransitionAmount;
        Coroutine m_VisibilityCoroutine;

        bool m_Visible;

        int k_UILayerMask;

        [Inject]
        public void Setup(IObjectPicker picker)
        {
            m_TeleportPicker = picker;
        }

        /// <summary>
        ///     The max teleport lateral distance.
        /// </summary>
        public float TeleportDistance
        {
            get => m_TeleportDistance;
            set => m_TeleportDistance = value;
        }

        /// <summary>
        ///     The target position to teleport to, if there is one
        /// </summary>
        public Vector3? targetPosition { get; private set; }

        /// <summary>
        ///     The target rotation to teleport to, if there is one
        /// </summary>
        public Quaternion? targetRotation { get; private set; }

        /// <summary>
        ///     A set of gameObjects to ignore when casting the teleport ray
        /// </summary>
        public HashSet<GameObject> ignoredGameObjects { private get; set; }

        /// <summary>
        ///     The XRRyInteractor associated with the Teleport script which created this visuals object.
        /// </summary>
        public XRRayInteractor xrRayInteractor { get; set; }

        /// <summary>
        ///     Sets whether the Teleport visuals should currently be visible or not.
        /// </summary>
        public void SetVisible(bool value)
        {
            if (value == m_Visible)
                return;

            m_Visible = value;

            if (m_Visible)
            {
                gameObject.SetActive(true);

                m_Picking = false;

                targetPosition = null;
                m_ResetAimEasing = true;

                if (m_VisibilityCoroutine != null)
                    StopCoroutine(m_VisibilityCoroutine);

                m_VisibilityCoroutine = StartCoroutine(VisibilityTransition(true));
            }
            else if (isActiveAndEnabled)
            {
                if (m_VisibilityCoroutine != null)
                    StopCoroutine(m_VisibilityCoroutine);

                m_VisibilityCoroutine = StartCoroutine(VisibilityTransition(false));
            }

            if (xrRayInteractor != null)
                xrRayInteractor.enabled = !m_Visible;
        }

        void Awake()
        {
            m_Positions = new Vector3[m_MaxProjectileSteps];
            m_EasedLineVertexPositions = new Vector3[m_MaxProjectileSteps];
            m_LineVertexPositions = new Vector3[m_MaxProjectileSteps + 1];

            m_LineRenderer = GetComponent<LineRenderer>();
            m_LineRenderer.positionCount = m_MaxProjectileSteps;
            m_LineRenderer.SetPositions(m_LineVertexPositions);
            m_LineRenderer.widthCurve = m_WidthCurve;

            k_UILayerMask = LayerMask.NameToLayer("UI");
        }

        void Update()
        {
            var rayOrigin = xrRayInteractor?.rayOriginTransform;
            if (rayOrigin == null)
                return;

            var currentTeleportDistance = m_TeleportDistance;

            var gravity = Physics.gravity;
            if (gravity == Vector3.zero)
                gravity = Vector3.down; // Assume (0,-1,0) if gravity is zero

            var timeStep = m_TimeStep / Mathf.Sqrt(currentTeleportDistance * gravity.magnitude);

            currentTeleportDistance *= m_TransitionAmount;
            var speed = Mathf.Sqrt(currentTeleportDistance * gravity.magnitude);
            var aimRayOrigin = rayOrigin;
            var velocity = aimRayOrigin.forward * (speed * timeStep);
            gravity *= timeStep * timeStep;
            var lastPosition = aimRayOrigin.position + rayOrigin.forward * 0.04f;

            var easeFactor = 1f;
            if (!m_ResetAimEasing)
                easeFactor -= Mathf.Exp(-m_LineDampening * Time.deltaTime);
            else
                m_ResetAimEasing = false;

            m_EasedStartPosition = Vector3.Lerp(m_EasedStartPosition, lastPosition, easeFactor);
            m_EasedForward = Vector3.Slerp(m_EasedForward, aimRayOrigin.forward, easeFactor);
            var easedVelocity = m_EasedForward * (speed * timeStep);

            CalculateTrajectory(lastPosition, velocity, gravity, ref m_Positions);
            CalculateTrajectory(m_EasedStartPosition, easedVelocity, gravity, ref m_EasedLineVertexPositions);

            FindTargetPosition(m_EasedLineVertexPositions).ConfigureAwait(false);
        }

        void OnEnable()
        {
            for (var i = 0; i < m_MaxProjectileSteps; i++)
            {
                var pos = transform.position;
                m_Positions[i] = pos;
                m_EasedLineVertexPositions[i] = pos;
                m_LineVertexPositions[i] = pos;
            }

            m_FinishedComputeFirstRayHit = false;

            m_LineRenderer.SetPositions(m_LineVertexPositions);
        }

        void OnDisable()
        {
            targetPosition = null;
            m_AnimPlaying = false;
        }

        /// <summary>
        ///     Rotates the forward direction to teleport.
        /// </summary>
        /// <param name="angle">The rotation angle being applied to the target rotation.</param>
        /// <param name="rigRotation">The rig's current rotation.</param>
        /// <param name="lookRotation">The current direction that the camera is looking.</param>
        public void Rotate(float angle, Quaternion rigRotation, Quaternion lookRotation)
        {
            var rotateAmount = Quaternion.Euler(0f, angle, 0f);
            targetRotation = rigRotation * rotateAmount;

            var rotateToNormal = Quaternion.FromToRotation(Vector3.up, m_HitNormal);
            var rotateEase = 1f;

            m_TargetCameraForwardVisual.transform.rotation = Quaternion.Slerp(m_TargetCameraForwardVisual.transform.rotation, rotateToNormal * lookRotation * rotateAmount, rotateEase);
        }

        /// <summary>
        ///     Starts the animation that occurs after a teleport is started, but before the rig moves.
        /// </summary>
        /// <param name="duration"></param>
        public void PlayRayAnim(float duration)
        {
            m_AnimPlaying = true;
            StartCoroutine(RayTransition(duration));
        }

        IEnumerator VisibilityTransition(bool isVisible)
        {
            var startValue = m_TransitionAmount;
            var targetValue = isVisible ? 1f : 0f;
            var startTime = Time.time;
            var timeDiff = Time.time - startTime;
            while (timeDiff < m_TransitionTime)
            {
                m_TransitionAmount = Mathf.Lerp(startValue, targetValue, timeDiff / m_TransitionTime);
                timeDiff = Time.time - startTime;
                yield return null;
            }

            m_TransitionAmount = targetValue;

            if (!isVisible)
                gameObject.SetActive(false);
        }

        IEnumerator RayTransition(float duration)
        {
            var elapsedTime = 0f;
            var gradientColorKeys = m_Gradient.colorKeys;
            var start = new GradientAlphaKey(0f, 0f);
            var end = new GradientAlphaKey(1f, 1f);
            var alphaKeys = new[] { start, start, end };

            while (elapsedTime < duration)
            {
                var t = CubicEaseOut(elapsedTime / duration);

                var gradient = new Gradient();
                var mid = new GradientAlphaKey(1f, t);
                alphaKeys[1] = mid;
                gradient.SetKeys(gradientColorKeys, alphaKeys);

                m_LineRenderer.colorGradient = gradient;
                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        static float CubicEaseOut(float t)
        {
            var f = t - 1;
            return f * f * f + 1;
        }

        void CalculateTrajectory(Vector3 currentPosition, Vector3 velocity, Vector3 gravity, ref Vector3[] positions)
        {
            for (var i = 0; i < m_MaxProjectileSteps; i++)
            {
                var nextPosition = currentPosition + velocity;
                velocity += gravity;
                var segment = nextPosition - currentPosition;

                if (segment == Vector3.zero)
                    break;

                positions[i] = currentPosition;
                currentPosition = nextPosition;
            }
        }

        async Task FindTargetPosition(IReadOnlyList<Vector3> linePoints)
        {
            bool hasHitUI = false;
            var size = linePoints.Count;
            if (m_TeleportPicker == null || size == 0 || linePoints[0] == linePoints[size- 1])
            {
                return;
            }

            if (m_Picking)
            {
                if (m_FinishedComputeFirstRayHit)
                {
                    // Reuse the last target value for in between frame before callback
                    SetTargetPosition(targetPosition, m_LastHitIndex);
                }
                return;
            }

            // Start picking & generate new values
            m_Picking = true;

            // pick
            var hitIndex = linePoints.Count - 1;
            Vector3? hitPosition = null;
            var step = 32; // Remove this when the picker will be able to handle raycast by bunch
            for (int i = 0; i < size - 2; i+=step)
            {
                if (i + step > size - 1)
                {
                    step = size - i - 1;
                }

                var ray = new Ray(linePoints[i], linePoints[i + step] - linePoints[i]);
                var distance = Vector3.Distance(linePoints[i], linePoints[i + step]);

                // Detect if the ray hit a UI element
                if(Physics.Raycast(ray, out RaycastHit hitInfo, distance, 1 << k_UILayerMask))
                {
                    hitIndex = i;
                    hitPosition = hitInfo.point;
                    hasHitUI = true;
                    break;
                }

                var result = await m_TeleportPicker.RaycastAsync(ray, distance);
                if(result.HasIntersected)
                {
                    hitIndex = i;
                    hitPosition = result.Point;
                    m_HitNormal = Vector3.up; // Until we have a way to get the normal from the picker
                    break;
                }
            }

            m_FinishedComputeFirstRayHit = true;
            m_Picking = false;

            SetTargetPosition(hitPosition, hitIndex, hasHitUI);
        }

        void SetTargetPosition(Vector3? position, int easedHitIndex, bool hasHitUI = false)
        {
            bool isValid;
            bool isVerticalSurface = false;

            targetPosition = position;
            m_LastHitIndex = targetPosition.HasValue && easedHitIndex == m_MaxProjectileSteps - 1 ? m_LastHitIndex : easedHitIndex;
            if (m_AnimPlaying)
            {
                targetPosition = null;
            }

            if(hasHitUI)
            {
                isValid = false;
            }
            else
            {
                // Check if destination is too much vertical
                isVerticalSurface = Vector3.Dot(m_HitNormal, Vector3.up) < 0.3f;
                isValid = targetPosition.HasValue && !isVerticalSurface;
            }
            m_TargetRigPoseVisual.SetActive(isValid);

            if (isValid)
            {
                m_TargetRigPoseVisual.transform.rotation = Quaternion.identity;
                m_TargetRigPoseVisual.transform.position = targetPosition.Value;
                m_TargetRigPoseVisual.transform.localScale = Vector3.one;
            }

            if (!m_Visible)
            {
                targetPosition = null;
                return;
            }

            var color = isValid ? m_Gradient : m_InvalidGradient;

            for (var i = 0; i <= m_LastHitIndex; i++)
            {
                var bendAmount = Mathf.Clamp01((float)i / (m_LastHitIndex + 1));
                m_LineVertexPositions[i] = Vector3.Lerp(m_Positions[i], m_EasedLineVertexPositions[i], bendAmount);
            }

            // The final point of the visible vertices is directly set to the target point, because the line vertex positions will end at the start of the segment that resulted in a hit.
            m_LineVertexPositions[m_LastHitIndex + 1] = targetPosition.HasValue ? Vector3.Lerp(m_LineVertexPositions[m_LastHitIndex], targetPosition.Value, 0.8f) : m_LineVertexPositions[m_LastHitIndex];
            m_LineRenderer.widthMultiplier = hasHitUI ? 0.01f : m_LineWidth;
            m_LineRenderer.colorGradient = color;
            m_LineRenderer.positionCount = m_LastHitIndex + 2;
            m_LineRenderer.SetPositions(m_LineVertexPositions);

            if (hasHitUI || isVerticalSurface)
            {
                targetPosition = null;
            }
        }
    }
}
