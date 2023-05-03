using System;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    class CameraDefaultPositionController : MonoBehaviour
    {
        const int k_VolumeProcessingIterations = 50;
        [SerializeField]
        float m_PitchAngle = 20.0f;

        [SerializeField]
        float m_BoundsFillRatio = 0.9f;

        IDataStreamer m_DataStreamer;
        INavigationManager m_NavigationManager;
        Camera m_StreamingCamera;

        [Inject]
        void Setup(ISceneEvents sceneEvents, IDataStreamerProvider dataStreamerProvider, Camera streamingCamera, INavigationManager navigationManager)
        {
            sceneEvents.SceneOpened += OnSceneOpening;

            m_DataStreamer = dataStreamerProvider.DataStreamer;

            m_StreamingCamera = streamingCamera;
            m_NavigationManager = navigationManager;
        }

        void OnSceneOpening(IScene scene)
        {
            m_DataStreamer.StreamingStateChanged += OnStreamingStateChanged;
        }

        async void OnStreamingStateChanged(StreamingState state)
        {
            if (await ProcessDefaultVolumeOfInterestAsync())
                m_DataStreamer.StreamingStateChanged -= OnStreamingStateChanged;
        }

        async Task<bool> ProcessDefaultVolumeOfInterestAsync()
        {
            var result = false;
            VolumeOfInterest prevVolume = new();

            for (var i = 0; i < k_VolumeProcessingIterations; ++i)
            {
                var v = await m_DataStreamer.GetDefaultVolumeOfInterestAsync();
                if (v != prevVolume)
                {
                    prevVolume = v;

                    if (m_StreamingCamera == null) // Camera might have been destroyed since last loop.
                        return false;

                    SetView(v.Bounds, m_StreamingCamera);

                    await Task.Delay(100);
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        void SetView(Bounds bounds, Camera cam)
        {
            var (position, rotation) = CameraUtilities.CalculateViewFitPosition(bounds, m_PitchAngle, m_BoundsFillRatio,
                cam.fieldOfView, cam.aspect);

            m_NavigationManager.TryTeleport(position, rotation);
        }
    }

    public static class CameraUtilities
    {
        public static (Vector3 position, Vector3 eulerAngles) CalculateViewFitPosition(Bounds bounds, float pitch, float fillRatio, float fieldOfView, float aspectRatio)
        {
            var desiredEuler = new Vector3(pitch, 0, 0);
            var distanceFromCenter = GetDistanceFromCenterToFit(bounds, fillRatio, fieldOfView, aspectRatio);
            var position = bounds.center - distanceFromCenter * (Quaternion.Euler(desiredEuler) * Vector3.forward);

            return (position, desiredEuler);
        }

        static float GetDistanceFromCenterToFit(Bounds bb, float fillRatio, float fovY, float aspectRatio)
        {
            var fovX = GetHorizontalFov(fovY, aspectRatio);
            var distanceToFitXAxisInView = GetDistanceFromCenter(bb, bb.extents.x, fovX, fillRatio);
            var distanceToFitYAxisInView = GetDistanceFromCenter(bb, bb.extents.y, fovY, fillRatio);

            return Mathf.Max(distanceToFitXAxisInView, distanceToFitYAxisInView);
        }

        static float GetHorizontalFov(float fovY, float aspectRatio)
        {
            var ratio = Mathf.Tan(Mathf.Deg2Rad * (fovY / 2.0f));
            return Mathf.Rad2Deg * Mathf.Atan(ratio * aspectRatio) * 2.0f;
        }

        static float GetDistanceFromCenter(Bounds bb, float opposite, float fov, float fillRatio)
        {
            var lookAt = bb.center;

            var angle = fov / 2.0f;
            var ratio = Mathf.Tan(Mathf.Deg2Rad * angle);
            var adjacent = opposite / ratio;
            var distanceFromLookAt = lookAt.z - bb.min.z + adjacent / fillRatio;

            return distanceFromLookAt;
        }
    }
}
