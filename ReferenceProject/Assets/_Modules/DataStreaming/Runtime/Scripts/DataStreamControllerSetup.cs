using System;
using Unity.Cloud.DataStreaming.Runtime;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.DataStreaming
{
    public class DataStreamControllerSetup : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The maximum screen space error used by the observer allowing to specify if the object should be displayed." +
            "This value is used to calculate the error multiplier." +
            "Higher value will result in loading less geometry to be loaded since it would require a smaller Geometric Error;" +
            "A lower value will result in displaying more geometry.")]
        float m_ScreenSpaceError = SceneObserverFactory.DefaultScreenSpaceError;

        [Inject]
        void Setup(IDataStreamControllerWithObserver dataStreamController, Camera streamingCamera)
        {
            dataStreamController.SetCameraObserver(SceneObserverFactory.CreateCameraObserver(streamingCamera, m_ScreenSpaceError));
        }
    }
}
