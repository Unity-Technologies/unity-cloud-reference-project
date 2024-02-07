using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.Cloud.DeepLinking;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.DeepLinking;
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
        
        IStage m_Stage;
        INavigationManager m_NavigationManager;
        ICameraProvider m_CameraProvider;

        IAssetEvents m_AssetEvents;
        IDataStreamBound m_DataStreamBound;

        bool m_IsProcessing;

        IQueryArgumentsProcessor m_QueryArgumentsProcessor;
        QueryArgumentHandler<string> m_QueryArgumentCameraTransformHandler;
        DeepLinkData m_DeepLinkData;

        [Inject]
        void Setup(IAssetEvents assetEvents, IDataStreamerProvider dataStreamerProvider, ICameraProvider cameraProvider, INavigationManager navigationManager, IDataStreamBound dataStreamBound,
            IQueryArgumentsProcessor queryArgumentsProcessor, DeepLinkData deepLinkData)
        {
            m_AssetEvents = assetEvents;
            m_AssetEvents.AssetLoaded += OnAssetLoaded;
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;

            dataStreamerProvider.DataStreamer.StageCreated.Subscribe(stage => m_Stage = stage);
            dataStreamerProvider.DataStreamer.StageDestroyed.Subscribe(() => m_Stage = null);

            m_CameraProvider = cameraProvider;
            m_NavigationManager = navigationManager;
            m_DataStreamBound = dataStreamBound;
            
            m_QueryArgumentsProcessor = queryArgumentsProcessor;
            m_DeepLinkData = deepLinkData;
        }

        void Awake()
        {
            m_QueryArgumentCameraTransformHandler = new QueryArgumentHandler<string>(
                "Camera Position",
                GetCameraPosition,
                SetCameraPosition
            );
            
            m_QueryArgumentsProcessor.Register(m_QueryArgumentCameraTransformHandler, DeepLinkResourceType.Dataset);
        }

        void OnDestroy()
        {
            m_AssetEvents.AssetLoaded -= OnAssetLoaded;
            m_AssetEvents.AssetUnloaded -= OnAssetUnloaded;
            m_QueryArgumentsProcessor.Unregister(m_QueryArgumentCameraTransformHandler, DeepLinkResourceType.Dataset);
        }
        
        static string Vector3UrlFormat(Vector3 v)
        {
            return $"{v.x},{v.y},{v.z}";
        }
        public string GetCameraPosition()
        {
            var cameraTransform = m_CameraProvider.Camera.transform;
            return $"{Vector3UrlFormat(cameraTransform.position)}, {Vector3UrlFormat(cameraTransform.rotation.eulerAngles)}";
        }
        
        void SetCameraPosition(string cameraString)
        {
            var splitValue = cameraString.Split(',').Select(i => float.TryParse(i, out float result) ? result : 0.0f).ToList();
            if (splitValue.Count > 5)
            {
                var newPosition = new Vector3(splitValue[0], splitValue[1], splitValue[2]);
                var newEulerAngle = new Vector3(splitValue[3], splitValue[4], splitValue[5]);
                m_NavigationManager.TryTeleport(newPosition, newEulerAngle);
            }
        }

        void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            m_Stage.StreamingStateChanged.Subscribe(OnStreamingStateChanged);
        }
        
        void OnAssetUnloaded()
        {
            m_Stage?.StreamingStateChanged.Unsubscribe(OnStreamingStateChanged);
        }

        async void OnStreamingStateChanged(StreamingState state)
        {
            if (m_IsProcessing)
                return;

            m_IsProcessing = true;
            
            if (m_DeepLinkData.SetDeepLinkCamera) 
            {
                m_DeepLinkData.SetCameraReady?.Invoke(); // Process Deeplink QueryArguments
                m_Stage.StreamingStateChanged.Unsubscribe(OnStreamingStateChanged);
                m_IsProcessing = false;
                return; 
            }
            
            if (await ProcessDefaultVolumeOfInterestAsync())
            {
                m_Stage.StreamingStateChanged.Unsubscribe(OnStreamingStateChanged);
            }
            
            m_IsProcessing = false;
        }

        async Task<bool> ProcessDefaultVolumeOfInterestAsync()
        {
            var doubleBounds = await m_Stage.GetWorldBoundsAsync();
            
            if (m_CameraProvider.Camera == null) // Camera might have been destroyed since last loop.
                return false;

            var c = doubleBounds.Center;
            var s = doubleBounds.Size;
            
            var bounds = new Bounds(new Vector3((float)c.x, (float)c.y, (float)c.z), new Vector3((float)s.x, (float)s.y, (float)s.z));
            SetView(bounds, m_CameraProvider.Camera);

            return true;
        }

        void SetView(Bounds bounds, Camera cam)
        {
             var (position, rotation) = m_DataStreamBound.CalculateViewFitPosition(bounds, m_PitchAngle,
                 m_BoundsFillRatio, cam);
             m_NavigationManager.SetDefaultPosition(position, rotation);
             m_NavigationManager.TryTeleport(position, rotation);
        }
    }
}
