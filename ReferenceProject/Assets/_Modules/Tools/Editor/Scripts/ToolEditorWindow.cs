using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Tools.Editor
{
    public class ToolEditorWindow : EditorWindow
    {
        const string k_EditorWindowText = "Tool name: ";
        const string k_TemplatePath = "../Assets/_Modules/Tools/Editor/Templates";
        Sprite m_Icon;

        string m_InputText = "My Tool";

        void OnGUI()
        {
            m_InputText = EditorGUILayout.TextField(k_EditorWindowText, m_InputText);
            m_Icon = (Sprite)EditorGUILayout.ObjectField("Icon", m_Icon, typeof(Sprite), false);

            if (GUILayout.Button("Add"))
            {
                if (!string.IsNullOrEmpty(m_InputText))
                {
                    AddNewTool(m_InputText, m_Icon);
                }
                else
                {
                    Debug.LogError("New tool cannot have a empty name.");
                }

                Close();
            }
        }

        static void CreateNewToolWindow()
        {
            var window = CreateInstance<ToolEditorWindow>();
            window.ShowUtility();
        }

        void AddNewTool(string toolName, Sprite icon)
        {
            var trimToolName = toolName.Replace(" ", string.Empty);
            var mainPath = $"Assets/{trimToolName}";
            string filename;
            string currentPath;
            string fileText;

            // Create View script
            filename = $"{trimToolName}UIController.cs";
            currentPath = $"{mainPath}/Scripts/UIControllers/";
            fileText = File.ReadAllText($"{k_TemplatePath}/ToolUIControllerTemplate.txt");
            fileText = fileText.Replace("[TrimToolName]", trimToolName);
            CreateFile(currentPath, filename, fileText);

            // Create Panel content UI Document
            filename = $"{trimToolName}PanelContent.uxml";
            currentPath = $"{mainPath}/UIToolkit";
            fileText = File.ReadAllText($"{k_TemplatePath}/ToolPanelContentTemplate.txt");
            CreateFile(currentPath, filename, fileText);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var panelUIDocument = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{currentPath}/{filename}");

            // Create Prefab asset
            var go = new GameObject();
            go.name = trimToolName;
            var toolUIController = go.AddComponent<ToolUIController>();
            toolUIController.DisplayName = toolName;
            toolUIController.Icon = icon;
            toolUIController.Template = panelUIDocument;

            EditorPrefs.SetString("New Class Name", trimToolName);

            AssetDatabase.ImportAsset(mainPath);
        }

        static void CreateFile(string path, string filename, string fileText)
        {
            Directory.CreateDirectory(path);
            File.WriteAllText($"{path}/{filename}", fileText);
        }

        static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        [DidReloadScripts]
        static void ScriptReloaded()
        {
            if (!EditorPrefs.HasKey("New Class Name"))
            {
                return;
            }

            var name = EditorPrefs.GetString("New Class Name");
            var go = GameObject.Find(name);
            if (go == null)
            {
                return;
            }

            var typeName = $"Unity.ReferenceProject.{name}.UI.{name}UIController";
            go.AddComponent(GetType(typeName));

            var mainPath = $"Assets/{name}";
            var filename = $"{name}Prefab.prefab";
            var currentPath = $"{mainPath}/Prefabs";
            AssetDatabase.CreateFolder(mainPath, "Prefabs");
            PrefabUtility.SaveAsPrefabAsset(go, $"{currentPath}/{filename}");
            DestroyImmediate(go);

            EditorPrefs.DeleteKey("New Class Name");

            Debug.Log($"New files for {name} created successfully into {mainPath}.");
        }
    }
}
