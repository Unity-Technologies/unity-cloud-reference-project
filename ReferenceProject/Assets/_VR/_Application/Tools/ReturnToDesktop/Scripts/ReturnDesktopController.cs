using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.DeepLinking;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class ReturnDesktopController : ToolUIController
    {
        [Header("Localization")]
        [SerializeField]
        string m_ConfirmationTitle = "@ReferenceProject:Navigation_ReturnDesktop_Title";

        [SerializeField]
        string m_ConfirmationMessage = "@ReferenceProject:Navigation_ReturnDesktop_Message";

        IDataStreamController m_DataStreamController;
        IAppMessaging m_AppMessaging;
        IToolUIManager m_ToolUIManager;
        
        IDeepLinkingController m_DeepLinkingController;
        PropertyValue<IScene> m_ActiveScene;
        Uri m_Uri;
        DeepLinkData m_DeepLinkData;

        [Inject]
        public void Setup(IDataStreamController dataStreamController, IAppMessaging appMessaging, IToolUIManager toolUIManager, 
            IDeepLinkingController deepLinkingController, PropertyValue<IScene> sceneListStore, DeepLinkData deepLinkData)
        {
            m_DataStreamController = dataStreamController;
            m_AppMessaging = appMessaging;
            m_ToolUIManager = toolUIManager;
            m_DeepLinkingController = deepLinkingController;
            m_ActiveScene = sceneListStore;
            m_DeepLinkData = deepLinkData;
        }

        protected override void Awake()
        {
            base.Awake();

            if (!m_DeepLinkData.EnableSwitchToDesktop) 
            {
                gameObject.SetActive(false);
            }
        }

        public override void OnToolOpened()
        {
            m_AppMessaging.ShowDialog(m_ConfirmationTitle,
                m_ConfirmationMessage,
                "@ReferenceProject:No",
                () => m_ToolUIManager.CloseAllTools(),
                "@ReferenceProject:Yes",
                OnReturnToDesktop,
                null);
        }

        async void OnReturnToDesktop()
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
            SceneManager.LoadScene("Main");
        }
        
        async void OnSceneSwitched(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!await m_DeepLinkingController.TryConsumeUri(m_Uri.ToString()))
            {
                Console.WriteLine("Unable to Consume URI");
            }

            SceneManager.sceneLoaded -= OnSceneSwitched;
        }
    }
}
