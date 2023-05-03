using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// The type of haptic feedback to trigger.
    /// </summary>
    public enum HapticFeedbackType
    {
        /// <summary>
        /// No haptic feedback will be triggered with this value.
        /// </summary>
        UNDEFINED = 0,
        /// <summary>
        /// A light haptic feedback.
        /// </summary>
        LIGHT = 1,
        /// <summary>
        /// A medium haptic feedback.
        /// </summary>
        MEDIUM,
        /// <summary>
        /// A heavy haptic feedback.
        /// </summary>
        HEAVY,
        /// <summary>
        /// A success haptic feedback.
        /// </summary>
        SUCCESS,
        /// <summary>
        /// An error haptic feedback.
        /// </summary>
        ERROR,
        /// <summary>
        /// A warning haptic feedback.
        /// </summary>
        WARNING,
        /// <summary>
        /// A selection haptic feedback.
        /// </summary>
        SELECTION
    }

    /// <summary>
    /// Utility methods and properties related to the Target Platform.
    /// </summary>
    public static partial class Platform
    {
        const float k_BaseDpi = 96f;

        /// <summary>
        /// The DPI value that should be used in UI-Toolkit PanelSettings
        /// <see cref="UnityEngine.UIElements.PanelSettings.referenceDpi"/>.
        /// <para>
        /// This value is the value of <see cref="Screen.dpi"/> divided by the main screen scale factor.
        /// </para>
        /// </summary>
        public static float referenceDpi
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // On Windows we can use a value of 96dpi because UI Toolkit scales correctly the UI based on
            // Operating System's DPI and ScaleFactor changes.
            return k_BaseDpi;
#else
                return Screen.dpi / mainScreenScale;
#endif
            }
        }

        /// <summary>
        /// The main screen scale factor.
        /// <remarks>
        /// The "main" screen is the current screen used at highest priority to display the application window.
        /// </remarks>
        /// </summary>
        public static float mainScreenScale
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
            return _IOSAppUIScaleFactor();
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return _NSAppUIScaleFactor();
#elif UNITY_ANDROID && !UNITY_EDITOR
            // Android ScaledDensity: https://developer.android.com/reference/android/util/DisplayMetrics#scaledDensity
            return AndroidAppUI.scaledDensity;
#else
                // On Windows Screen.dpi is already the result of the dpi multiplied by the scale factor.
                // For example on a 27in 4k monitor at 100% scale, the dpi is 96 (really small UI), but the recommended
                // Scale factor with a 4k monitor is 150%, which gives 96 * 1.5 = 144dpi.
                // Unity Engine sets the DPI awareness per monitor, so the UI will scale automatically :
                // https://docs.microsoft.com/en-us/windows/win32/api/windef/ne-windef-dpi_awareness
                return Screen.dpi / k_BaseDpi;
#endif
            }
        }

        static event Action<string> s_SystemThemeChangedInternal;

        /// <summary>
        /// Event triggered when the system theme changes.
        /// </summary>
        public static event Action<string> systemThemeChanged
        {
            add
            {
                s_SystemThemeChangedInternal += value;
                if (value != null)
                    EnableThemePolling();
            }
            remove
            {
                s_SystemThemeChangedInternal -= value;
                if (s_SystemThemeChangedInternal == null)
                    DisableThemePolling();
            }
        }

        static bool s_SystemThemePollingEnabled = false;

        static void EnableThemePolling()
        {
            s_SystemThemePollingEnabled = true;
        }

        static void DisableThemePolling()
        {
            s_SystemThemePollingEnabled = false;
        }

        static string s_PreviousSystemTheme;

        static int s_ThemePollingDelta = 0;

        /// <summary>
        /// Polls the system theme and triggers the <see cref="systemThemeChanged"/> event if the theme has changed.
        /// </summary>
        internal static void PollSystemTheme()
        {
            if (!s_SystemThemePollingEnabled)
                return;

            s_ThemePollingDelta += Time.frameCount;

            if (s_ThemePollingDelta < 60)
                return;

            s_ThemePollingDelta = 0;
            var currentTheme = systemTheme;
            if (currentTheme != s_PreviousSystemTheme)
            {
                s_SystemThemeChangedInternal?.Invoke(currentTheme);
                s_PreviousSystemTheme = currentTheme;
            }
        }

        /// <summary>
        /// The current system theme.
        /// </summary>
        public static string systemTheme
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return _IOSCurrentAppearance() == 2 ? "dark" : "light";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return _NSCurrentAppearance() == 2 ? "dark" : "light";
#elif UNITY_ANDROID && !UNITY_EDITOR
                return AndroidAppUI.isNightModeDefined && AndroidAppUI.isNightModeEnabled ? "dark" : "light";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return _WINUseLightTheme == 1 ? "light" : "dark";
#else
                return "dark";
#endif
            }
        }

        /// <summary>
        /// Run a haptic feedback on the current platform.
        /// </summary>
        /// <param name="feedbackType">The type of haptic feedback to trigger.</param>
        public static void RunHapticFeedback(HapticFeedbackType feedbackType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _IOSRunHapticFeedback((int)feedbackType);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidAppUI.RunHapticFeedback(feedbackType);
#else
            if (Application.isEditor)
                Debug.LogWarning("Haptic Feedbacks are not supported in the Editor.");
            else
                Debug.LogWarning("Haptic Feedbacks are not supported on the current platform.");
#endif
        }
    }
}
