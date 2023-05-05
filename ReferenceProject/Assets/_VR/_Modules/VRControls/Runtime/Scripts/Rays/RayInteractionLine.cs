using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
            UpdateLine(); // Update the line in BeforeRender so that it is based on the latest position of the controller
        }

        /// <summary>
        ///     Called every frame in which the target ray detector is non-null.
        /// </summary>
        protected override void UpdateVisuals()
        {
            UpdateLine();
        }

        void UpdateLine()
        {
            if (RayInteractor == null || !RayInteractor.enabled)
            {
                LineRenderer.enabled = false;
                return;
            }

            var lineRenderable = RayInteractor as ILineRenderable;
            if (lineRenderable == null)
            {
                LineRenderer.enabled = false;
                return;
            }

            lineRenderable.GetLinePoints(ref m_RaycastLinePoints, out var noPoints);

            if (noPoints <= 0)
            {
                LineRenderer.enabled = false;
                return;
            }

            LineRenderer.enabled = !Hidden;

            var bendable = m_LineSettings.Bendable;

            var forward = RayInteractor.attachTransform.forward;
            var startPosition = m_RaycastLinePoints[0] + forward * (m_LineSettings.OffsetStart);

            if (!m_LineSettings.CapLength)
            {
                m_StraightLineEndPoint = startPosition + forward * (CurrentRayLength - (m_LineSettings.OffsetEnd));
            }
            else
            {
                var rayLength = Mathf.Min(CurrentRayLength, m_LineSettings.MaxRayLength);
                m_StraightLineEndPoint = startPosition + forward * rayLength;
            }

            if (bendable)
            {
                m_CurrentEndPoint = CurrentHitOrSelectPoint;
            }
            else
            {
                m_CurrentEndPoint = m_LineSettings.SmoothEndpoint ? Vector3.Lerp(m_CurrentEndPoint, m_StraightLineEndPoint, 1 - Mathf.Exp(-m_LineSettings.FollowTightness * Time.deltaTime)) : m_StraightLineEndPoint;
            }

            if (m_SnapEndPoint)
            {
                m_CurrentEndPoint = m_StraightLineEndPoint;
                m_SnapEndPoint = false;
            }

            var increment = 1f / (m_NewPoints.Length - 1);
            var normalizedPointValue = 0f;
            if (bendable)
            {
                for (var i = 0; i < m_NewPoints.Length; i++)
                {
                    var manipToEndPoint = Vector3.LerpUnclamped(startPosition, m_CurrentEndPoint, normalizedPointValue);
                    var manipToAnchor = Vector3.LerpUnclamped(startPosition, m_StraightLineEndPoint, normalizedPointValue);
                    m_NewPoints[i] = Vector3.LerpUnclamped(manipToAnchor, manipToEndPoint, normalizedPointValue);
                    normalizedPointValue += increment;
                }
            }
            else
            {
                for (var i = 0; i < m_NewPoints.Length; i++)
                {
                    m_NewPoints[i] = Vector3.LerpUnclamped(startPosition, m_CurrentEndPoint, normalizedPointValue);
                    normalizedPointValue += increment;
                }
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
}
