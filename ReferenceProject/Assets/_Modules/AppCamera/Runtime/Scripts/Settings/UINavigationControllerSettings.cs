using System;
using UnityEngine;

namespace Unity.ReferenceProject.AppCamera
{
    [CreateAssetMenu(fileName = nameof(UINavigationControllerSettings), menuName = "ScriptableObjects/" + nameof(UINavigationControllerSettings))]
    public class UINavigationControllerSettings : ScriptableObject
    {
        [Header("Input Lag Skip Threshold")]
        [Tooltip("If an input event is older than the indicated value, one out of \"Input Lag Skip Amount\" events will be taken into considerations.  Others will be skipped.")]
        public float inputLagSkipThreshold = .25f;

        [Header("Input Lag Skip Amount")]
        [Tooltip("If the \"Input Lag Skip Threshold\" is reached, Only one out of this value event will be treated.")]
        public int inputLagSkipAmount = 3;

        [Header("Input Lag Cutoff Threshold")]
        [Tooltip("If an input event is older than the indicated value, it will remain untreated")]
        public float inputLagCutoffThreshold = 1.0f;
    }
}
