using System;
using UnityEngine;

namespace Unity.ReferenceProject.AppCamera
{
    [CreateAssetMenu(fileName = nameof(UINavigationControllerSettings), menuName = "ReferenceProject/Camera/" + nameof(UINavigationControllerSettings))]
    public class UINavigationControllerSettings : ScriptableObject
    {
        [Header("Input Lag Skip Threshold")]
        [Tooltip("If an input event is older than the indicated value, one out of \"Input Lag Skip Amount\" events will be taken into considerations.  Others will be skipped.")]
        [SerializeField]
        float m_InputLagSkipThreshold = .25f;

        [Header("Input Lag Skip Amount")]
        [Tooltip("If the \"Input Lag Skip Threshold\" is reached, Only one out of this value event will be treated.")]
        [SerializeField]
        int m_InputLagSkipAmount = 3;

        [Header("Input Lag Cutoff Threshold")]
        [Tooltip("If an input event is older than the indicated value, it will remain untreated")]
        [SerializeField]
        float m_InputLagCutoffThreshold = 1.0f;
        
        public float InputLagSkipThreshold => m_InputLagSkipThreshold;
        public int InputLagSkipAmount => m_InputLagSkipAmount;
        public float InputLagCutoffThreshold => m_InputLagCutoffThreshold;
    }
}
