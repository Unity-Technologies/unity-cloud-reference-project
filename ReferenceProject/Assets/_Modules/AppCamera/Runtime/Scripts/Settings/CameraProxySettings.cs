using System;
using UnityEngine;

namespace Unity.ReferenceProject.AppCamera
{
    [CreateAssetMenu(fileName = nameof(CameraProxySettings), menuName = "ScriptableObjects/" + nameof(CameraProxySettings))]
    public class CameraProxySettings : ScriptableObject
    {
        [Header("Movement Speed")]
        [Tooltip("The maximum time in seconds to travel the entire scene when the camera is at the minimum speed")]
        public float maxTimeToTravelMinSpeed = 30.0f;

        [Tooltip("The maximum time in seconds to travel the entire scene when the camera is at the maximum speed")]
        public float maxTimeToTravelFullSpeed = 3.0f;

        [Tooltip("The maximum time in seconds for the camera to accelerate from minimum to maximum speed")]
        public float maxTimeToAccelerate = 5.0f;

        [Tooltip("Scaling on camera minimum speed")]
        public float minSpeedScaling = 1.0f;

        [Tooltip("Scaling on camera maximum speed")]
        public float maxSpeedScaling = 1.0f;

        [Tooltip("Scaling on camera acceleration")]
        public float accelerationScaling = 1.0f;

        [Tooltip("Higher value means the camera will go back faster to minimum speed when not moving")]
        public float waitingDecelerationScaling = 4.0f;

        [Header("Constraints")]
        [Tooltip("The distance at which the look at point will start to move with the camera when zooming")]
        public float minDistanceFromLookAt = 3.0f;

        [Tooltip("The maximum angle in degree on the pitch axis (looking up/down)")]
        public float maxPitchAngle = 85.0f;

        [Header("Camera Elasticity")]
        [Range(0.001f, 1.0f)]
        [Tooltip("The linear interpolation factor in second between where the camera is and where it should be")]
        public float positionElasticity = 0.05f;

        [Range(0.001f, 1.0f)]
        [Tooltip("The linear interpolation factor in second between where the camera is looking at and where it should be looking at.")]
        public float rotationElasticity = 0.02f;

        [Range(0.001f, 1.0f)]
        [Tooltip("The linear interpolation factor in second between where the current camera scale and the destination camera scale.")]
        public float scalingElasticity = 0.02f;

        [Header("Others")]
        [Tooltip("Linear scaling over default pan (camera drag) movement speed")]
        public float panScaling = 0.15f;

        [Tooltip("Linear scaling over default 'zoom' movement speed")]
        public float moveOnAxisScaling = 0.05f;

        [Tooltip("The maximum distance at which the camera can go from the scene")]
        public float maxLookAtDistanceScaling = 2.0f;
    }
}
