using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Vector3 = UnityEngine.Vector3;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     A line renderer that uses a ray interactor to drive its visuals.
    ///     The line will bend if there are more than 2 vertices.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(LineWidthController))]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_LineVisual)]
    public class RayInteractionLine : RayInteractionRenderer
    {
        [SerializeField, Tooltip("Specifies the settings scriptable object that defines the visual style of this line.")]
        InteractionLineSettings m_LineSettings;
        Vector3 m_CurrentEndPoint;

        LineRenderer m_LineRenderer;
        LineWidthController m_LineWidthController;
        Vector3[] m_NewPoints;
        Vector3[] m_RaycastLinePoints = new Vector3[2];
        bool m_SnapEndPoint = true;
        Vector3 m_StraightLineEndPoint;

        LineRenderer LineRenderer
        {
            get
            {
                if (m_LineRenderer == null)
                {
                    m_LineRenderer = GetComponent<LineRenderer>();
                    if (m_LineRenderer != null)
                        m_LineWidthController = m_LineRenderer.GetComponent<LineWidthController>();
                }

                return m_LineRenderer;
            }
        }

        /// <summary>
        ///     Reference to the lineSettings scriptable object that contains the visual properties for this line.
        /// </summary>
        public InteractionLineSettings LineSettings
        {
            get => m_LineSettings;
            set
            {
                if (m_LineSettings == value)
                    return;

                m_LineSettings = value;
                UpdateSettings();
            }
        }

        /// <summary>
        ///     If enabled, the line will be hidden
        /// </summary>
        public bool Hidden { get; set; }

        protected void Awake()
        {
            if (m_LineSettings == null)
            {
                Debug.LogError("Ray Interaction Line does not have a line settings asset reference.", this);
            }
            else
            {
                m_NewPoints = new Vector3[m_LineSettings.MinimumVertexCount];
                UpdateSettings();
            }
        }

        protected virtual void Reset()
        {
            LineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
            UpdateSettings();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LineRenderer.enabled = true;
            m_SnapEndPoint = true;
            Application.onBeforeRender += OnBeforeRenderVisuals;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LineRenderer.enabled = false;
            Application.onBeforeRender -= OnBeforeRenderVisuals;
        }

        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderLineVisual)]
        void OnBeforeRenderVisuals()
        {
            UpdateVisuals(); // Update the line in BeforeRender so that it is based on the latest position of the controller
        }

        /// <summary>
        ///     Called every frame in which the target ray detector is non-null.
        /// </summary>
        protected override void UpdateVisuals()
        {
            if (IsLineRenderable())
            {
                GenerateLine();
            }
            else
            {
                m_LineRenderer.enabled = false;
            }
        }

        bool IsLineRenderable()
        {
            if (RayInteractor == null)
            {
                return false;
            }

            if (!RayInteractor.enabled)
            {
                return false;
            }

            var lineRenderable = RayInteractor as ILineRenderable;
            if (lineRenderable == null)
            {
                return false;
            }

            lineRenderable.GetLinePoints(ref m_RaycastLinePoints, out var pointCount);

            return pointCount > 0;
        }

        void GenerateLine()
        {
            LineRenderer.enabled = !Hidden;

            var forward = RayInteractor.attachTransform.forward;
            var startPosition = m_RaycastLinePoints[0] + forward * (m_LineSettings.OffsetStart);
            var rayLength =  m_LineSettings.CapLength
                ? Mathf.Min(CurrentRayLength, m_LineSettings.MaxRayLength)
                : CurrentRayLength - m_LineSettings.OffsetEnd;

            m_StraightLineEndPoint = startPosition + forward * rayLength;

            var bendable = m_LineSettings.Bendable;

            if (bendable)
            {
                m_CurrentEndPoint = CurrentHitOrSelectPoint;
            }
            else
            {
                m_CurrentEndPoint = m_LineSettings.SmoothEndpoint
                    ? Vector3.Lerp(m_CurrentEndPoint, m_StraightLineEndPoint, 1 - Mathf.Exp(-m_LineSettings.FollowTightness * Time.deltaTime))
                    : m_StraightLineEndPoint;
            }

            if (m_SnapEndPoint)
            {
                m_CurrentEndPoint = m_StraightLineEndPoint;
                m_SnapEndPoint = false;
            }

            if (bendable)
            {
                LineUtil.CreateCurvePositions(startPosition, m_CurrentEndPoint, m_StraightLineEndPoint, ref m_NewPoints);
            }
            else
            {
                LineUtil.CreateLinePositions(startPosition, m_CurrentEndPoint, ref m_NewPoints);
            }

            LineRenderer.SetPositions(m_NewPoints);
        }

        [ContextMenu("Force Update Line Renderer")]
        void ForceUpdate()
        {
            UpdateSettings();

            for (var i = 0; i < m_NewPoints.Length; i++)
            {
                var normalizedPointValue = (float)(i) / (m_NewPoints.Length - 1);

                m_NewPoints[i] = Vector3.Lerp(transform.position, transform.TransformPoint(new Vector3(0, 0, m_DefaultLineLength)), normalizedPointValue);
            }

            LineRenderer.SetPositions(m_NewPoints);
        }

        void UpdateSettings()
        {
            LineRenderer.positionCount = m_LineSettings.MinimumVertexCount;
            m_NewPoints = new Vector3[m_LineSettings.MinimumVertexCount];
            m_LineWidthController.DefaultWidthMultiplier = m_LineSettings.LineWidth;
            LineRenderer.widthCurve = m_LineSettings.WidthCurve;
            LineRenderer.colorGradient = m_LineSettings.LineColorGradient;
            m_SnapEndPoint = true;
        }
    }

    public static class LineUtil
    {
        public static void CreateLinePositions(Vector3 startPosition, Vector3 endPosition, ref Vector3[] positions)
        {
            // -1 because the set does not contain the endpoint
            var stepping = 1f / (positions.Length - 1);
            var t = 0f;
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = Vector3.LerpUnclamped(startPosition, endPosition, t);
                t += stepping;
            }
        }

        public static void CreateCurvePositions(Vector3 startPosition, Vector3 endPosition, Vector3 control, ref Vector3[] positions)
        {
            // -1 because the set does not contain the endpoint
            var t = 1f / (positions.Length - 1);
            var stepping = 0f;

            for (var i = 0; i < positions.Length; i++)
            {
                var manipToEndPoint = Vector3.LerpUnclamped(startPosition, endPosition, stepping);
                var manipToAnchor = Vector3.LerpUnclamped(startPosition, control, stepping);
                positions[i] = Vector3.LerpUnclamped(manipToAnchor, manipToEndPoint, stepping);
                stepping += t;
            }
        }

    }
}
