using System;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.ReferenceProject.DeepLinking;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class VRModeController : NavigationMode
    {
        IDataStreamController m_DataStreamController;
        IDeepLinkingController m_DeepLinkingController;
        PropertyValue<IScene> m_ActiveScene;
        Uri m_Uri;
        DeepLinkData m_DeepLinkData;
        
        [Inject]
        void Setup(IDataStreamController dataStreamController, IDeepLinkingController deepLinkingController, PropertyValue<IScene> sceneListStore, DeepLinkData deepLinkData)
        {
            m_DataStreamController = dataStreamController;
            m_DeepLinkingController = deepLinkingController;
            m_ActiveScene = sceneListStore;
            m_DeepLinkData = deepLinkData;
        }

        void Awake()
        {
            m_DeepLinkData.EnableSwitchToDesktop = true;
        }

        async Task Start()
        {
            await LoadScene();
        }

        async Task LoadScene()
        {
            m_DataStreamController.Close();
            
            try
            {
                m_Uri = await m_DeepLinkingController.GenerateUri(m_ActiveScene.GetValue());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            SceneManager.sceneLoaded += OnSceneSwitched;
            SceneManager.LoadScene("MainVR");
        }

        async void OnSceneSwitched(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!await m_DeepLinkingController.TryConsumeUri(m_Uri.ToString()))
            {
                Console.WriteLine("Unable to Consume URI");
            }
            SceneManager.sceneLoaded -= OnSceneSwitched;
        }

        public override void Teleport(Vector3 position, Vector3 eulerAngles)
        {
            // Not supported
        }
    }
}
