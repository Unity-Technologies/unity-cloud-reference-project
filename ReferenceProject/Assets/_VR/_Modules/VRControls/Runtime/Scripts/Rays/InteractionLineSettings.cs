using System;
using UnityEngine;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     Set of properties for the visual appearance of a interaction line including width, color, and bendiness
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "NewLineSettings.asset", menuName = "ReferenceProject/VRManager/Interaction Line Settings")]
    public class InteractionLineSettings : ScriptableObject
    {
        [SerializeField, Tooltip("The width of the line (in centimeters).")]
        float m_LineWidth = 0.2f;

        [SerializeField, Tooltip("The relative width of the line from the start to the end.")]
        AnimationCurve m_WidthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [SerializeField, Tooltip("The color of the line as a gradient from start to end.")]
        Gradient m_LineColorGradient;

        [SerializeField, Tooltip("The minimum number of vertices used to draw the line. Increase this for a smoother color gradients, width changes, and bending.")]
        int m_MinimumVertexCount = 32;

        [SerializeField, Tooltip("If enabled, the line will be drawn to bend from the direction the ray origin is pointing to the actual endpoint.")]
        bool m_Bendable = true;

        [SerializeField, Tooltip("If enabled, the end point of the line will smoothly follow the end of the ray.")]
        bool m_SmoothEndpoint;

        [SerializeField, Tooltip("Controls the speed that the line's endpoint will follow the end of the ray, if Smooth Endpoint is enabled.")]
        float m_FollowTightness = 50f;

        [SerializeField, Tooltip("If enabled, the ray will be limited to a max length.")]
        bool m_CapLength = true;

        [SerializeField, Tooltip("The max ray length, if Cap Length is enabled.")]
        float m_MaxRayLength = 4f;

        [SerializeField, Tooltip("The offset in z direction at the start of the line visual.")]
        float m_OffsetStart = 0.02f;

        [SerializeField, Tooltip("The offset in z direction at the end of the line visual.")]
        float m_OffsetEnd = 0.05f;

        /// <summary>
        ///     The width of the line (in centimeters).
        /// </summary>
        public float LineWidth
        {
            get => m_LineWidth;
            set => m_LineWidth = value;
        }

        /// <summary>
        ///     The relative width of the line from the start to the end.
        /// </summary>
        public AnimationCurve WidthCurve
        {
            get => m_WidthCurve;
            set => m_WidthCurve = value;
        }

        /// <summary>
        ///     The color of the line as a gradient from start to end.
        /// </summary>
        public Gradient LineColorGradient => m_LineColorGradient;

        /// <summary>
        ///     The minimum number of vertices used to draw the line. Increase this for a smoother color gradients, width changes,
        ///     and bending.
        /// </summary>
        public int MinimumVertexCount => m_MinimumVertexCount;

        /// <summary>
        ///     If enabled, the line will be drawn to bend from the direction the ray origin is pointing to the actual endpoint.
        /// </summary>
        public bool Bendable
        {
            get => m_Bendable;
            set => m_Bendable = value;
        }

        /// <summary>
        ///     Controls the speed that the line's endpoint will follow the end of the ray, if Smooth Endpoint is enabled.
        /// </summary>
        public float FollowTightness
        {
            get => m_FollowTightness;
            set => m_FollowTightness = value;
        }

        /// <summary>
        ///     If enabled, the line's endpoint will smoothly follow the end of the ray instead of being directly at the end point.
        /// </summary>
        public bool SmoothEndpoint
        {
            get => m_SmoothEndpoint;
            set => m_SmoothEndpoint = value;
        }

        /// <summary>
        ///     If enabled, the ray will be limited to a max length.
        /// </summary>
        public bool CapLength
        {
            get => m_CapLength;
            set => m_CapLength = value;
        }

        /// <summary>
        ///     The max ray length, if Cap Length is enabled.
        /// </summary>
        public float MaxRayLength
        {
            get => m_MaxRayLength;
            set => m_MaxRayLength = value;
        }

        /// <summary>
        ///     The offset in z direction at the start of the line visual.
        /// </summary>
        public float OffsetStart
        {
            get => m_OffsetStart;
            set => m_OffsetStart = value;
        }

        /// <summary>
        ///     The offset in z direction at the end of the line visual.
        /// </summary>
        public float OffsetEnd
        {
            get => m_OffsetEnd;
            set => m_OffsetEnd = value;
        }
    }
}
