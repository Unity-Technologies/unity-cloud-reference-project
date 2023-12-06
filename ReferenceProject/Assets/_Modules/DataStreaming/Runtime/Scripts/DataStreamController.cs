using System;
using System.Threading;
using System.Threading.Tasks;
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
        public IDataStreamer DataStreamer { get; } = IDataStreamer.Create();
        public event Action<IAsset, IDataset> AssetLoaded;
        public event Action AssetUnloaded;

        public bool IsAssetLoaded { get; private set; }
        
        readonly IServiceHttpClient m_ServiceHttpClient;

        ICameraObserver m_CameraObserver;

        public DataStreamerController(IServiceHttpClient serviceHttpClient)
        {
            m_ServiceHttpClient = serviceHttpClient;
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

            var builder = DataStreamerSettingsBuilder
                .CreateDefaultBuilder()
                .SetModel(dataset, m_ServiceHttpClient);
            
            // WebGL and iOS are crashing around the 2-3 Gb mark, 
            // this is generally plenty for most needs at the time
#if UNITY_WEBGL || UNITY_IOS
            builder.ConfigureDefaultResourceLimiter(x => x
                .SetMaxVertexCount(50000 * 200)
                .SetMaxTexelCount(1024 * 1024 * 200));
#endif

            DataStreamer.StageCreated.Subscribe(OnStageCreated);
            DataStreamer.Open(builder.Build());

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
            stage.Observers.Add(m_CameraObserver);
        }

        public void SetCameraObserver(ICameraObserver cameraObserver)
        {
            m_CameraObserver = cameraObserver;
        }
    }
}
