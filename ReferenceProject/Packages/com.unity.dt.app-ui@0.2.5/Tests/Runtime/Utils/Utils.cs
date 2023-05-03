using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Dt.App.Tests
{
    static class Utils
    {
        static readonly MethodInfo k_ImportXmlFromString;

        static readonly object k_UxmlImporterImplInstance;

        static Utils()
        {
#if UNITY_EDITOR
            Type importerType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName == "UnityEditor.UIElements.UXMLImporterImpl")
                    {
                        importerType = type;
                        break;
                    }
                }

                if (importerType != null)
                    break;
            }

            if (importerType == null)
            {
                Debug.LogError("Could not find UXMLImporterImpl type");
            }
            else
            {
                k_UxmlImporterImplInstance = importerType.GetConstructors(
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .First(c => !c.GetParameters().Any())
                    .Invoke(Array.Empty<object>());

                k_ImportXmlFromString = importerType.GetMethod("ImportXmlFromString",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (k_ImportXmlFromString == null)
                    Debug.LogError("Could not find ImportXmlFromString method");
            }
#endif
        }

        internal static UIDocument ConstructTestUI()
        {
            var obj = new GameObject("TestUI");
            obj.AddComponent<Camera>();
            var doc = obj.AddComponent<UIDocument>();
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ConstantPhysicalSize;
#if UNITY_EDITOR
            panelSettings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss");
#endif
            doc.panelSettings = panelSettings;

            return doc;
        }

        internal static VisualTreeAsset LoadUxmlTemplateFromString(string contents)
        {
            // ReSharper disable once RedundantAssignment
            VisualTreeAsset vta = null;
#if UNITY_EDITOR
            var args = new object[] {contents, null};
            k_ImportXmlFromString.Invoke(k_UxmlImporterImplInstance, args);
            vta = args[1] as VisualTreeAsset;
#endif
            return vta;
        }
    }
}
