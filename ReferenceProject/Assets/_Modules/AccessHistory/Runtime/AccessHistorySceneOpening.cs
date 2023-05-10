using System;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.AccessHistory
{
    public class AccessHistorySceneOpening : MonoBehaviour
    {
        [Inject]
        public void Setup(IAccessHistoryController accessHistoryController, ISceneEvents sceneEvents)
        {
            sceneEvents.SceneOpened += scene =>
            {
                if (scene != null)
                {
                    accessHistoryController.AddData(scene);
                }
            };
        }
    }
}
