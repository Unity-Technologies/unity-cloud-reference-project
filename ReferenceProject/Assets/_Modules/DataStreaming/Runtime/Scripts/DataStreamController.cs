using System;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.Cloud.DataStreaming.Runtime.AssetManager;
using UnityEngine;

namespace Unity.ReferenceProject.DataStreaming
{
    public interface IDataStreamController
    {
        void Load(IAsset asset);
        void Unload();
    }

    public interface IAssetEvents
    {
        public event Action<IAsset, IDataset> AssetLoaded;
        public event Action AssetUnloaded;

        public bool IsAssetLoaded { get; }
    }

    public interface IDataStreamerProvider
    {
        public IDataStreamer DataStreamer { get; }
    }

    public interface IDataStreamControllerWithObserver
    {
        void SetCameraObserver(ICameraObserver cameraObserver);
    }

    public class DataStreamerController : IDataStreamerProvider, IDataStreamController, IDataStreamControllerWithObserver, IAssetEvents
    {
        public IDataStreamer DataStreamer { get; }
        public event Action<IAsset, IDataset> AssetLoaded;
        public event Action AssetUnloaded;

        public bool IsAssetLoaded { get; private set; }
        
        readonly IServiceHttpClient m_ServiceHttpClient;

        ICameraObserver m_CameraObserver;

        IStage m_Stage;

        readonly DataStreamerSettings m_DataStreamerSettings;

        public DataStreamerController(IServiceHttpClient serviceHttpClient)
        {
            m_ServiceHttpClient = serviceHttpClient;
            m_DataStreamerSettings = CreateDataStreamSettings();

            DataStreamer = IDataStreamer.Create();
            DataStreamer.StageCreated.Subscribe(OnStageCreated);
        }

        static DataStreamerSettings CreateDataStreamSettings()
        {
            var builder = DataStreamerSettingsBuilder
                .CreateDefaultBuilder();
            
            // WebGL and iOS are crashing around the 2-3 Gb mark, 
            // this is generally plenty for most needs at the time
#if UNITY_WEBGL || UNITY_IOS
            builder.ConfigureDefaultResourceLimiter(x => x
                .SetMaxVertexCount(50000 * 200)
                .SetMaxTexelCount(1024 * 1024 * 200));
#endif

            return builder.Build();
        }

        public async void Load(IAsset asset)
        {
            if (m_CameraObserver == null)
                throw new InvalidOperationException($"A {nameof(ICameraObserver)} must be set before opening a asset.");

            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (IsAssetLoaded)
            {
                Unload();
            }
            
            var dataset = await StreamableAssetHelper.FindStreamableDataset(asset);


            var stage = DataStreamer.Open(m_DataStreamerSettings);
            stage.Models.Add((config) => config.FromDataset(dataset, m_ServiceHttpClient));

            IsAssetLoaded = true;
            AssetLoaded?.Invoke(asset, dataset);
        }

        public void Unload()
        {
            if (IsAssetLoaded)
            {
                DataStreamer.Close();
            }

            IsAssetLoaded = false;
            AssetUnloaded?.Invoke();
        }
        
        void OnStageCreated(IStage stage)
        {
            if (m_Stage != null && m_CameraObserver != null)
            {
                RemoveStageObserver(m_Stage, m_CameraObserver);
            }
            
            m_Stage = stage;
            AddStageObserver(m_Stage, m_CameraObserver);
        }

        public void SetCameraObserver(ICameraObserver cameraObserver)
        {
            RemoveStageObserver(m_Stage, m_CameraObserver);
            AddStageObserver(m_Stage, cameraObserver);
            
            m_CameraObserver = cameraObserver;
        }
        
        static void RemoveStageObserver(IStage stage, IStageObserver stageObserver)
        {
            if(stage == null || stageObserver == null)
                return;
            
            stage.Observers.Remove(stageObserver);
        }
        
        static void AddStageObserver(IStage stage, IStageObserver stageObserver)
        {
            if(stage == null || stageObserver == null)
                return;
            
            stage.Observers.Add(stageObserver);
        }
    }
}
