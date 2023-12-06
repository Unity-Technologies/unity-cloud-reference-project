using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using UnityEditor.OSXStandalone;
#endif
using UnityEngine;

namespace Unity.ReferenceProject.Editor
{
    public static class Utils
    {
        public static IDictionary<string, string> ParseArguments()
        {
            var commandLinesArgs = Environment.GetCommandLineArgs();
            var len = commandLinesArgs.Length;
            var arguments = new Dictionary<string, string>(len);
            for (var i = 0; i < len; ++i)
            {
                var argument = commandLinesArgs[i];
                var isLast = (i + 1) == len;

                if (argument.StartsWith("-"))
                {
                    var value = commandLinesArgs[i + 1];
                    if (!isLast && !value.StartsWith("-"))
                    {
                        arguments.Add(argument, value);
                    }
                }
            }
            return arguments;
        }
    }

    public class BuildSettingsParser
    {
        public BuildTarget ActiveBuildTarget { get; private set; }

        public string BuildDirectory { get; private set; }

        public void StartParsing()
        {
            Initialize();
            ProcessHandledArguments(Utils.ParseArguments());
        }

        void Initialize()
        {
            ActiveBuildTarget = BuildTarget.NoTarget;
            BuildDirectory = BuilderConstants.DEFAULT_BUILD_DIRECTORY;
        }

        void ProcessHandledArguments(IDictionary<string, string> arguments)
        {

            if (arguments.TryGetValue(BuilderArguments.BUILD_TARGET, out var buildTarget))
            {
                SwitchToRespectiveBuildTarget(buildTarget);
            }

            if (arguments.TryGetValue(BuilderArguments.OUTPUT_PATH, out var outputPath))
            {
                BuildDirectory = outputPath;
            }

            if (arguments.TryGetValue(BuilderArguments.APP_ID, out var appId)
                && arguments.TryGetValue(BuilderArguments.APP_NAME, out var appName)
                && arguments.TryGetValue(BuilderArguments.APP_DISPLAY_NAME, out var appDisplayName)
                && arguments.TryGetValue(BuilderArguments.ORGANIZATION_ID, out var organizationId))
            {
                BuildCloudPlayerSettingsProvider.CreateCloudPlayerSettings(appId, appName, appDisplayName, organizationId);
            }
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
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    UserBuildSettings.architecture = OSArchitecture.x64;
#endif
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
