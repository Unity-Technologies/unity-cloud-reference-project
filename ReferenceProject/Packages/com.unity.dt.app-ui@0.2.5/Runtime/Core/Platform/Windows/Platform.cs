#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityEngine.Dt.App.Core
{
    public static partial class Platform
    {
        const int MONITOR_DEFAULTTONULL       = 0x00000000;
        const int MONITOR_DEFAULTTOPRIMARY    = 0x00000001;
        const int MONITOR_DEFAULTTONEAREST    = 0x00000002;

        enum DPI_AWARENESS
        {
            DPI_AWARENESS_INVALID           = -1,
            DPI_AWARENESS_UNAWARE           = 0,
            DPI_AWARENESS_SYSTEM_AWARE      = 1,
            DPI_AWARENESS_PER_MONITOR_AWARE = 2
        }

        enum DEVICE_SCALE_FACTOR {
            DEVICE_SCALE_FACTOR_INVALID = 0,
            SCALE_100_PERCENT = 100,
            SCALE_120_PERCENT = 120,
            SCALE_125_PERCENT = 125,
            SCALE_140_PERCENT = 140,
            SCALE_150_PERCENT = 150,
            SCALE_160_PERCENT = 160,
            SCALE_175_PERCENT = 175,
            SCALE_180_PERCENT = 180,
            SCALE_200_PERCENT = 200,
            SCALE_225_PERCENT = 225,
            SCALE_250_PERCENT = 250,
            SCALE_300_PERCENT = 300,
            SCALE_350_PERCENT = 350,
            SCALE_400_PERCENT = 400,
            SCALE_450_PERCENT = 450,
            SCALE_500_PERCENT = 500
        } ;

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

        [DllImport("user32.dll")]
        static extern int GetWindowDpiAwarenessContext(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern DPI_AWARENESS GetAwarenessFromDpiAwarenessContext(int value);

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(IntPtr hWnd);

        [DllImport("AppUINativePlugin", CallingConvention = CallingConvention.Cdecl)]
        static extern float _WinRuntimeAppsGetMainScreenScale();

        [DllImport("AppUINativePlugin", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetScaleFactorForMonitorEx(IntPtr hMon);

        [DllImport("AppUINativePlugin", CallingConvention = CallingConvention.Cdecl)]
        static extern uint AppsUseLightTheme();

        [DllImport("AppUINativePlugin", CallingConvention = CallingConvention.Cdecl)]
        static extern uint SystemUsesLightTheme();

        internal static uint _WINUseLightTheme => SystemUsesLightTheme();

    }
}
#endif
