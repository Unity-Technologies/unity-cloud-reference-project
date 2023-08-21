using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.ReferenceProject.Editor
{
    public class Focus3ProjectSettings : UnityEditor.Editor
    {
        static readonly string s_PipeLineName = "URP-Performant";
        static readonly AndroidSdkVersions s_MinAndroidVersion = AndroidSdkVersions.AndroidApiLevel22;
        
        static readonly string s_PackageName = "com.htc.upm.wave.openxr";
        static readonly string s_Url = "https://github.com/ViveSoftware/VIVE-OpenXR-AIO/releases/download/versions%2F1.0.5/com.htc.upm.wave.openxr-1.0.5.tgz";
        
        static readonly string s_PackageFileName = Path.GetFileName(s_Url);
        static readonly string s_SavePath = Path.GetDirectoryName(Application.dataPath) + $@"\Packages\{s_PackageFileName}";
        static readonly string s_PackagePath = $"file:{s_PackageFileName}";
        static readonly string s_EditroPrefs = "Focus3ChangeSettings";
        
        [MenuItem("ReferenceProject/VR/Switch/To Focus3 project", false, 11)]
        public static void DownloadPackage()
        {
            DownloadFile(s_Url, s_SavePath, (o, e) =>
            {
                if (e.Error == null)
                {
                    SetupPackage();
                }
                else
                {
                    Debug.LogError($"Error downloading file: {e.Error}");
                }
            });
        }
        
        static void SetupPackage()
        {
            if (SetupVR.AddPackages(s_PackagePath))
            {
                EditorPrefs.SetBool(s_EditroPrefs, true);
                CompilationPipeline.RequestScriptCompilation();
            }
        }

        [DidReloadScripts]
        static void DidRecompile()
        {
            if (EditorPrefs.GetBool(s_EditroPrefs, false))
            {
                EditorPrefs.DeleteKey(s_EditroPrefs);
                
                EditorApplication.update += UpdateChangeSettings;
                
                Debug.Log("Started to update settings. Please wait...");
            }
        }
        
        static void UpdateChangeSettings()
        {
            // We need to run update only once
            EditorApplication.update -= UpdateChangeSettings;
            
            if (!SetupVR.CheckPackagesExistence(s_PackageName))
            {
                Debug.LogError("Focus3 switch failed");
                return;
            }

            SetupPlayerSettings();
            Debug.Log("Focus3 switch completed");
        }

        static void SetupPlayerSettings()
        {
            SetupVR.EnsureAndroidBuildTarget();
            
            SetupVR.SetupOpenXR(SetupVR.DeviceTarget.Focus3);
            SetupVR.SetVRBuildScenes();

            try
            {
                EditorApplication.LockReloadAssemblies();
                try
                {
                    SetupVR.PrepareRendererPipelineAsset(s_PipeLineName);
                    SetAndroidSettings();
                }
                finally
                {
                    EditorApplication.UnlockReloadAssemblies();
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            catch (Exception e)
            {
                Debug.LogError($"Focus3 settings could not be set: {e.GetType().Name}; {e.Message}; {e.StackTrace}");
                throw;
            }
        }

        static void SetAndroidSettings()
        {
            // Enable multi-threaded rendering
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);

            // Set Minimum Android version
            // https://developer.vive.com/resources/openxr/openxr-mobile/tutorials/unity/getting-started-openxr-mobile/
            if (PlayerSettings.Android.minSdkVersion < s_MinAndroidVersion || PlayerSettings.Android.targetSdkVersion < s_MinAndroidVersion)
            {
                PlayerSettings.Android.minSdkVersion = s_MinAndroidVersion;
            }

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = false;
            
            // Set IL2CPP
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Remove the "Auto Graphics API" option
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            
            PlayerSettings.Android.androidTVCompatibility = false;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        }
        
        static void DownloadFile(string url, string savePath, AsyncCompletedEventHandler callback)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFileCompleted += callback;
                client.DownloadFileAsync(new Uri (url), savePath);
                Debug.Log("Downloading started");
            }
        }
    }
}
