using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.ReferenceProject.Editor
{
    public class QuestProjectSettings : UnityEditor.Editor
    {
        static string s_PipeLineName = "URP-Performant";
        static readonly AndroidSdkVersions s_MinAndroidVersion = AndroidSdkVersions.AndroidApiLevel29;

        [MenuItem("ReferenceProject/VR/Switch/To Quest project", false, 10)]
        public static void Switch2Quest()
        {
            SetupVR.EnsureAndroidBuildTarget();

            SetupVR.SetupOpenXR(SetupVR.DeviceTarget.Standalone); // Added to be able to test in Editor
            SetupVR.SetupOpenXR(SetupVR.DeviceTarget.Quest);
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
                Debug.Log("Switch to Quest finished with success");
            }
            catch (Exception e)
            {
                Debug.LogError($"Quest 2 settings could not be set: {e.GetType().Name}; {e.Message}; {e.StackTrace}");
            }
        }

        static void SetAndroidSettings()
        {
            // Enable multi-threaded rendering
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);

            // Set Minimum Android version
            // https://developer.oculus.com/documentation/native/android/mobile-application-signing/
            if (PlayerSettings.Android.minSdkVersion < s_MinAndroidVersion || PlayerSettings.Android.targetSdkVersion < s_MinAndroidVersion)
            {
                PlayerSettings.Android.minSdkVersion = s_MinAndroidVersion;
            }

            PlayerSettings.Android.androidTVCompatibility = false;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan });
        }
    }
}
