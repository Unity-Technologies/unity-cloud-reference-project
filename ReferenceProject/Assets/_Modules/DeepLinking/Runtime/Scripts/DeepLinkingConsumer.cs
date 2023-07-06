using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.DeepLinking
{
    public class DeepLinkCameraInfo
    {
        public bool SetDeepLinkCamera { get; set; }
        public Action SetCameraReady { get; set; }
    }
    
    public class DeepLinkingConsumer : MonoBehaviour
    {
        [SerializeField] AppState m_NextState;

        [Header("Localization")]
        [SerializeField]
        string m_OpenLinkSuccessString = "@DeepLinking:OpenLinkSuccess";
        
        [SerializeField]
        string m_SceneAlreadyOpenedString = "@DeepLinking:SceneAlreadyOpen";
        
        IAppStateController m_AppStateController;
        PropertyValue<IScene> m_ActiveScene;
        IAppMessaging m_AppMessaging;

        [Inject]
        public void Setup(IDeepLinkingController deepLinkingController, IAppStateController appStateController, PropertyValue<IScene> sceneListStore, IAppMessaging appMessaging)
        {
            deepLinkingController.DeepLinkConsumed += OnDeepLinkConsumed;

            m_AppStateController = appStateController;
            m_ActiveScene = sceneListStore;
            m_AppMessaging = appMessaging;
        }

        void OnDeepLinkConsumed(IScene scene, bool hasNewSceneState)
        {
            var activeScene = m_ActiveScene.GetValue();

            var isOpeningNewScene = activeScene == null || !activeScene.Id.Equals(scene.Id);
            
            if (isOpeningNewScene || hasNewSceneState)
            {
                m_AppMessaging.ShowMessage(m_OpenLinkSuccessString, false, scene.Name);
                m_AppStateController.PrepareTransition(m_NextState).OnBeforeEnter(() => m_ActiveScene.SetValue(scene)).Apply();
            }
            else
            {
                m_AppMessaging.ShowWarning(m_SceneAlreadyOpenedString, true, scene.Name);
            }
        }
    }
}
