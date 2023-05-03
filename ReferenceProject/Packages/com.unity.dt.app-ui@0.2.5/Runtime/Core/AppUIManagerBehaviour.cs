using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// A <see cref="MonoBehaviour"/> which is responsible for updating the AppUI system every frame.
    /// <remarks>
    /// A single instance of this class should be present.
    /// </remarks>
    /// </summary>
    [DisallowMultipleComponent]
    public class AppUIManagerBehaviour : MonoBehaviour
    {
        void Update()
        {
            if (!Application.isEditor)
            {
                AppUI.EnsureInitialized();
                AppUI.Update();
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void OnAndroidNativeMessageReceived(string message)
        {
            Platform.HandleAndroidMessage(message);
        }
#endif
    }
}
