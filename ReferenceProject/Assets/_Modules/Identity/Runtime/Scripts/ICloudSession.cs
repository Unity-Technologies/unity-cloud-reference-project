using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.ReferenceProject.Identity
{
    public enum SessionState
    {
        LoggingIn,
        LoggedIn,
        LoggingOut,
        LoggedOut
    }

    public interface ICloudSession
    {
        void CancelLogin();
        Task Initialize();
        void RegisterLoggedInCallback(Func<Task> callback);

        void UnRegisterLoggedInCallback(Func<Task> callback);

        void RegisterLoggingOutCallback(Func<Task> callback);

        void UnRegisterLoggingOutCallback(Func<Task> callback);

        event Action<SessionState> SessionStateChanged;

        SessionState State { get; }
        bool Initialized { get; }
        IUserData UserData { get; }


        Task<bool> Logout();

        Task<bool> Login();
    }
}
