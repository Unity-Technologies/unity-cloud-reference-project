using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// Every possible reason why a Notification element can be dismissed.
    /// </summary>
    public enum DismissType
    {
        /// <summary>
        /// The Notification element has been dismissed via an action click.
        /// </summary>
        Action,

        /// <summary>
        /// The Notification element has been dismissed via an action click.
        /// </summary>
        Consecutive,

        /// <summary>
        /// The Notification element has been dismissed via a call of the method Dismiss().
        /// </summary>
        Manual,

        /// <summary>
        /// The Notification element has been dismissed via a time out.
        /// </summary>
        Timeout,

        /// <summary>
        /// The popup element has been dismissed via a click out of the element bounding box.
        /// </summary>
        OutOfBounds,

        /// <summary>
        /// The popup element has been dismissed via a Cancel action (Esc key, or Back button on mobile).
        /// </summary>
        Cancel,

        /// <summary>
        /// The popup element has been dismissed because the panel has been destroyed.
        /// </summary>
        PanelDestroyed
    }

    /// <summary>
    /// The duration for a Notification to be displayed on  the screen.
    /// </summary>
    public enum NotificationDuration
    {
        /// <summary>
        /// The Notification will be visible indefinitely. It will need to be dismissed manually.
        /// </summary>
        Indefinite = -1,

        /// <summary>
        /// The Notification will be visible for a short amount of time.
        /// </summary>
        Short = 1500,

        /// <summary>
        /// The Notification will be visible for a long amount of time.
        /// </summary>
        Long = 2750
    }

    /// <summary>
    /// The Notification Manager is in charge of displaying notifications in the right order at the right time.
    /// </summary>
    class NotificationManager
    {
        const int k_NotificationTimeout = 3;

        readonly Handler m_Handler;

        readonly object m_LockObj;

        NotificationRecord m_CurrentNotification;

        NotificationRecord m_NextNotification;

        /// <summary>
        /// Construct the manager using an <see cref="AppUIManager"/> instance.
        /// </summary>
        /// <param name="manager">The provided <see cref="AppUIManager"/>.</param>
        /// <exception cref="ArgumentNullException">The Application object is null.</exception>
        internal NotificationManager(AppUIManager manager)
        {
            var looper = manager?.mainLooper ?? throw new ArgumentNullException(nameof(manager));
            m_LockObj = new object();
            m_Handler = new Handler(looper, message =>
            {
                switch (message.what)
                {
                    case k_NotificationTimeout:
                        HandleTimeout((NotificationRecord)message.obj);
                        return true;
                    default:
                        return false;
                }
            });
        }

        internal void Show(NotificationDuration duration, ICallback callback)
        {
            lock (m_LockObj)
            {
                if (IsCurrentLocked(callback))
                {
                    m_CurrentNotification.duration = duration;
                    m_Handler.RemoveCallbacksAndMessages(m_CurrentNotification);
                    ScheduleTimeoutLocked(m_CurrentNotification);
                }
                else if (IsNextLocked(callback))
                {
                    m_NextNotification.duration = duration;
                }
                else
                {
                    m_NextNotification = new NotificationRecord(callback, duration);
                }

                if (m_CurrentNotification != null && CancelNotificationLocked(m_CurrentNotification, DismissType.Consecutive)) return;

                m_CurrentNotification = null;
                ShowNextNotificationLocked();
            }
        }

        internal void Dismiss(ICallback callback, DismissType dismissType)
        {
            lock (m_LockObj)
            {
                if (IsCurrentLocked(callback))
                    CancelNotificationLocked(m_CurrentNotification, dismissType);
                else if (IsNextLocked(callback)) CancelNotificationLocked(m_NextNotification, dismissType);
            }
        }

        internal void OnShown(ICallback callback)
        {
            lock (m_LockObj)
            {
                if (IsCurrentLocked(callback)) ScheduleTimeoutLocked(m_CurrentNotification);
            }
        }

        internal void OnDismissed(ICallback callback)
        {
            lock (m_LockObj)
            {
                if (IsCurrentLocked(callback))
                {
                    m_CurrentNotification = null;
                    if (m_NextNotification != null)
                        ShowNextNotificationLocked();
                }
            }
        }

        internal bool IsCurrent(ICallback callback)
        {
            lock (m_LockObj)
            {
                return IsCurrentLocked(callback);
            }
        }

        internal bool IsCurrentOrNext(ICallback callback)
        {
            lock (m_LockObj)
            {
                return IsCurrentLocked(callback) || IsNextLocked(callback);
            }
        }

        bool IsCurrentLocked(ICallback callback)
        {
            return callback != null && m_CurrentNotification != null && m_CurrentNotification.callback == callback;
        }

        bool IsNextLocked(ICallback callback)
        {
            return callback != null && m_NextNotification != null && m_NextNotification.callback == callback;
        }

        void ShowNextNotificationLocked()
        {
            if (m_NextNotification != null)
            {
                m_CurrentNotification = m_NextNotification;
                m_NextNotification = null;

                if (m_CurrentNotification.callback != null)
                    m_CurrentNotification.callback.Show();
                else
                    m_CurrentNotification = null;
            }
        }

        /// <summary>
        /// Cancel the current notification.
        /// </summary>
        /// <param name="reason">The reason for the cancellation.</param>
        /// <returns>True if the current notification was cancelled, false otherwise.</returns>
        public bool CancelCurrentNotification(DismissType reason)
        {
            lock (m_LockObj)
            {
                return CancelNotificationLocked(m_CurrentNotification, reason);
            }
        }

        bool CancelNotificationLocked(NotificationRecord notification, DismissType reason)
        {
            if (notification != null && notification.callback != null)
            {
                m_Handler.RemoveCallbacksAndMessages(notification);
                notification.callback.Dismiss(reason);
                return true;
            }

            return false;
        }

        void ScheduleTimeoutLocked(NotificationRecord notification)
        {
            if (notification.duration == NotificationDuration.Indefinite)
                return;

            var durationMs = (int)notification.duration;

            m_Handler.RemoveCallbacksAndMessages(notification);
            var message = m_Handler.ObtainMessage(k_NotificationTimeout, notification);
            m_Handler.SendMessageDelayed(message, durationMs);
        }

        void HandleTimeout(NotificationRecord notification)
        {
            lock (m_LockObj)
            {
                if (notification == m_CurrentNotification || notification == m_NextNotification) CancelNotificationLocked(notification, DismissType.Timeout);
            }
        }

        /// <summary>
        /// The interface that needs to be implemented to your Notification element class in order to call back methods for
        /// displaying and hiding the notification.
        /// </summary>
        internal interface ICallback
        {
            object obj { get; }

            void Show();

            void Dismiss(DismissType reason);
        }

        /// <summary>
        /// A notification record.
        /// </summary>
        class NotificationRecord
        {
            /// <summary>
            /// The callback for the notification.
            /// </summary>
            public readonly ICallback callback; // todo convert to WeakReference

            /// <summary>
            /// The duration of the notification.
            /// </summary>
            public NotificationDuration duration;

            /// <summary>
            /// Construct a notification record.
            /// </summary>
            /// <param name="callback">The callback for the notification.</param>
            /// <param name="duration">The duration of the notification.</param>
            public NotificationRecord(ICallback callback, NotificationDuration duration)
            {
                this.callback = callback;
                this.duration = duration;
            }
        }
    }
}
