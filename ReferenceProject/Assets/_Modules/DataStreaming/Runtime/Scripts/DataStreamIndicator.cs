using System;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.DataStreaming
{
    public class DataStreamIndicator : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        ISceneEvents m_SceneEvents;
        IDataStreamer m_DataStreamer;

        [Inject]
        public void Setup(ISceneEvents sceneEvents, IDataStreamerProvider dataStreamerProvider)
        {
            m_SceneEvents = sceneEvents;
            m_SceneEvents.SceneOpened += OnSceneOpened;
            m_SceneEvents.SceneClosed += OnSceneClosed;
            
            m_DataStreamer = dataStreamerProvider.DataStreamer;
        }
        
        void OnDestroy()
        {
            m_SceneEvents.SceneOpened -= OnSceneOpened;
            m_SceneEvents.SceneClosed -= OnSceneClosed;

            OnSceneClosed();
        }

        void OnEnable()
        {
            SetIndicatorVisibility(m_SceneEvents.IsSceneOpened);
        }

        void OnSceneOpened(IScene obj)
        {
            m_DataStreamer.StreamingStateChanged += OnStreamingStateChanged;
            SetIndicatorVisibility(true);
        }
        
        void OnSceneClosed()
        {
            m_DataStreamer.StreamingStateChanged -= OnStreamingStateChanged;
            SetIndicatorVisibility(false);
        }

        void OnStreamingStateChanged(StreamingState state)
        {
            SetIndicatorVisibility(state.IsStreamingInProgress);
        }

        void SetIndicatorVisibility(bool visible)
        {
            Utils.SetVisible(m_UIDocument.rootVisualElement, visible);
        }
    }
}
