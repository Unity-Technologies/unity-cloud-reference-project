using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Unity.ReferenceProject.Editor
{
    public class SetupVR : UnityEditor.Editor
    {
        const string k_VRPath = "Assets/_VR";
        const string k_MainVRPath = k_VRPath + "/Scenes/MainVR.unity";
        const string k_OpenXRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader";
        const string k_XRPath = "Assets/XR";
        static AddRequest s_AddRequest;
        static Queue<string> s_PkgNameQueue;

        public static void SetVRBuildScenes()
        {
            EditorBuildSettings.scenes = Array.Empty<EditorBuildSettingsScene>();
            AddBuildScenes(new[] { k_MainVRPath });
        }

        public static void SetupOpenXR()
        {
            EnablePlugin(BuildTargetGroup.Standalone, k_OpenXRLoader);
            XRGeneralSettings.Instance.InitManagerOnStart = false;

            var settings = GetOrCreateOpenXRSettings();

            var features = settings.GetFeatures();
            foreach (var feature in features)
            {
                switch (feature.name)
                {
                    case "MockRuntime Standalone":
                        feature.enabled = false;
                        break;
                    case "HTCViveControllerProfile Standalone":
                        feature.enabled = true;
                        break;
                    case "OculusTouchControllerProfile Standalone":
                        feature.enabled = true;
                        break;
                    case "ValveIndexControllerProfile Standalone":
                        feature.enabled = true;
                        break;
                    case "MicrosoftMotionControllerProfile Standalone":
                        feature.enabled = true;
                        break;
                    case "KHRSimpleControllerProfile Standalone":
                        feature.enabled = true;
                        break;
                }
            }
        }

        static OpenXRSettings GetOrCreateOpenXRSettings()
        {
            var settings = OpenXRSettings.Instance;
            if (settings != null)
                return settings;

            var objectType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                from type in asm.GetTypes()
                where type.IsClass && type.Name == "FeatureHelpers"
                select type).Single();

            var method = objectType?.GetMethod("RefreshFeatures",
                BindingFlags.Static | BindingFlags.Public);

            method?.Invoke(null, new object[] { BuildTargetGroup.Standalone });

            return OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
        }

        static void EnablePlugin(BuildTargetGroup buildTargetGroup, string plugin)
        {
            var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            if (buildTargetSettings == null)
            {
                EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var generalSettings);
                if (generalSettings == null)
                {
                    generalSettings = CreateInstance(typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                    var assetPath = k_XRPath;
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        if (!AssetDatabase.IsValidFolder(k_XRPath))
                        {
                            AssetDatabase.CreateFolder("Assets", "XR");
                        }

                        assetPath = Path.Combine(assetPath, "XRGeneralSettings.asset");
                        AssetDatabase.CreateAsset(generalSettings, assetPath);
                    }

                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, generalSettings, true);
                }

                generalSettings.CreateDefaultManagerSettingsForBuildTarget(buildTargetGroup);
                buildTargetSettings = generalSettings.SettingsForBuildTarget(buildTargetGroup);
            }

            var pluginsSettings = buildTargetSettings.AssignedSettings;

            var success = XRPackageMetadataStore.AssignLoader(pluginsSettings, plugin, buildTargetGroup);
            if (success)
            {
                Debug.Log($"Enabled {plugin} plugin on {buildTargetGroup}");
            }
        }

        static void AddBuildScenes(string[] scenes)
        {
            var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (var scene in scenes)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scene, true));
            }

            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
    }
}
