#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace UnityEngine.Dt.App.Core
{
    public static partial class Platform
    {
        internal static void HandleAndroidMessage(string message)
        {
            if (message == "configurationChanged")
                AndroidAppUI.QueryConfiguration();
        }

        static class AndroidAppUI
        {
            static AndroidJavaObject s_AppUiActivity;
            
            static bool s_IsCustomAppUiActivity;

            static AndroidAppUI()
            {
                if (Application.platform != RuntimePlatform.Android || Application.isEditor)
                    return;

                using var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                s_AppUiActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                
                // check if the app is running with our custom AppUIActivity
                s_IsCustomAppUiActivity = true;
                try 
                {
                    s_AppUiActivity.Call<float>("ScaledDensity");
                } 
                catch (System.Exception e) 
                {
                    s_IsCustomAppUiActivity = false;
                    Debug.LogWarning("This Android app is not running with our custom AppUIActivity. " +
                        "Some features will not work as expected. " +
                        "Please make sure you checked the 'Override Android manifest' option in App UI Settings.");
                }

                QueryConfiguration();
            }

            internal static float scaledDensity { get; private set; } = 1f;

            internal static float density { get; private set; } = 1f;

            internal static int densityDPI { get; private set; } = 600;

            internal static float fontScale { get; private set; } = 1f;

            internal static bool isNightModeEnabled { get; private set; } = false;

            internal static bool isNightModeDefined { get; private set; } = false;

            internal static void RunHapticFeedback(HapticFeedbackType feedbackType)
            {
                if (!s_IsCustomAppUiActivity)
                    return;
                
                s_AppUiActivity.Call("RunHapticFeedback", (int)feedbackType);
            }

            internal static void QueryConfiguration()
            {
                if (!s_IsCustomAppUiActivity)
                    return;
                
                scaledDensity = s_AppUiActivity.Call<float>("ScaledDensity");
                density = s_AppUiActivity.Call<float>("Density");
                densityDPI = s_AppUiActivity.Call<int>("DensityDpi");
                fontScale = s_AppUiActivity.Call<float>("FontScale");
                isNightModeEnabled = s_AppUiActivity.Call<bool>("IsNightModeEnabled");
                isNightModeDefined = s_AppUiActivity.Call<bool>("IsNightModeDefined");
            }
        }
    }
}
#endif
