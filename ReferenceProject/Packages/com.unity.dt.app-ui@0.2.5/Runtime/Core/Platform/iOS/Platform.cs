#if UNITY_IOS
using System.Runtime.InteropServices;

namespace UnityEngine.Dt.App.Core
{
    public static partial class Platform
    {
        [DllImport("__Internal")]
        static extern float _IOSAppUIScaleFactor();

        [DllImport("__Internal")]
        static extern void _IOSRunHapticFeedback(int feedbackType);

        [DllImport("__Internal")]
        static extern int _IOSCurrentAppearance();
    }
}
#endif
