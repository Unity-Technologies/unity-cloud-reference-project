namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// The settings for the App UI system.
    /// </summary>
    [CreateAssetMenu(menuName = "App UI/Settings", fileName = "App UI Settings")]
    public class AppUISettings : ScriptableObject
    {
        internal const string configName = "com.unity.dt.app-ui";

        /// <summary>
        /// Enable this options to correct the scale of UIDocuments,
        /// depending on the target platform and screen dpi.
        /// </summary>
        public bool autoCorrectUiScale
        {
            get => m_AutoCorrectUiScale;
            set
            {
                if (value == m_AutoCorrectUiScale)
                    return;

                m_AutoCorrectUiScale = value;
                OnChange();
            }
        }

        /// <summary>
        /// Enable this option to use a custom update frequency for the App UI system in the editor.
        /// </summary>
        public bool useCustomEditorUpdateFrequency
        {
            get => m_UseCustomEditorUpdateFrequency;
            set
            {
                if (m_UseCustomEditorUpdateFrequency == value)
                    return;
                m_UseCustomEditorUpdateFrequency = value;
                OnChange();
            }
        }

        /// <summary>
        /// Configure how frequently you want to run the main App UI process loop (to handle queued messages for examples).
        /// Default is 60Hz.
        /// </summary>
        public float editorUpdateFrequency
        {
            get => m_EditorUpdateFrequency;
            set
            {
                if (Mathf.Approximately(value, m_EditorUpdateFrequency))
                    return;

                m_EditorUpdateFrequency = value;
                OnChange();
            }
        }

        /// <summary>
        /// Enable this option to automatically override the AndroidManifest.xml file with the one provided by the App UI system.
        /// </summary>
        public bool autoOverrideAndroidManifest
        {
            get => m_AutoOverrideAndroidManifest;
            set
            {
                if (m_AutoOverrideAndroidManifest == value)
                    return;

                m_AutoOverrideAndroidManifest = value;
                OnChange();
            }
        }

        [Tooltip("Enable this options to correct the scale of UIDocuments, depending on the target platform and screen dpi.")]
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        bool m_AutoCorrectUiScale = true;

        [Tooltip("Enable this option to use a custom update frequency for the App UI system in the editor.")]
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        bool m_UseCustomEditorUpdateFrequency = false;

        [Tooltip("Configure how frequently you want to run the main App UI process loop (to handle queued messages for examples). Default is 60Hz.")]
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float m_EditorUpdateFrequency = 60;

        [Tooltip("Enable this option to automatically override the AndroidManifest.xml file with the one provided by the App UI system.")]
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        bool m_AutoOverrideAndroidManifest = true;

        internal void OnChange()
        {
            if (this != AppUI.s_Manager.defaultSettings && AppUI.settings == this)
                AppUI.s_Manager.ApplySettings();
        }
    }
}
