using System;
using Unity.ReferenceProject.DataStreaming;
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

        [Inject]
        public void Setup(IDataStreamController dataStreamController, IAppMessaging appMessaging, IToolUIManager toolUIManager)
        {
            m_DataStreamController = dataStreamController;
            m_AppMessaging = appMessaging;
            m_ToolUIManager = toolUIManager;
        }

        protected override void Awake()
        {
            base.Awake();

            var switchFlag = FindObjectOfType<SwitchFlag>();
            if (switchFlag == null)
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

        void OnReturnToDesktop()
        {
            m_DataStreamController.Close();

            SceneManager.LoadScene("Main");
        }
    }
}
