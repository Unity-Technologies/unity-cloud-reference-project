using System;
using Unity.Cloud.Assets;
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

        IAssetEvents m_AssetEvents;
        IStage m_Stage;

        [Inject]
        public void Setup(IAssetEvents sceneEvents, IDataStreamerProvider dataStreamerProvider)
        {
            m_AssetEvents = sceneEvents;
            m_AssetEvents.AssetLoaded += OnAssetLoaded;
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;

            dataStreamerProvider.DataStreamer.StageCreated.Subscribe(stage => m_Stage = stage);
            dataStreamerProvider.DataStreamer.StageDestroyed.Subscribe(() => m_Stage = null);
        }

        void OnDestroy()
        {
            m_AssetEvents.AssetLoaded -= OnAssetLoaded;
            m_AssetEvents.AssetUnloaded -= OnAssetUnloaded;

            OnAssetUnloaded();
        }

        void OnEnable()
        {
            SetIndicatorVisibility(m_AssetEvents.IsAssetLoaded);
        }

        void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            m_Stage.StreamingStateChanged.Subscribe(OnStreamingStateChanged);
            SetIndicatorVisibility(true);
        }

        void OnAssetUnloaded()
        {
            m_Stage?.StreamingStateChanged.Unsubscribe(OnStreamingStateChanged);
            SetIndicatorVisibility(false);
        }

        void OnStreamingStateChanged(StreamingState state)
        {
            SetIndicatorVisibility(state.IsStreamingInProgress);
        }

        void SetIndicatorVisibility(bool visible)
        {
            if (m_UIDocument == null || m_UIDocument.rootVisualElement == null)
                return;

            Utils.SetVisible(m_UIDocument.rootVisualElement, visible);
        }
    }
}
