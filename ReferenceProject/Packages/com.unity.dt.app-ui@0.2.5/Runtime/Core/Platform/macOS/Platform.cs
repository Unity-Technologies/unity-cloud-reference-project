#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Dt.App.Core
{
    public static partial class Platform
    {
        [DllImport("AppUINativePlugin")]
        static extern float _NSAppUIScaleFactor();

        [DllImport("AppUINativePlugin")]
        static extern int _NSCurrentAppearance();
    }
}
#endif
