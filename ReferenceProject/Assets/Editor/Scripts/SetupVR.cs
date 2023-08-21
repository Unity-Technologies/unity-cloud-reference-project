using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Unity.ReferenceProject.Editor
{
    public class SetupVR : UnityEditor.Editor
    {
        const string k_VRPath = "Assets/_VR/_Application";
        const string k_MainVRPath = k_VRPath + "/Scenes/MainVR.unity";
        const string k_StreamingVRPath = k_VRPath + "/Scenes/StreamingVR.unity";
        const string k_OpenXRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader";
        const string k_XRPath = "Assets/XR";
        static AddRequest s_AddRequest;
        static Queue<string> s_PkgNameQueue;

        public enum DeviceTarget
        {
            Standalone,
            Quest,
            Focus3
        }

        public static void SetVRBuildScenes()
        {
            EditorBuildSettings.scenes = Array.Empty<EditorBuildSettingsScene>();
            AddBuildScenes(new[] { k_MainVRPath, k_StreamingVRPath });
        }

        public static void SetupOpenXR(DeviceTarget deviceTarget = 0)
        {
            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
            switch (deviceTarget)
            {
                case DeviceTarget.Standalone:
                    buildTargetGroup = BuildTargetGroup.Standalone;
                    break;
                case DeviceTarget.Quest:
                case DeviceTarget.Focus3:
                    buildTargetGroup = BuildTargetGroup.Android;
                    break;
            }

            EnablePlugin(buildTargetGroup, k_OpenXRLoader);
            XRGeneralSettings.Instance.InitManagerOnStart = false;
            var settings = GetOrCreateOpenXRSettings(buildTargetGroup);
            var features = settings.GetFeatures();
            foreach (var feature in features)
            {
                switch (deviceTarget)
                {
                    case DeviceTarget.Standalone:
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
                        break;
                    case DeviceTarget.Quest:
                        switch (feature.name)
                        {
                            case "MockRuntime Standalone":
                                feature.enabled = false;
                                break;
                            case "VIVEFocus3Feature Android":
                                feature.enabled = false;
                                break;
                            case "VIVEFocus3Profile Android":
                                feature.enabled = false;
                                break;
                            case "OculusTouchControllerProfile Android":
                                feature.enabled = true;
                                break;
                            case "MetaQuestFeature Android":
                                feature.enabled = true;
                                break;
                        }
                        break;
                    case DeviceTarget.Focus3:
                        switch (feature.name)
                        {
                            case "MockRuntime Standalone":
                                feature.enabled = false;
                                break;
                            case "OculusTouchControllerProfile Android":
                                feature.enabled = false;
                                break;
                            case "MetaQuestFeature Android":
                                feature.enabled = false;
                                break;
                            case "VIVEFocus3Feature Android":
                                feature.enabled = true;
                                break;
                            case "VIVEFocus3Profile Android":
                                feature.enabled = true;
                                break;
                        }
                        break;
                }
            }
        }

        public static OpenXRSettings GetOrCreateOpenXRSettings(BuildTargetGroup buildTargetGroup)
        {
            var settings = OpenXRSettings.Instance;
            if (settings == null)
            {
                var objectType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                    from type in asm.GetTypes()
                    where type.IsClass && type.Name == "FeatureHelpers"
                    select type).Single();

                var method = objectType?.GetMethod("RefreshFeatures",
                    BindingFlags.Static | BindingFlags.Public);

                method?.Invoke(null, new object[] { buildTargetGroup });
            }

            return OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
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
        
        public static void EnsureAndroidBuildTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android || EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }
        
        public static void PrepareRendererPipelineAsset(string pipeLineName)
        {
            foreach (var pipeline in GraphicsSettings.allConfiguredRenderPipelines.OfType<UniversalRenderPipelineAsset>())
            {
                if (pipeline.name == pipeLineName)
                {
                    GraphicsSettings.defaultRenderPipeline = pipeline;
                    EditorUtility.SetDirty(pipeline);
                    AssetDatabase.SaveAssetIfDirty(pipeline);
                    return;
                }
            }
        }
        
        public static bool AddPackages(params string[] packs)
        {
            var request = Client.List(true);
            
            if(!WaitRequestForCompletion(request))
            {
                Debug.LogError($"Could not obtain the list of packages. Can not continue. ErrorCode:{request.Error.errorCode} message: {request.Error.message}");
                return false;
            }

            foreach (var pack in packs)
            {
                if (request.Result.Any(x => x.name.Equals(pack)))
                {
                    Debug.LogWarning($"{pack} is already in the project and cannot be added. Skipping");
                    continue;
                }

                var addRequest = Client.Add(pack);
                
                if(WaitRequestForCompletion(addRequest))
                {
                    Debug.Log($"{pack} was added");
                }
                else
                {
                    Debug.LogError($"{pack} cannot be added: errorCode:{addRequest.Error.errorCode}; message: {addRequest.Error.message}");
                    return false;
                }
            }
            
            return true;
        }
        
        public static bool CheckPackagesExistence(params string[] packageNames)
        {
            var request = Client.List(true);

            if(!WaitRequestForCompletion(request))
            {
                Debug.LogError($"Could not obtain the list of packages. Can not continue. ErrorCode:{request.Error.errorCode} message: {request.Error.message}");
                return false;
            }

            int packagesCount = 0;

            foreach (var package in request.Result)
            {
                foreach (var packageName in packageNames)
                {
                    if (package.name == packageName)
                    {
                        packagesCount++;
                        break;
                    }
                }
            }

            return packagesCount == packageNames.Length;
        }

        static bool WaitRequestForCompletion(Request request)
        {
            while (!request.IsCompleted)
            {
                // Wait for completion.
            }
            
            if (request.Status != StatusCode.Success)
            {
                return false;
            }

            return true;
        }
    }
}
