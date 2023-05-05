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
        [SerializeField]
        float m_PitchAngle = 20.0f;

        [SerializeField]
        float m_BoundsFillRatio = 0.9f;
        
        [SerializeField]
        int m_VolumeProcessingIterations = 5;
        
        [SerializeField]
        int m_VolumeProcessingDelay = 50;

        IDataStreamer m_DataStreamer;
        INavigationManager m_NavigationManager;
        Camera m_StreamingCamera;

        ISceneEvents m_SceneEvents;

        bool m_IsProcessing;

        [Inject]
        void Setup(ISceneEvents sceneEvents, IDataStreamerProvider dataStreamerProvider, Camera streamingCamera, INavigationManager navigationManager)
        {
            m_SceneEvents = sceneEvents;
            m_SceneEvents.SceneOpened += OnSceneOpened;
            m_SceneEvents.SceneClosed += OnSceneClosed;

            m_DataStreamer = dataStreamerProvider.DataStreamer;

            m_StreamingCamera = streamingCamera;
            m_NavigationManager = navigationManager;
        }

        void OnDestroy()
        {
            m_SceneEvents.SceneOpened -= OnSceneOpened;
            m_SceneEvents.SceneClosed -= OnSceneClosed;
        }

        void OnSceneOpened(IScene scene)
        {
            m_DataStreamer.StreamingStateChanged += OnStreamingStateChanged;
        }
        
        void OnSceneClosed()
        {
            m_DataStreamer.StreamingStateChanged -= OnStreamingStateChanged;
        }

        async void OnStreamingStateChanged(StreamingState state)
        {
            if (m_IsProcessing)
                return;
            
            m_IsProcessing = true;
            
            if (await ProcessDefaultVolumeOfInterestAsync())
            {
                m_DataStreamer.StreamingStateChanged -= OnStreamingStateChanged;
            }
            
            m_IsProcessing = false;
        }

        async Task<bool> ProcessDefaultVolumeOfInterestAsync()
        {
            var result = false;
            VolumeOfInterest prevVolume = new();

            for (var i = 0; i < m_VolumeProcessingIterations; ++i)
            {
                var v = await m_DataStreamer.GetDefaultVolumeOfInterestAsync();
                if (v != prevVolume)
                {
                    prevVolume = v;

                    if (m_StreamingCamera == null) // Camera might have been destroyed since last loop.
                        return false;

                    SetView(v.Bounds, m_StreamingCamera);

                    await Task.Delay(m_VolumeProcessingDelay);
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
