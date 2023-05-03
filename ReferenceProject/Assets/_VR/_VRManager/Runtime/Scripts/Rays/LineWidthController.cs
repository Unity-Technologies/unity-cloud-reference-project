using System;
using UnityEngine;

namespace Unity.ReferenceProject.VRManager
{
    /// <summary>
    ///     Component that controls the width of a line renderer.
    ///     The line width will be adjusted based on the current viewer scale and a temporary width modifier that can be set by
    ///     other scripts.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LineWidthController : MonoBehaviour
    {
        LineRenderer m_LineRenderer;

        /// <summary>
        ///     The line width when there is no viewer scale or temporary width change
        /// </summary>
        public float DefaultWidthMultiplier { get; set; }

        /// <summary>
        ///     A multiplier factor that will be applied to the final line renderer width.
        ///     This can be changed to temporarily change the line separate from the viewer scale width multiplier.
        /// </summary>
        public float TemporaryWidthMultiplier { get; set; } = 1.0f;

        void LateUpdate()
        {
            var currentWidthMultiplier = DefaultWidthMultiplier * TemporaryWidthMultiplier;

            // Avoid setting width multiplier if possible because it will require writing a new width for every point in the line
            if (!Mathf.Approximately(currentWidthMultiplier, m_LineRenderer.widthMultiplier))
                m_LineRenderer.widthMultiplier = currentWidthMultiplier;
        }

        void OnEnable()
        {
            m_LineRenderer = GetComponent<LineRenderer>();
            DefaultWidthMultiplier = m_LineRenderer.widthMultiplier;
        }

        void OnDisable()
        {
            m_LineRenderer.widthMultiplier = DefaultWidthMultiplier;
        }
    }
}
