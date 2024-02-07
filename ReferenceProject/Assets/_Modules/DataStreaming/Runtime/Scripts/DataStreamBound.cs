using Unity.Cloud.Assets;
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
        IStage m_Stage;

        [Inject]
        void Setup(IAssetEvents assetEvents, IDataStreamerProvider dataStreamerProvider)
        {
            assetEvents.AssetLoaded += OnAssetLoad;
            assetEvents.AssetUnloaded += OnAssetUnload;

            dataStreamerProvider.DataStreamer.StageCreated.Subscribe(stage => m_Stage = stage);
            dataStreamerProvider.DataStreamer.StageDestroyed.Subscribe(() => m_Stage = null);
        }

        async void OnAssetLoad(IAsset asset, IDataset dataset)
        {
            m_TotalBound = (Bounds) await m_Stage.GetWorldBoundsAsync();
        }

        void OnAssetUnload()
        {
            m_TotalBound = default;
        }
        

        public Bounds GetBound()
        {
            return m_TotalBound;
        }

        public float GetDistanceFromCenter(Camera cam)
        {
            return CameraUtilities.GetDistanceFromCenterToFit(m_TotalBound, k_BoundsFillRatio, cam.fieldOfView, cam.aspect);
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

