using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.DataStreaming
{
    public interface IDataStreamBound
    {
        Bounds GetBound();
        (Vector3 position, Vector3 eulerAngles) CalculateViewFitPosition(Bounds bounds, float pitch, float fillRatio, Camera cam);
        float GetDistanceVisibleFromCenter(Camera cam);
    }

    public class DataStreamBound : IDataStreamBound
    {
        const float k_BoundsFillRatio = 0.9f;
        Bounds m_TotalBound;
        IDataStreamer m_DataStreamer;

        [Inject]
        void Setup(ISceneEvents sceneEvents, IDataStreamerProvider dataStreamerProvider)
        {
            sceneEvents.SceneOpened += OnSceneOpened;
            sceneEvents.SceneClosed += OnSceneClosed;

            m_DataStreamer = dataStreamerProvider.DataStreamer;
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
            var volumeOfInterest = await m_DataStreamer.GetDefaultVolumeOfInterestAsync();
            m_TotalBound = volumeOfInterest.Bounds;
        }

        public Bounds GetBound()
        {
            return m_TotalBound;
        }

        public float GetDistanceFromCenter(Camera cam)
        {
            return CameraUtilities.GetDistanceFromCenterToFit(m_TotalBound, k_BoundsFillRatio, cam.fieldOfView, cam.fieldOfView);
        }

        public float GetDistanceVisibleFromCenter(Camera cam)
        {
            var distanceFromCenter = GetDistanceFromCenter(cam);
            
            if (distanceFromCenter > cam.farClipPlane)
            {
               return (cam.farClipPlane + cam.nearClipPlane)/2;
            }
            return distanceFromCenter;
        }
        
        public (Vector3 position, Vector3 eulerAngles) CalculateViewFitPosition(Bounds bounds, float pitch, float fillRatio, Camera cam)
        {
            return CameraUtilities.CalculateViewFitPosition(bounds, pitch, fillRatio, cam.fieldOfView, cam.aspect, cam.farClipPlane, cam.nearClipPlane);
        }
    }

    public static class CameraUtilities
    {
        public static (Vector3 position, Vector3 eulerAngles) CalculateViewFitPosition(Bounds bounds, float pitch, float fillRatio, float fieldOfView, float aspectRatio, float farClipPlane, float nearClipPlane)
        {
            var desiredEuler = new Vector3(pitch, 0, 0);
            var distanceFromCenter = GetDistanceFromCenterToFit(bounds, fillRatio, fieldOfView, aspectRatio);
            
            if (distanceFromCenter > farClipPlane)
            {
                distanceFromCenter = (farClipPlane + nearClipPlane) / 2 ;
            }
            
            var position = bounds.center - distanceFromCenter * (Quaternion.Euler(desiredEuler) * Vector3.forward);

            return (position, desiredEuler);
        }

        public static float GetDistanceFromCenterToFit(Bounds bb, float fillRatio, float fovY, float aspectRatio)
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

