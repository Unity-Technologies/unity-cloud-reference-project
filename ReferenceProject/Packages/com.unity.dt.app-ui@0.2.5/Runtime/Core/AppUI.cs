using System;
using UnityEngine.Dt.App.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// A dummy object used to store some data in the editor.
    /// </summary>
    class AppUISystemObject : ScriptableObject
    {
        public string settings;
    }

    /// <summary>
    /// The main entry point for the App UI system.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class AppUI
    {
        static AppUISystemObject s_SystemObject;

        internal static AppUIManager s_Manager;

        /// <summary>
        /// Initialize the App UI system.
        /// </summary>
        static AppUI()
        {
#if UNITY_EDITOR
            InitializeInEditor();
#else
            InitializeInPlayer();
#endif
        }

        /// <summary>
        /// Register a Panel with the App UI system.
        /// </summary>
        /// <param name="panel">A panel</param>
        /// <exception cref="InvalidOperationException">Thrown if the App UI system is not ready.</exception>
        internal static void RegisterPanel(Panel panel)
        {
            if (s_Manager == null)
                throw new InvalidOperationException("The App UI Manager is not ready");

            s_Manager.RegisterPanel(panel);
        }

        /// <summary>
        /// Unregister a Panel with the App UI system.
        /// </summary>
        /// <param name="panel">A panel</param>
        /// <exception cref="InvalidOperationException">Thrown if the App UI system is not ready.</exception>
        internal static void UnregisterPanel(Panel panel)
        {
            if (s_Manager == null)
                throw new InvalidOperationException("The App UI Manager is not ready");

            s_Manager.UnregisterPanel(panel);
        }

        /// <summary>
        /// The main looper of the App UI system.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the App UI system is not ready.</exception>
        internal static Looper mainLooper
        {
            get
            {
                if (s_Manager == null)
                    throw new InvalidOperationException("The App UI Manager is not ready");

                return s_Manager.mainLooper;
            }
        }

        /// <summary>
        /// The notification manager of the App UI system.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the App UI system is not ready.</exception>
        internal static NotificationManager notificationManager
        {
            get
            {
                if (s_Manager == null)
                    throw new InvalidOperationException("The App UI Manager is not ready");

                return s_Manager.notificationManager;
            }
        }

        /// <summary>
        /// Dummy method to ensure the App UI system is initialized.
        /// </summary>
        internal static void EnsureInitialized() { }

        /// <summary>
        /// The update method that must be called every frame.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the App UI system is not ready.</exception>
        internal static void Update()
        {
            if (s_Manager == null)
                throw new InvalidOperationException("The App UI Manager is not ready");

            s_Manager.Update();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Initialize the App UI system in the editor.
        /// </summary>
        static void InitializeInEditor()
        {
            Reset();

            var existingSystemObjects = Resources.FindObjectsOfTypeAll<AppUISystemObject>();
            if (existingSystemObjects != null && existingSystemObjects.Length > 0)
            {
                s_SystemObject = existingSystemObjects[0];
                // here we can restore some state saved inside the system object
            }
            else
            {
                s_SystemObject = ScriptableObject.CreateInstance<AppUISystemObject>();
                s_SystemObject.hideFlags = HideFlags.HideAndDontSave;
            }

            if (EditorBuildSettings.TryGetConfigObject(AppUISettings.configName,
                    out AppUISettings settingsAsset))
            {
                if (s_Manager.m_Settings.hideFlags == HideFlags.HideAndDontSave)
                    ScriptableObject.DestroyImmediate(s_Manager.m_Settings);
                s_Manager.m_Settings = settingsAsset;
                // here we can apply new settings on managers
                s_Manager.ApplySettings();
            }
        }

        static void Reset()
        {
            if (s_Manager != null)
            {
                s_Manager.Shutdown();
            }

            var newSettings = ScriptableObject.CreateInstance<AppUISettings>();
            newSettings.hideFlags = HideFlags.HideAndDontSave;

            s_Manager = new AppUIManager();
            s_Manager.Initialize(newSettings);

            // here we can reset others managers

            EditorApplication.projectChanged -= OnProjectChange;
            EditorApplication.playModeStateChanged -= OnPlayModeChange;
            EditorApplication.projectChanged += OnProjectChange;
            EditorApplication.playModeStateChanged += OnPlayModeChange;
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        static void OnPlayModeChange(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (!string.IsNullOrEmpty(s_SystemObject.settings))
                    {
                        JsonUtility.FromJsonOverwrite(s_SystemObject.settings, settings);
                        s_SystemObject.settings = null;
                        settings.OnChange();
                    }
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    s_SystemObject.settings = JsonUtility.ToJson(settings);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        }

        static void OnProjectChange()
        {
            if (EditorUtility.InstanceIDToObject(settings.GetInstanceID()) == null)
            {
                var newSettings = ScriptableObject.CreateInstance<AppUISettings>();
                newSettings.hideFlags = HideFlags.HideAndDontSave;
                settings = newSettings;
            }
        }

        static double s_PreviousTime = 0;

        static void EditorUpdate()
        {
            if (s_Manager == null)
                return;

            if (settings.useCustomEditorUpdateFrequency)
            {
                var currentTime = Time.realtimeSinceStartupAsDouble;
                var delta = currentTime - s_PreviousTime;
                if (delta >= 1.0 / settings.editorUpdateFrequency)
                {
                    s_Manager.Update();
                    s_PreviousTime = currentTime;
                }
            }
            else
            {
                s_Manager.Update();
            }
        }
#else

        static AppUIManagerBehaviour s_Updater;

        static void InitializeInPlayer()
        {
            var settings = Resources.FindObjectsOfTypeAll<AppUISettings>();
            var newSettings = settings.Length > 0 ? settings[0] : null;
            if (!newSettings)
            {
                Debug.LogWarning("<b>[App UI]</b> Unable to find an AppUISettings instance, creating a default one...");
                newSettings = ScriptableObject.CreateInstance<AppUISettings>();
            }

            s_Manager = new AppUIManager();
            s_Manager.Initialize(newSettings);
        }

#endif

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunInitializeInPlayer()
        {
            // IL2CPP has a bug that causes the class constructor to not be run when
            // the RuntimeInitializeOnLoadMethod is invoked. So we need an explicit check
            // here until that is fixed (case 1014293).
#if !UNITY_EDITOR
            if (s_Manager == null)
                InitializeInPlayer();
#endif
        }

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AddUpdater()
        {
#if !UNITY_EDITOR
            if (s_Updater == null)
            {
                var availableUpdaters = Resources.FindObjectsOfTypeAll<AppUIManagerBehaviour>();
                if (availableUpdaters != null && availableUpdaters.Length > 0)
                {
                    for (var i = availableUpdaters.Length - 1; i >= 0; i--)
                    {
                        Object.Destroy(availableUpdaters[i].gameObject);
                    }
                }
                s_Updater = new GameObject("AppUIUpdater").AddComponent<AppUIManagerBehaviour>();
                s_Updater.hideFlags = HideFlags.HideAndDontSave;
                Object.DontDestroyOnLoad(s_Updater);
            }
#endif
        }

        /// <summary>
        /// The settings used by the App UI system.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
        public static AppUISettings settings
        {
            get
            {
                return s_Manager.settings;
            }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (s_Manager.settings == value)
                    return;

#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(value)))
                {
                    EditorBuildSettings.AddConfigObject(AppUISettings.configName,
                        value, true);
                }
#endif

                s_Manager.settings = value;
            }
        }
    }
}
