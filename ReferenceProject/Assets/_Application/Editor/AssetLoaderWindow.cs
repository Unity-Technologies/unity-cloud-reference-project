#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Unity.ReferenceProject.Editor
{
    public class AssetLoaderWindow : EditorWindow, IStatusReceiver
    {
        const string k_MenuItem = "ReferenceProject/Tools/Auto Asset Loader";

        static readonly string k_AssetNameKey = "ReferenceProject:Editor:AssetLoad:AssetName";
        static readonly string k_EnabledKey = "ReferenceProject:Editor:AssetLoad:Enabled";

        string m_EnabledPerSceneKey;
        
        string m_Status;
        
        [SerializeField]
        bool m_IsEnabled;
        
        [SerializeField]
        string m_AssetName;

        [MenuItem(k_MenuItem)]
        public static void Enable()
        {
            var window = GetWindow<AssetLoaderWindow>();
            window.titleContent = new GUIContent("Auto Asset Loader");
            window.Show();
        }

        public void OnEnable()
        {
            OnSceneOpened(SceneManager.GetActiveScene(), OpenSceneMode.Single);
            m_IsEnabled = SavedEnabledState;
            m_AssetName = SavedAssetName;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }
        
        public void OnDisable()
        {
            SavedAssetName = m_AssetName;
            SavedEnabledState = m_IsEnabled;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }
        
        void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            m_EnabledPerSceneKey = $"{k_EnabledKey}:{scene.name}";
            m_IsEnabled = SavedEnabledState;
        }
        
        public void SetStatus(string status)
        {
            m_Status = status;
            Repaint();
        }

        void OnGUI()
        {
            m_IsEnabled = EditorGUILayout.Toggle("Enable For This Scene", m_IsEnabled);
            m_AssetName = EditorGUILayout.TextField("Asset Name", m_AssetName);

            if (GUI.changed)
            {
                SavedAssetName = m_AssetName;
                SavedEnabledState = m_IsEnabled;
            }

            EditorGUILayout.LabelField("Status : " + (m_IsEnabled ? m_Status : "Disabled"));
        }
        
        bool SavedEnabledState
        {
            get => EditorPrefs.GetBool(m_EnabledPerSceneKey, false);
            set => EditorPrefs.SetBool(m_EnabledPerSceneKey, value);
        }
        
        static string SavedAssetName
        {
            get => EditorPrefs.GetString(k_AssetNameKey, "");
            set => EditorPrefs.SetString(k_AssetNameKey, value);
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!m_IsEnabled)
            {
                return;
            }
            
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var sceneContext = FindObjectOfType<SceneContext>();
                AssetLoader.StatusReceiver = this;
                var sceneLoader = sceneContext.Container.InstantiateComponent<AssetLoader>(new GameObject(nameof(AssetLoader)));
                sceneLoader.AssetName = m_AssetName;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                m_Status = string.Empty;
            }
        }
    }
}
#endif