using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

namespace Unity.Cloud.ReferenceProject.Utils.Git {

    /*
     * See VersionControlInformation for runtime access.
     */
    public static class GitVersionControlEditor
    {
        const string k_ResourcesDirectory = "Assets/Utilities/Git/Resources/";
        static readonly string k_AssetPath = $"{k_ResourcesDirectory}{VersionControlInformation.k_AssetName}.asset";

        static string GitHashShortArgs = "rev-parse --short --verify HEAD";

#if USE_GIT_INFO
        [DidReloadScripts]
#endif
        public static void UpdateGitHashShort()
        {
            try
            {
                VersionControlInformation.Instance.SetHash(RunGitCommand(GitHashShortArgs));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            SaveOrCreateAsset();
        }

        static void SaveOrCreateAsset()
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(k_ResourcesDirectory))
                {
                    Directory.CreateDirectory(k_ResourcesDirectory);
                }
                if (!AssetDatabase.Contains(VersionControlInformation.Instance))
                    AssetDatabase.CreateAsset(VersionControlInformation.Instance, k_AssetPath);

                EditorUtility.SetDirty(VersionControlInformation.Instance);

                AssetDatabase.SaveAssetIfDirty(VersionControlInformation.Instance);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        static string RunGitCommand(string args)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("git", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = new Process()
            {
                StartInfo = processStartInfo
            };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                throw;
            }

            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();

            process.Close();

            if (!string.IsNullOrEmpty(errorOutput))
            {
                Debug.LogError(errorOutput);
                return "SomethingWentWrong";
            }

            return output;
        }
    }
}
