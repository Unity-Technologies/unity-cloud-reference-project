using System;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;

namespace Unity.ReferenceProject.DataStreaming
{
    public interface IDataStreamController
    {
        void Open(IScene scene);
        void Close();
    }

    public interface ISceneEvents
    {
        public event Action<IScene> SceneOpened;
        public event Action SceneClosed;

        public bool IsSceneOpened { get; }
    }

    public interface IDataStreamerProvider
    {
        public IDataStreamer DataStreamer { get; }
    }

    public interface IDataStreamControllerWithObserver
    {
        void SetCameraObserver(ICameraObserver cameraObserver);
    }

    public class DataStreamerController : IDataStreamerProvider, IDataStreamController, IDataStreamControllerWithObserver, ISceneEvents
    {
        public IDataStreamer DataStreamer { get; } = new DataStreamer();
        public event Action<IScene> SceneOpened;
        public event Action SceneClosed;
        public bool IsSceneOpened { get; private set; }

        readonly ServiceHostConfiguration m_CloudConfiguration;
        readonly IServiceHttpClient m_ServiceHttpClient;

        ICameraObserver m_CameraObserver;

        public DataStreamerController(IServiceHttpClient serviceHttpClient, ServiceHostConfiguration cloudConfiguration)
        {
            m_ServiceHttpClient = serviceHttpClient;
            m_CloudConfiguration = cloudConfiguration;
        }

        public void Open(IScene scene)
        {
            if (m_CameraObserver == null)
                throw new InvalidOperationException($"A {nameof(ICameraObserver)} must be set before opening a {nameof(IScene)}");

            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (IsSceneOpened)
            {
                Close();
            }

            var builder = DataStreamerSettingsBuilder
                .CreateDefaultBuilder()
                .SetScene(scene, m_ServiceHttpClient, m_CloudConfiguration);

            DataStreamer.AddObserver(m_CameraObserver);
            DataStreamer.Open(builder.Build());

            IsSceneOpened = true;
            SceneOpened?.Invoke(scene);
        }

        public void Close()
        {
            if (IsSceneOpened)
            {
                DataStreamer.Close();
                DataStreamer.RemoveObserver(m_CameraObserver);
            }

            IsSceneOpened = false;
            SceneClosed?.Invoke();
        }

        public void SetCameraObserver(ICameraObserver cameraObserver)
        {
            m_CameraObserver = cameraObserver;
        }
    }
}
