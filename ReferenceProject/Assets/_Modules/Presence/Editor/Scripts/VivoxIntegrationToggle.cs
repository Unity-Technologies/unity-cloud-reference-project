using System;
using System.Linq;
using UnityEditor;
#if UNITY_EDITOR_OSX
using UnityEditor.OSXStandalone;
#endif
using UnityEngine;
using static UnityEditor.EditorUtility;

namespace Unity.ReferenceProject.Presence.Editor
{
    static class VivoxIntegrationToggle
    {
        static readonly string k_UseVivoxDefine = "USE_VIVOX";
        static readonly string k_VivoxPackageName = "com.unity.services.vivox@15.1.180001-pre.5";

        [MenuItem("ReferenceProject/Vivox/Enable Vivox Integration")]
        static void EnableVivoxIntegration()
        {
            PackageManagerUtils.AddPackages(k_VivoxPackageName);
            AddEnvironmentVariable(k_UseVivoxDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        [MenuItem("ReferenceProject/Vivox/Disable Vivox Integration")]
        static void DisableVivoxIntegration()
        {
            PackageManagerUtils.RemovePackages(k_VivoxPackageName);
            RemoveEnvironmentVariable(k_UseVivoxDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        static void AddEnvironmentVariable(string val, BuildTargetGroup buildTargetGroup)
        {
            var scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').ToList();
            if (!scriptingDefines.Contains(val, StringComparer.OrdinalIgnoreCase))
            {
                scriptingDefines.Add(val);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", scriptingDefines.ToArray()));
            }
        }

        static void RemoveEnvironmentVariable(string val, BuildTargetGroup buildTargetGroup)
        {
            var scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').ToList();
            if (scriptingDefines.Contains(val, StringComparer.OrdinalIgnoreCase))
            {
                scriptingDefines.Remove(val);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", scriptingDefines.ToArray()));
            }
        }

        public static bool IsSiliconEditor
        {
            get
            {
                return SystemInfo.processorType.Contains("Apple") ^ IsRunningUnderCPUEmulation();
            }
        }

#if USE_VIVOX
        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsReloaded()
        {
            if (Application.isBatchMode)
            {
                return;
            }

#if UNITY_EDITOR_OSX
            if (UserBuildSettings.architecture != MacOSArchitecture.x64) {
                if (DisplayDialog("Vivox Unsupported",
                        "Vivox is not supported on Apple Silicon architecture. However it will run with _X64 CPU emulation.\nSet the architecture to Intel 64-bit, or else, remove the USE_VIVOX define symbol in Player Settings",
                        "Set Architecture to Intel 64-bit",
                        "Disable Vivox"))
                {
                    UserBuildSettings.architecture = MacOSArchitecture.x64;
                }
                else
                {
                    DisableVivoxIntegration();
                }
            }

            if (IsSiliconEditor
                && DisplayDialog("Vivox Unsupported in Unity-Silicon",
                        "Vivox is not supported on MacOS Silicon version of Unity. \nTo use Vivox in the Editor (Playmode), please open the project with the Intel version, available in hub via the Unity version archive. We will now disable the USE_VIVOX define symbol.",
                        "Disable Vivox",
                        "Ok"))
            {
                DisableVivoxIntegration();
            }
#endif
            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.WebGL
                && DisplayDialog("Vivox Unsupported",
                        "Vivox is not supported on WebGL. Delete the USE_VIVOX define symbol in Player Settings",
                        "Disable Vivox",
                        "Ok"))
                {
                    DisableVivoxIntegration();
                }
        }
#endif
    }
}
