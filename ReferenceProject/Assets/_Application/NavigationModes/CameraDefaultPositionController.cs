using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.Cloud.DeepLinking;
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

        IDataStreamer m_DataStreamer;
        INavigationManager m_NavigationManager;
        Camera m_StreamingCamera;

        ISceneEvents m_SceneEvents;
        IDataStreamBound m_DataStreamBound;

        bool m_IsProcessing;

        IQueryArgumentsProcessor m_QueryArgumentsProcessor;
        QueryArgumentHandler<string> m_QueryArgumentCameraTransformHandler;
        DeepLinkCameraInfo m_SetDeepLinkCamera;

        [Inject]
        void Setup(ISceneEvents sceneEvents, IDataStreamerProvider dataStreamerProvider, Camera streamingCamera, INavigationManager navigationManager, IDataStreamBound dataStreamBound,
            IQueryArgumentsProcessor queryArgumentsProcessor, DeepLinkCameraInfo deepLinkCameraInfo)
        {
            m_SceneEvents = sceneEvents;
            m_SceneEvents.SceneOpened += OnSceneOpened;
            m_SceneEvents.SceneClosed += OnSceneClosed;

            m_DataStreamer = dataStreamerProvider.DataStreamer;

            m_StreamingCamera = streamingCamera;
            m_NavigationManager = navigationManager;
            m_DataStreamBound = dataStreamBound;
            
            m_QueryArgumentsProcessor = queryArgumentsProcessor;
            m_SetDeepLinkCamera = deepLinkCameraInfo;
        }

        void Awake()
        {
            m_QueryArgumentCameraTransformHandler = new QueryArgumentHandler<string>(
                "Camera Position",
                GetCameraPosition,
                SetCameraPosition
            );
            
            m_QueryArgumentsProcessor.Register(m_QueryArgumentCameraTransformHandler, DeepLinkResourceType.Scene);
        }

        void OnDestroy()
        {
            m_SceneEvents.SceneOpened -= OnSceneOpened;
            m_SceneEvents.SceneClosed -= OnSceneClosed;
            m_QueryArgumentsProcessor.Unregister(m_QueryArgumentCameraTransformHandler, DeepLinkResourceType.Scene);
        }
        
        static string Vector3UrlFormat(Vector3 v)
        {
            return $"{v.x},{v.y},{v.z}";
        }
        public string GetCameraPosition()
        {
            return $"{Vector3UrlFormat(m_StreamingCamera.transform.position)}, {Vector3UrlFormat(m_StreamingCamera.transform.rotation.eulerAngles)}";
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
            
            if (m_SetDeepLinkCamera.SetDeepLinkCamera) 
            {
                m_SetDeepLinkCamera.SetCameraReady?.Invoke(); // Process Deeplink QueryArguments
                m_DataStreamer.StreamingStateChanged -= OnStreamingStateChanged;
                m_IsProcessing = false;
                return; 
            }
            
            if (await ProcessDefaultVolumeOfInterestAsync())
            {
                m_DataStreamer.StreamingStateChanged -= OnStreamingStateChanged;
            }
            
            m_IsProcessing = false;
        }

        async Task<bool> ProcessDefaultVolumeOfInterestAsync()
        {
            var v = await m_DataStreamer.GetDefaultVolumeOfInterestAsync();
            
            if (m_StreamingCamera == null) // Camera might have been destroyed since last loop.
                return false;

            SetView(v.Bounds, m_StreamingCamera);

            return true;
        }

        void SetView(Bounds bounds, Camera cam)
        {
             var (position, rotation) = m_DataStreamBound.CalculateViewFitPosition(bounds, m_PitchAngle,
                 m_BoundsFillRatio, cam);
            m_NavigationManager.TryTeleport(position, rotation);
        }
    }
}
