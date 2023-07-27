using System;
using Unity.XR.CoreUtils.GUI;
using UnityEngine;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    /// Defines how to translate/rotate an axis of position or euler rotation
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Transform Axis Range.asset", menuName = "ReferenceProject/VR/Transform Axis Range")]
    public class TransformAxisRange : ScriptableObject
    {
        [FlagsProperty]
        [SerializeField, Tooltip("The axes on which to perform translation of a transform.")]
        Axis m_TranslateAxes;

        [FlagsProperty]
        [SerializeField, Tooltip("The axes on which to perform rotation of a transform.")]
        Axis m_RotateAxes;

        [SerializeField, Tooltip("The offset added to the axis when the input value is 0.")]
        float m_MinimumOutputValue;

        [SerializeField, Tooltip("The offset added to the axis when the input value is 1. If the input value is negative, the output will also be ")]
        float m_MaximumOutputValue;

        /// <summary>
        /// The axes on which to perform translation of a transform.
        /// </summary>
        public Axis TranslateAxes => m_TranslateAxes;

        /// <summary>
        /// The axes on which to perform rotation of a transform.
        /// </summary>
        public Axis RotateAxes => m_RotateAxes;

        /// <summary>
        /// The offset added to the axis when the input value is 0.
        /// </summary>
        public float MinimumOutputValue => m_MinimumOutputValue;

        /// <summary>
        /// The offset added to the axis when the input value is 1. If the input value is negative, the output value is subtracted instead of added.
        /// </summary>
        public float MaximumOutputValue => m_MaximumOutputValue;
    }
}