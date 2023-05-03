using UnityEditor;
using UnityEngine.Rendering;

namespace UnityEngine.Dt.App.Editor
{
    /// <summary>
    /// This class is used to add the App UI Shaders to the GraphicsSettings PreloadedShaders list.
    /// </summary>
    [InitializeOnLoad]
    public static class AppUISetup
    {
        static AppUISetup()
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var preloadedShadersProperty = serializedObject.FindProperty("m_PreloadedShaders");
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Packages/com.unity.dt.app-ui/PackageResources/Shaders/App UI Shaders.shadervariants");

            Utils.AddItemInArray(preloadedShadersProperty, collection);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
