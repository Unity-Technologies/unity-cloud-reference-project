#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.Dt.App.Core;

namespace UnityEngine.Dt.App.Editor
{
    class AppUISettingsProvider : SettingsProvider, IDisposable
    {
        public const string kEditorBuildSettingsConfigKey = "com.unity.dt.app-ui";
        public const string kSettingsPath = "Project/App UI";

        public static void Open()
        {
            SettingsService.OpenProjectSettings(kSettingsPath);
        }

        [SettingsProvider]
        public static SettingsProvider CreateAppUISettingsProvider()
        {
            return new AppUISettingsProvider(kSettingsPath, SettingsScope.Project);
        }

        AppUISettingsProvider(string path, SettingsScope scopes)
            : base(path, scopes)
        {
            label = "App UI";
            s_Instance = this;
        }

        public void Dispose()
        {
            m_SettingsObject?.Dispose();
        }

        public override void OnTitleBarGUI()
        {
            if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("_Popup"), FocusType.Passive, EditorStyles.label))
            {
                var menu = new GenericMenu();
                menu.AddDisabledItem(new GUIContent("Available Settings Assets:"));
                menu.AddSeparator("");
                for (var i = 0; i < m_AvailableSettingsAssetsOptions.Length; i++)
                    menu.AddItem(new GUIContent(m_AvailableSettingsAssetsOptions[i]), m_CurrentSelectedInputSettingsAsset == i, (path) => {
                        AppUI.settings = AssetDatabase.LoadAssetAtPath<AppUISettings>((string)path);
                    }, m_AvailableInputSettingsAssets[i]);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("New Settings Asset…"), false, CreateNewSettingsAsset);
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        public override void OnGUI(string searchContext)
        {
            InitializeWithCurrentSettingsIfNecessary();

            EditorGUIUtility.labelWidth = 200;

            if (m_AvailableInputSettingsAssets.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Settings for App UI are stored in an asset. Click the button below to create a settings asset you can edit.",
                    MessageType.Info);
                if (GUILayout.Button("Create settings asset", GUILayout.Height(30)))
                    CreateNewSettingsAsset();
                GUILayout.Space(20);
            }

            using (new EditorGUI.DisabledScope(m_AvailableInputSettingsAssets.Count == 0))
            {
                EditorGUILayout.Space();
                EditorGUILayout.Separator();
                EditorGUILayout.Space();

                Debug.Assert(m_Settings != null);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_AutoScaleUI, m_AutoScaleUIContent);

                EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(m_UseCustomEditorUpdateFrequency, m_UseCustomEditorUpdateFrequencyContent);

