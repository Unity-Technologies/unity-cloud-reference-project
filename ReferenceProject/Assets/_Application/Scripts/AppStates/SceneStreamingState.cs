using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.AccessHistory;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Unity.ReferenceProject
{
    [Serializable]
    public sealed class SceneStreamingState : AppState
    {
        [SerializeField]
        string m_DataStreamingSceneName = "Streaming";

        IDataStreamController m_DataStreamController;

        bool m_IsUnloadingScene;
        bool m_IsWaitingToLoadScene;

        PropertyValue<IScene> m_SelectedScene;

        StreamingSceneLoader m_StreamingSceneLoader;

        [Inject]
        void Setup(IDataStreamController dataStreamController, PropertyValue<IScene> selectedScene)
        {
            m_SelectedScene = selectedScene;
            m_DataStreamController = dataStreamController;
        }

        void Awake()
        {
            m_StreamingSceneLoader = new StreamingSceneLoader(m_DataStreamingSceneName);

            m_StreamingSceneLoader.Loaded += OnSceneLoaded;
            m_StreamingSceneLoader.Unloaded += OnSceneUnloaded;
        }

        void OnDestroy()
        {
            m_StreamingSceneLoader.Loaded -= OnSceneLoaded;
            m_StreamingSceneLoader.Unloaded -= OnSceneUnloaded;

            m_StreamingSceneLoader.Dispose();
        }

        void OnSceneLoaded()
        {
            var scene = m_SelectedScene.GetValue();
            m_DataStreamController.Open(scene);
        }

        void OnSceneUnloaded()
        {
            m_IsUnloadingScene = false;

            if (m_IsWaitingToLoadScene)
            {
                m_IsWaitingToLoadScene = false;
                EnterStateInternal();
            }
        }

        protected override void EnterStateInternal()
        {
            if (m_IsUnloadingScene)
            {
                m_IsWaitingToLoadScene = true;
            }
            else
            {
                m_StreamingSceneLoader.LoadScene();
            }
        }

        protected override void ExitStateInternal()
        {
            m_SelectedScene.SetValue((IScene)null);
            m_DataStreamController.Close();

            m_IsUnloadingScene = true;
            m_StreamingSceneLoader.UnloadScene();
        }
    }

    sealed class StreamingSceneLoader : IDisposable
    {
        readonly bool m_Skip;

        readonly string m_UnitySceneName;

        public StreamingSceneLoader(string unitySceneName)
        {
            m_UnitySceneName = unitySceneName;
            m_Skip = string.IsNullOrEmpty(m_UnitySceneName);

            if (!m_Skip)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
        }

        public void Dispose()
        {
            if (!m_Skip)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
            }
        }

        public event Action Loaded;
        public event Action Unloaded;

        public void LoadScene()
        {
            if (m_Skip)
            {
                Loaded?.Invoke();
            }
            else
            {
                SceneManager.LoadScene(m_UnitySceneName, LoadSceneMode.Additive);
            }
        }

        public void UnloadScene()
        {
            if (m_Skip)
            {
                Unloaded?.Invoke();
            }
            else
            {
                SceneManager.UnloadSceneAsync(m_UnitySceneName);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != m_UnitySceneName)
                return;

            Loaded?.Invoke();
        }

        void OnSceneUnloaded(Scene scene)
        {
            if (scene.name != m_UnitySceneName)
                return;

            Unloaded?.Invoke();
        }
    }
}
