using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject
{
    public class SceneNameDisplay : MonoBehaviour
    {
        static readonly string k_NoScene = "<none>";
        [SerializeField]
        UIDocument m_SourceUIDocument;

        [Header("UXML")]
        [SerializeField]
        string m_HeaderElement = "header";

        Heading m_Header;

        ISceneEvents m_SceneEvents;

        [Inject]
        void Setup(ISceneEvents sceneEvents)
        {
            m_SceneEvents = sceneEvents;
        }

        void Awake()
        {
            m_Header = m_SourceUIDocument.rootVisualElement.Q<Heading>(m_HeaderElement);
        }

        void OnEnable()
        {
            m_SceneEvents.SceneOpened += OnSceneOpened;
            m_SceneEvents.SceneClosed += OnSceneClosed;
        }

        void OnDisable()
        {
            m_SceneEvents.SceneOpened -= OnSceneOpened;
            m_SceneEvents.SceneClosed -= OnSceneClosed;
        }

        void OnSceneOpened(IScene scene)
        {
            SetTitle(scene);
        }

        void OnSceneClosed()
        {
            SetTitle(null);
        }

        void SetTitle(IScene scene)
        {
            m_Header.text = scene != null ? scene.Name : k_NoScene;
        }
    }
}
