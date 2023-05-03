using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.ReferenceProject.Editor
{
    public static class Builder
    {
        // CI Entry point
        static void PerformBuild()
        {
            var buildSettings = new BuildSettingsParser();
            buildSettings.StartParsing();

            var target = buildSettings.ActiveBuildTarget;
            var buildDirectory = buildSettings.BuildDirectory;

            var scenePaths = GetScenePaths();
            var relativePath = AssembleName(target, buildDirectory);

            var buildReport = BuildPipeline.BuildPlayer(scenePaths, relativePath, target, BuildOptions.None);

            ParseBuildReport(buildReport);
        }

        // CI Entry point for processing the passed arguments without building
        static void ProcessArguments()
        {
            var buildSettings = new BuildSettingsParser();
            buildSettings.StartParsing();
        }

        // CI Entry point for a VR build
        static void PerformBuildVR()
        {
            SetupVR.SetVRBuildScenes();
            SetupVR.SetupOpenXR();
            PerformBuild();
        }

        // CI Entry point for a build supporting OpenXR
        static void PerformBuildWithOpenXR()
        {
            SetupVR.SetupOpenXR();
            PerformBuild();
        }

        static string AssembleName(BuildTarget buildTarget, string buildDirectory)
        {
            var fileName = GetFilenameFromBuildTarget(buildTarget);
            var stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(fileName))
            {
                stringBuilder.AppendFormat("{0}{1}{2}", buildDirectory, Path.DirectorySeparatorChar.ToString(), buildTarget.ToString());
            }
            else
            {
                stringBuilder.AppendFormat("{0}{1}{2}{1}{3}", buildDirectory, Path.DirectorySeparatorChar.ToString(), buildTarget.ToString(), fileName);
            }

            return stringBuilder.ToString();
        }

        static string GetProductName()
        {
            var productNameOverride = Environment.GetEnvironmentVariable("PRODUCT_NAME_OVERRIDE");
            return string.IsNullOrEmpty(productNameOverride) ? UnityEngine.Application.productName : productNameOverride;
        }

        static string GetProductNameNoSpace()
        {
            return GetProductName().Replace(" ", string.Empty);
        }

        static string GetFilenameFromBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return $"{GetProductNameNoSpace()}.apk";

                case BuildTarget.StandaloneOSX:
                    return $"{GetProductNameNoSpace()}.app";

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return $"{GetProductName()}.exe";

                case BuildTarget.iOS: // iOS doesn't need the name set. XCode will do these for us.
                default:
                    return string.Empty;
            }
        }

        static string[] GetScenePaths()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }

            return scenes;
        }

        static void ParseBuildReport(BuildReport buildReport)
        {
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[Builder] Build failed with result of " + buildReport.summary.result);
                Debug.LogError("[Builder] Number of errors found " + buildReport.summary.totalErrors);

                EditorApplication.Exit(1);
            }

            if (buildReport.summary.totalErrors > 0)
            {
                Debug.LogError("[ ~~ TESTING IF THIS MATCHES ~~ ]" + buildReport.summary.totalErrors);
            }
        }
    }
}