                using (new EditorGUI.DisabledScope(!m_UseCustomEditorUpdateFrequency.boolValue))
                {
                    EditorGUILayout.PropertyField(m_EditorUpdateFrequency, m_EditorUpdateFrequencyContent);
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox("In order to get all features working properly on Android, " +
                    "you need to override the default Android manifest file with the one provided by App UI.",
                    MessageType.Warning);

                EditorGUILayout.PropertyField(m_AutoOverrideAndroidManifest, m_AutoOverrideAndroidManifestContent);

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                if (EditorGUI.EndChangeCheck())
                    Apply();
            }
        }

        internal static void CreateNewSettingsAsset(string relativePath)
        {
            var existingGuid = AssetDatabase.AssetPathToGUID(relativePath, AssetPathToGUIDOptions.OnlyExistingAssets);
            if (string.IsNullOrEmpty(existingGuid))
            {
                // Create settings file.
                var settings = ScriptableObject.CreateInstance<AppUISettings>();
                AssetDatabase.CreateAsset(settings, relativePath);
                EditorGUIUtility.PingObject(settings);
                // Install the settings. This will lead to an AppUI.settingsChanged event which in turn
                // will cause us to re-initialize.
                AppUI.settings = settings;
            }
        }

        static void CreateNewSettingsAsset()
        {
            // Query for file name.
            var path = EditorUtility.SaveFilePanel("Create App UI Settings File", "Assets",
                "App UI Settings", "asset");
            if (string.IsNullOrEmpty(path))
                return;

            // Make sure the path is in the Assets/ folder.
            path = path.Replace("\\", "/"); // Make sure we only get '/' separators.
            var dataPath = Application.dataPath + "/";
            if (!path.StartsWith(dataPath, StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.LogError($"App UI settings must be stored in Assets folder of the project (got: '{path}')");
                return;
            }

            // Make sure it ends with .asset.
            var extension = Path.GetExtension(path);
            if (string.Compare(extension, ".asset", StringComparison.InvariantCultureIgnoreCase) != 0)
                path += ".asset";

            // Create settings file.
            var relativePath = "Assets/" + path.Substring(dataPath.Length);
            CreateNewSettingsAsset(relativePath);
        }

        void InitializeWithCurrentSettingsIfNecessary()
        {
            if (AppUI.settings == m_Settings && m_Settings != null && m_SettingsDirtyCount == EditorUtility.GetDirtyCount(m_Settings))
                return;

            InitializeWithCurrentSettings();
        }

        /// <summary>
        /// Grab <see cref="AppUI.settings"/> and set it up for editing.
        /// </summary>
        void InitializeWithCurrentSettings()
        {
            // Find the set of available assets in the project.
            m_AvailableInputSettingsAssets = new List<string>(FindInputSettingsInProject());

            // See which is the active one.
            m_Settings = AppUI.settings;
            m_SettingsDirtyCount = EditorUtility.GetDirtyCount(m_Settings);
            var currentSettingsPath = AssetDatabase.GetAssetPath(m_Settings);
            if (string.IsNullOrEmpty(currentSettingsPath))
            {
                if (m_AvailableInputSettingsAssets.Count != 0)
                {
                    m_CurrentSelectedInputSettingsAsset = 0;
                    m_Settings = AssetDatabase.LoadAssetAtPath<AppUISettings>(m_AvailableInputSettingsAssets[0]);
                    AppUI.settings = m_Settings;
                }
            }
            else
            {
                m_CurrentSelectedInputSettingsAsset = m_AvailableInputSettingsAssets.IndexOf(currentSettingsPath);
                if (m_CurrentSelectedInputSettingsAsset == -1)
                {
                    m_AvailableInputSettingsAssets.Add(currentSettingsPath);
                    m_CurrentSelectedInputSettingsAsset = m_AvailableInputSettingsAssets.IndexOf(currentSettingsPath);
                }

                ////REVIEW: should we store this by platform?
                EditorBuildSettings.AddConfigObject(kEditorBuildSettingsConfigKey, m_Settings, true);
            }

            // Refresh the list of assets we display in the UI.
            m_AvailableSettingsAssetsOptions = new GUIContent[m_AvailableInputSettingsAssets.Count];
            for (var i = 0; i < m_AvailableInputSettingsAssets.Count; ++i)
            {
                var name = m_AvailableInputSettingsAssets[i];
                if (name.StartsWith("Assets/"))
                    name = name.Substring("Assets/".Length);
                if (name.EndsWith(".asset"))
                    name = name.Substring(0, name.Length - ".asset".Length);

                // Ugly hack: GenericMenu interprets "/" as a submenu path. But luckily, "/" is not the only slash we have in Unicode.
                m_AvailableSettingsAssetsOptions[i] = new GUIContent(name.Replace("/", "\u29f8"));
            }

            // Look up properties.
            m_SettingsObject = new SerializedObject(m_Settings);
            m_AutoScaleUI = m_SettingsObject.FindProperty("m_AutoCorrectUiScale");
            m_AutoScaleUIContent = new GUIContent("Auto Scale UI",
                "Enable this options to correct the scale of UIDocuments, depending on the target platform and screen dpi.");
            
            m_UseCustomEditorUpdateFrequency = m_SettingsObject.FindProperty("m_UseCustomEditorUpdateFrequency");
            m_UseCustomEditorUpdateFrequencyContent = new GUIContent("Use Custom Loop Frequency",
                "Enable this option to override the default update loop frequency (the default frequency is the one used by the Editor loop).");

            m_EditorUpdateFrequency = m_SettingsObject.FindProperty("m_EditorUpdateFrequency");
            m_EditorUpdateFrequencyContent = new GUIContent("Update Loop Frequency",
                "Configure how frequently you want to run the main App UI process loop (to handle queued messages for examples). Default is 60Hz.");

            m_AutoOverrideAndroidManifest = m_SettingsObject.FindProperty("m_AutoOverrideAndroidManifest");
            m_AutoOverrideAndroidManifestContent = new GUIContent("Auto Override Android Manifest", "");
        }

        void Apply()
        {
            if (!m_Settings)
                return;

            m_SettingsObject.ApplyModifiedProperties();
            m_SettingsObject.Update();
            m_Settings.OnChange();
        }

        /// <summary>
        /// Find all <see cref="AppUISettings"/> stored in assets in the current project.
        /// </summary>
        /// <returns>List of input settings in project.</returns>
        static IEnumerable<string> FindInputSettingsInProject()
        {
            var guids = AssetDatabase.FindAssets("t:AppUISettings");
            
            var paths = new List<string>();
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                paths.Add(path);
            }
            
            return paths;
        }

        [SerializeField] AppUISettings m_Settings;
        [SerializeField] bool m_SettingsIsNotAnAsset;

        [NonSerialized] int m_SettingsDirtyCount;
        [NonSerialized] SerializedObject m_SettingsObject;
        [NonSerialized] SerializedProperty m_AutoScaleUI;
        [NonSerialized] SerializedProperty m_UseCustomEditorUpdateFrequency;
        [NonSerialized] SerializedProperty m_EditorUpdateFrequency;
        [NonSerialized] SerializedProperty m_AutoOverrideAndroidManifest;

        [NonSerialized] List<string> m_AvailableInputSettingsAssets;
        [NonSerialized] GUIContent[] m_AvailableSettingsAssetsOptions;
        [NonSerialized] int m_CurrentSelectedInputSettingsAsset;

        [NonSerialized] GUIStyle m_NewAssetButtonStyle;

        GUIContent m_AutoScaleUIContent;
        GUIContent m_UseCustomEditorUpdateFrequencyContent;
        GUIContent m_EditorUpdateFrequencyContent;
        GUIContent m_AutoOverrideAndroidManifestContent;

        static AppUISettingsProvider s_Instance;

        internal static void ForceReload()
        {
            if (s_Instance != null)
            {
                // Force next OnGUI() to re-initialize.
                s_Instance.m_Settings = null;

                // Request repaint.
                SettingsService.NotifySettingsProviderChanged();
            }
        }
    }
}
#endif // UNITY_EDITOR
