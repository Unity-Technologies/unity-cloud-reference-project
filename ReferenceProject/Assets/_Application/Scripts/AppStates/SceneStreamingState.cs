using System;
using Unity.Cloud.Assets;
using Unity.ReferenceProject.AssetManager;
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

        PropertyValue<IAsset> m_SelectedAsset;

        StreamingSceneLoader m_StreamingSceneLoader;

        [Inject]
        void Setup(IDataStreamController dataStreamController, AssetManagerStore assetManagerStore)
        {
            m_DataStreamController = dataStreamController;

            m_SelectedAsset = assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset));
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
            var asset = m_SelectedAsset.GetValue();
            m_DataStreamController.Load(asset);
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
            m_SelectedAsset.SetValue((IAsset)null);
            m_DataStreamController.Unload();

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
