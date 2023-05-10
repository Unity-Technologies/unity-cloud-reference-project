using System;
using System.Collections.Generic;
using System.IO;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Unity.ReferenceProject.Editor
{
    public class BuildSettingsParser
    {
        Dictionary<string, string> m_CommandLineArgsByKey;

        public BuildTarget ActiveBuildTarget { get; private set; }

        public string BuildDirectory { get; private set; }

        public void StartParsing()
        {
            Initialize();
            ParseArguments();
            ActOnHandledArguments();
        }

        void Initialize()
        {
            m_CommandLineArgsByKey = new Dictionary<string, string>();
            ActiveBuildTarget = BuildTarget.NoTarget;
            BuildDirectory = BuilderConstants.DEFAULT_BUILD_DIRECTORY;
        }

        void ParseArguments()
        {
            var commandLinesArgs = Environment.GetCommandLineArgs();
            var len = commandLinesArgs.Length;
            for (var i = 0; i < len; ++i)
            {
                var argument = commandLinesArgs[i];
                var isLast = (i + 1) == len;

                if (argument.StartsWith("-"))
                {
                    var value = commandLinesArgs[i + 1];
                    if (!isLast && !value.StartsWith("-"))
                    {
                        m_CommandLineArgsByKey.Add(argument, value);
                    }
                }
            }
        }

        void ActOnHandledArguments()
        {
            if (m_CommandLineArgsByKey.ContainsKey(BuilderArguments.APP_ID))
            {
                SetAppID(m_CommandLineArgsByKey[BuilderArguments.APP_ID],
                    m_CommandLineArgsByKey[BuilderArguments.APP_NAME],
                    m_CommandLineArgsByKey[BuilderArguments.APP_DISPLAY_NAME]);
            }

            if (m_CommandLineArgsByKey.ContainsKey(BuilderArguments.BUILD_TARGET))
            {
                SwitchToRespectiveBuildTarget(m_CommandLineArgsByKey[BuilderArguments.BUILD_TARGET]);
            }

            if (m_CommandLineArgsByKey.ContainsKey(BuilderArguments.OUTPUT_PATH))
            {
                BuildDirectory = m_CommandLineArgsByKey[BuilderArguments.OUTPUT_PATH];
            }
        }

        static void SetAppID(string appID, string appName, string appDisplayName)
        {
            var settings = ScriptableObject.CreateInstance<UnityCloudPlayerSettings>();

            settings.AppId = appID;
            settings.AppName = appName;
            settings.AppDisplayName = appDisplayName;

            const string directory = "Assets/Resources/";

            if (!AssetDatabase.IsValidFolder(directory))
                Directory.CreateDirectory(directory);

            var assetPath = $"{directory}{UnityCloudPlayerSettings.k_AssetName}.asset";
            AssetDatabase.CreateAsset(settings, assetPath);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);

            Debug.Log($"App ID successfully set in '{assetPath}'");
        }

        void SwitchToRespectiveBuildTarget(string buildTarget)
        {
            switch (buildTarget)
            {
                case BuilderConstants.ANDROID_BUILD_TARGET:
                    ActiveBuildTarget = BuildTarget.Android;
                    break;

                case BuilderConstants.IOS_BUILD_TARGET:
                    ActiveBuildTarget = BuildTarget.iOS;
                    break;

                case BuilderConstants.OSX_BUILD_TARGET:
                    ActiveBuildTarget = BuildTarget.StandaloneOSX;
                    break;

                case BuilderConstants.WIN_BUILD_TARGET:
                    ActiveBuildTarget = BuildTarget.StandaloneWindows64;
                    break;

                case BuilderConstants.WEBGL_BUILD_TARGET:
                    ActiveBuildTarget = BuildTarget.WebGL;
                    break;

                default:
                    throw new BuildFailedException($"[{nameof(BuildSettingsParser)}] Invalid Build target: " + buildTarget);
            }

            Debug.Log($"Active Build Target set to '{ActiveBuildTarget}'");
        }
    }
}
