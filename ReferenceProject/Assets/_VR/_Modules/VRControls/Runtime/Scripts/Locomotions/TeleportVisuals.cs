using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        int m_MaxProjectileSteps = 256;

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
        bool m_LastHitUI;
        float m_ConvertedTimeStep;
        Vector3 m_Gravity;
        Vector3 m_ConvertedGravity;

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
        int m_StepSize;

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
        public Vector3? TargetPosition { get; private set; }

        /// <summary>
        ///     The target rotation to teleport to, if there is one
        /// </summary>
        public Quaternion? TargetRotation { get; private set; }

        /// <summary>
        ///     A set of gameObjects to ignore when casting the teleport ray
        /// </summary>
        public HashSet<GameObject> IgnoredGameObjects { private get; set; }

        /// <summary>
        ///     The XRRyInteractor associated with the Teleport script which created this visuals object.
        /// </summary>
        public XRRayInteractor XRRayInteractor { get; set; }

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

                TargetPosition = null;
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

            if (XRRayInteractor != null)
                XRRayInteractor.enabled = !m_Visible;
        }

        void Awake()
        {
#if UNITY_ANDROID
            // Limit the number of projectile steps on Android to avoid performance issues
            if (m_MaxProjectileSteps <= 0 || m_MaxProjectileSteps > 64)
            {
                m_TimeStep = (m_TimeStep * m_MaxProjectileSteps) / 64;
                m_MaxProjectileSteps = 64;
            }
#endif
            if (m_MaxProjectileSteps != 0 && (m_MaxProjectileSteps & (m_MaxProjectileSteps - 1)) != 0)
            {
                Debug.LogWarning("Max Projectile Steps should be a power of 2 for performance reasons.");
                m_MaxProjectileSteps = (int)Math.Pow(2, (int)Math.Log(m_MaxProjectileSteps, 2));
            }

            m_StepSize = Mathf.FloorToInt(Mathf.Sqrt(m_MaxProjectileSteps));
            m_Gravity = Physics.gravity;
            if (m_Gravity == Vector3.zero)
            {
                m_Gravity = Vector3.down; // Assume (0,-1,0) if gravity is zero
            }

            m_ConvertedTimeStep = m_TimeStep / Mathf.Sqrt(m_TeleportDistance * m_Gravity.magnitude);
            m_ConvertedGravity = m_Gravity * m_ConvertedTimeStep * m_ConvertedTimeStep;

            m_Positions = new Vector3[m_MaxProjectileSteps];
            m_EasedLineVertexPositions = new Vector3[m_MaxProjectileSteps];
            m_LineVertexPositions = new Vector3[m_MaxProjectileSteps];

            m_LineRenderer = GetComponent<LineRenderer>();
            m_LineRenderer.positionCount = m_MaxProjectileSteps;
            m_LineRenderer.SetPositions(m_LineVertexPositions);
            m_LineRenderer.widthCurve = m_WidthCurve;

            k_UILayerMask = LayerMask.NameToLayer("UI");
        }

        void Update()
        {
            var rayOrigin = XRRayInteractor?.rayOriginTransform;
            if (rayOrigin == null)
                return;

            var currentTeleportDistance = m_TeleportDistance;

            currentTeleportDistance *= m_TransitionAmount;
            var speed = Mathf.Sqrt(currentTeleportDistance * m_Gravity.magnitude);
            var aimRayOrigin = rayOrigin;
            var velocity = aimRayOrigin.forward * (speed * m_ConvertedTimeStep);

            var lastPosition = aimRayOrigin.position + rayOrigin.forward * 0.04f;

            var easeFactor = 1f;
            if (!m_ResetAimEasing)
            {
                easeFactor -= Mathf.Exp(-m_LineDampening * Time.deltaTime);
            }
            else
            {
                m_ResetAimEasing = false;
            }

            m_EasedStartPosition = Vector3.Lerp(m_EasedStartPosition, lastPosition, easeFactor);
            m_EasedForward = Vector3.Slerp(m_EasedForward, aimRayOrigin.forward, easeFactor);
            var easedVelocity = m_EasedForward * (speed * m_ConvertedTimeStep);

            CalculateTrajectory(lastPosition, velocity, m_ConvertedGravity, ref m_Positions);
            CalculateTrajectory(m_EasedStartPosition, easedVelocity, m_ConvertedGravity, ref m_EasedLineVertexPositions);

            _ = FindTargetPosition(m_EasedLineVertexPositions.ToList());
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
            TargetPosition = null;
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
            TargetRotation = rigRotation * rotateAmount;

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

        async Task FindTargetPosition(List<Vector3> linePoints)
        {
            bool hasHitUI = false;
            var size = linePoints.Count;
            if (m_TeleportPicker == null || size == 0 || linePoints[0] == linePoints[size - 1])
                return;

            if (m_Picking)
            {
                if (m_FinishedComputeFirstRayHit && !m_LastHitUI)
                {
                    // Reuse the last target value for in between frame before callback
                    SetTargetPosition(TargetPosition, m_LastHitIndex);
                }

                return;
            }

            // Start picking & generate new values
            m_Picking = true;

            // pick
            var hitIndex = linePoints.Count - 1;
            Vector3? hitPosition = null;

            List<Vector3> subPoints = new List<Vector3>();
            for (int i = 0; i < linePoints.Count; i += m_StepSize)
            {
                subPoints.Add(linePoints[i]);
            }

            subPoints.Add(linePoints[linePoints.Count - 1]);

            var result = await m_TeleportPicker.PickFromPathAsync(subPoints.ToArray());
            if (result.Index != -1)
            {
                var secondSubPoints = new List<Vector3>();
                var start = result.Index * m_StepSize;
                for (int i = start; i <= start + m_StepSize + 1 && i < linePoints.Count; i++)
                {
                    secondSubPoints.Add(linePoints[i]);
                }

                var secondResult = await m_TeleportPicker.PickFromPathAsync(secondSubPoints.ToArray());
                if (secondResult.Index != -1)
                {
                    // Find hit index
                    hitIndex = start + secondResult.Index;
                    hitPosition = secondResult.PickerResult.Point;
                    m_HitNormal = secondResult.PickerResult.Normal;
                }
            }

            // Detect if the ray hit a UI element
            hasHitUI = CheckRaycastUI(linePoints, ref hitIndex, ref hitPosition);

            m_FinishedComputeFirstRayHit = true;
            m_Picking = false;

            SetTargetPosition(hitPosition, hitIndex, hasHitUI);
        }

        bool CheckRaycastUI(List<Vector3> linePoints, ref int hitIndex, ref Vector3? hitPosition)
        {
            for (int i = 0; i < linePoints.Count - 1; i++)
            {
                var ray = new Ray(linePoints[i], linePoints[i + 1] - linePoints[i]);
                var distance = Vector3.Distance(linePoints[i], linePoints[i + 1]);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, 1 << k_UILayerMask))
                {
                    hitIndex = i;
                    hitPosition = hitInfo.point;
                    return true;
                }
            }

            return false;
        }

        void SetTargetPosition(Vector3? position, int easedHitIndex, bool hasHitUI = false)
        {
            bool isValid;
            bool isVerticalSurface = false;

            TargetPosition = position;
            m_LastHitUI = hasHitUI;
            m_LastHitIndex = TargetPosition.HasValue && easedHitIndex == m_MaxProjectileSteps - 1 ? m_LastHitIndex : easedHitIndex;
            if (m_AnimPlaying)
            {
                TargetPosition = null;
            }

            if (hasHitUI)
            {
                isValid = false;
            }
            else
            {
                // Check if destination is too much vertical
                isVerticalSurface = Vector3.Dot(m_HitNormal, Vector3.up) < 0.3f;
                isValid = TargetPosition.HasValue && !isVerticalSurface;
            }

            m_TargetRigPoseVisual.SetActive(isValid);

            if (isValid)
            {
                m_TargetRigPoseVisual.transform.rotation = Quaternion.identity;
                m_TargetRigPoseVisual.transform.position = TargetPosition.Value;
                m_TargetRigPoseVisual.transform.localScale = Vector3.one;
            }

            if (!m_Visible)
            {
                TargetPosition = null;
                return;
            }

            var color = isValid ? m_Gradient : m_InvalidGradient;

            var lastVertexPosition = m_EasedLineVertexPositions[m_LastHitIndex];
            var delta = TargetPosition.HasValue ? TargetPosition.Value - lastVertexPosition : Vector3.zero;
            for (var i = 0; i <= m_LastHitIndex; i++)
            {
                var bendAmount = Mathf.Clamp01((float)i / (m_LastHitIndex + 1));
                m_LineVertexPositions[i] = Vector3.Lerp(m_Positions[i], m_EasedLineVertexPositions[i], bendAmount) + Vector3.Lerp(Vector3.zero, delta, bendAmount * bendAmount);
            }

            m_LineRenderer.widthMultiplier = hasHitUI ? 0.01f : m_LineWidth;
            m_LineRenderer.colorGradient = color;
            m_LineRenderer.positionCount = m_LastHitIndex + 1;
            m_LineRenderer.SetPositions(m_LineVertexPositions);

            if (hasHitUI || isVerticalSurface)
            {
                TargetPosition = null;
            }
        }
    }
}
