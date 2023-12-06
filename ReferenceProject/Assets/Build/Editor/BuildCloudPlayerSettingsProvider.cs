using System.IO;
using Unity.Cloud.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace  Unity.ReferenceProject.Editor
{
    public static class BuildCloudPlayerSettingsProvider
    {
            const string k_ResourcesDirectory = "Assets/Unity Cloud/Resources/";
            static readonly string k_AssetPath = $"{k_ResourcesDirectory}{UnityCloudPlayerSettings.k_AssetName}.asset";

            public static void CreateCloudPlayerSettings(string appId, string appName, string appDisplayName, string organizationId)
            {
                var settings = Resources.Load<UnityCloudPlayerSettings>(UnityCloudPlayerSettings.k_AssetName);

                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<UnityCloudPlayerSettings>();

                    if (!AssetDatabase.IsValidFolder(k_ResourcesDirectory))
                        Directory.CreateDirectory(k_ResourcesDirectory);

                    AssetDatabase.CreateAsset(settings, k_AssetPath);
                }

                EditorUtility.SetDirty(settings);

                UnityCloudPlayerSettings.Instance.AppId = appId;
                UnityCloudPlayerSettings.Instance.AppName = appName;
                UnityCloudPlayerSettings.Instance.AppDisplayName = appDisplayName;
                UnityCloudPlayerSettings.Instance.AppOrganizationID = organizationId;

                AssetDatabase.SaveAssetIfDirty(settings);
            }
    }
}
