using System;
using UnityEngine;

namespace Unity.ReferenceProject.AppCamera
{
    [CreateAssetMenu(fileName = nameof(CameraProxySettings), menuName = "ReferenceProject/Camera/" + nameof(CameraProxySettings))]
    public class CameraProxySettings : ScriptableObject
    {
        [Header("Constraints")]
        [Tooltip("The distance at which the look at point will start to move with the camera when zooming")]
        [SerializeField]
        float m_MinDistanceFromLookAt = 3.0f;

        [Tooltip("The maximum angle in degree on the pitch axis (looking up/down)")]
        [SerializeField]
        float m_MaxPitchAngle = 85.0f;

        [Header("Camera Elasticity")]
        [Range(0.001f, 1.0f)]
        [Tooltip("The linear interpolation factor in second between where the camera is and where it should be")]
        [SerializeField]
        float m_PositionElasticity = 0.05f;

        [Range(0.001f, 1.0f)]
        [Tooltip("The linear interpolation factor in second between where the camera is looking at and where it should be looking at.")]
        [SerializeField]
        float m_RotationElasticity = 0.02f;

        [Range(0.001f, 1.0f)]
        [Tooltip("The linear interpolation factor in second between where the current camera scale and the destination camera scale.")]
        [SerializeField]
        float m_ScalingElasticity = 0.02f;

        [Tooltip("Linear scaling over default 'zoom' movement speed")]
        [SerializeField]
        float m_MoveOnAxisScaling = 0.05f;

        public float MinDistanceFromLookAt => m_MinDistanceFromLookAt;
        public float MaxPitchAngle => m_MaxPitchAngle;
        public float PositionElasticity => m_PositionElasticity;
        public float RotationElasticity => m_RotationElasticity;
        public float ScalingElasticity => m_ScalingElasticity;
        public float MoveOnAxisScaling => m_MoveOnAxisScaling;
    }
}
