using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.ReferenceProject.Tools.Editor
{
    public class ToolEditorUtils
    {
        const string k_InternalSourcePath = "Assets/_Modules/Tools/Runtime";
        const string k_SourcePath = "../" + k_InternalSourcePath;
        const string k_DestinationPath = "Assets/ToolUIManager";

        static void AddToolUIManager()
        {
            var toolUIManager = Object.FindObjectOfType<ToolUIMenu>();
            if (toolUIManager != null)
            {
                Debug.LogError("ToolUIMenu already created");
                return;
            }

            var go = new GameObject
            {
                name = "ToolUIMenu"
            };
            var uiDocument = go.AddComponent<UIDocument>();
            go.AddComponent<ToolUIMenu>();

            var srcPath = $"{k_SourcePath}/UIToolkit";
            var uiToolkitPath = $"{k_DestinationPath}/UIToolkit";
            var panelSettingsFilename = "Main Panel Settings.asset";
            CopyFiles(panelSettingsFilename, srcPath, uiToolkitPath);

            var visualTreeFilename = "MainTree.uxml";
            CopyFiles(visualTreeFilename, srcPath, uiToolkitPath);

            srcPath = $"{k_SourcePath}/Data";
            var dataPath = $"{k_DestinationPath}/Data";
            var toolbarDataFilename = "ToolbarDataList.asset";
            CopyFiles(toolbarDataFilename, srcPath, dataPath);
            var mainTreeNamingFilename = "MainTreeNaming.asset";
            CopyFiles(mainTreeNamingFilename, srcPath, dataPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>($"{uiToolkitPath}/{panelSettingsFilename}");
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{uiToolkitPath}/{visualTreeFilename}");

            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;
        }

        static void CopyFiles(string filename, string srcPath, string destPath)
        {
            Directory.CreateDirectory(destPath);
            File.Copy($"{srcPath}/{filename}", $"{destPath}/{filename}", true);
        }
    }
}
