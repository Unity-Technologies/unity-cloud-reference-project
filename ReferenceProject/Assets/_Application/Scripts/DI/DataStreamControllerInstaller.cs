using System;
using Unity.ReferenceProject.DataStreaming;
using Zenject;

namespace Unity.ReferenceProject
{
    public class DataStreamControllerInstaller : MonoInstaller
    {
        IDataStreamController m_DataStreamController;

        void Awake()
        {
            m_DataStreamController = Container.Resolve<IDataStreamController>();
        }

        void OnDestroy()
        {
            m_DataStreamController.Close();
        }

        public override void InstallBindings()
        {
            Container.Bind(typeof(IDataStreamerProvider), typeof(IDataStreamController),
                typeof(IDataStreamControllerWithObserver), typeof(ISceneEvents)).To<DataStreamerController>().AsSingle();
            Container.Bind<IDataStreamBound>().To<DataStreamBound>().AsSingle();
        }
    }
}
