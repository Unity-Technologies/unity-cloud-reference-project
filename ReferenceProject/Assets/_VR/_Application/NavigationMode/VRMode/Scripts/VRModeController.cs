using System;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class VRModeController : NavigationMode
    {
        IDataStreamController m_DataStreamController;

        [Inject]
        void Setup(IDataStreamController dataStreamController)
        {
            m_DataStreamController = dataStreamController;
        }

        void Awake()
        {
            var switchVRFlag = FindObjectOfType<SwitchFlag>();
            if (switchVRFlag == null)
            {
                var go = new GameObject
                {
                    name = "SwitchVRFlag"
                };
                go.AddComponent<SwitchFlag>();
            }

            m_DataStreamController.Close();

            SceneManager.LoadScene("MainVR");
        }

        public override void Teleport(Vector3 position, Vector3 eulerAngles)
        {
            // Not supported
        }
    }
}
