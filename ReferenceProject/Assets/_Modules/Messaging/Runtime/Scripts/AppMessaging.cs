using System;
using System.Text;
using Unity.ReferenceProject.UIPanel;
using UnityEngine;
using UnityEngine.Dt.App.Core;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Messaging
{
    public class AppMessaging : IAppMessaging
    {
        public virtual void ShowMessage(string message, bool dismissable = false, params object[] args)
        {
            BuildToastMessage(MainUIPanel.Instance.Panel, message, NotificationStyle.Default, NotificationDuration.Long, dismissable, args).Show();
        }

        public virtual void ShowInfo(string message, bool dismissable = false, params object[] args)
        {
            BuildToastMessage(MainUIPanel.Instance.Panel, message, NotificationStyle.Informative, NotificationDuration.Long, dismissable, args).Show();
        }

        public virtual void ShowSuccess(string message, bool dismissable = false, params object[] args)
        {
            BuildToastMessage(MainUIPanel.Instance.Panel, message, NotificationStyle.Positive, NotificationDuration.Long, dismissable, args).Show();
        }

        public virtual void ShowWarning(string message, bool dismissable = false, params object[] args)
        {
            BuildToastMessage(MainUIPanel.Instance.Panel, message, NotificationStyle.Warning, NotificationDuration.Long, dismissable, args).Show();
        }

        public virtual void ShowError(string message, bool dismissable = false, params object[] args)
        {
            BuildToastMessage(MainUIPanel.Instance.Panel, message, NotificationStyle.Negative, NotificationDuration.Long, dismissable, args).Show();
        }

        public virtual void ShowException(Exception exception, string message = null, params object[] args)
        {
            BuildExceptionDialog(MainUIPanel.Instance.Panel, exception, message, args).Show();
        }

        public virtual void ShowDialog(string title, string message, string cancelButtonLabel, Action cancelCallback = null,
            string primaryActionLabel = null, Action primaryActionCallback = null, params object[] args)
        {
            var modal = BuildDialog(MainUIPanel.Instance.Panel, title, message, cancelButtonLabel, primaryActionLabel, primaryActionCallback);
            modal.dismissed += (t, type) => cancelCallback?.Invoke();
            modal.Show();
        }

        protected static Modal BuildDialog(VisualElement panel, string title, string message, string cancelButtonLabel,
            string primaryActionLabel = null, Action primaryActionCallback = null,
            params object[] args)
        {
            var dialog = new AlertDialog
            {
                title = title,
                description = message
            };

            dialog.SetCancelAction(0, cancelButtonLabel);

            if (!string.IsNullOrEmpty(primaryActionLabel))
            {
                dialog.SetPrimaryAction(1, primaryActionLabel, primaryActionCallback);
            }

            var modal = Modal.Build(panel, dialog);

            if (args is { Length: > 0 })
            {
                modal.view.Q<LocalizedTextElement>("appui-dialog__content").variables = args;
            }

            return modal;
        }

        protected static Toast BuildToastMessage(VisualElement panel, string message, NotificationStyle style, NotificationDuration duration, bool dismissable, params object[] args)
        {
            Debug.Log(message);

            var toast = Toast.Build(panel, message, dismissable ? NotificationDuration.Indefinite : duration)
                .SetStyle(style);

            if (dismissable)
            {
                toast.SetAction(-1, "@ReferenceProject:Dismiss", () => { });
            }

            if (args is { Length: > 0 })
            {
                toast.view.Q<LocalizedTextElement>("appui-toast__message").variables = args;
            }

            return toast;
        }

        protected static Modal BuildExceptionDialog(VisualElement panel, Exception exception, string message = null, params object[] args)
        {
            Debug.LogException(exception);

            var dialog = new Dialog
            {
                title = "@ReferenceProject:ExceptionTitle",
                description = ConstructExceptionMessage(exception),
                dismissable = true
            };

            var modal = Modal.Build(panel, dialog);

            var content = modal.view.Q<LocalizedTextElement>("appui-dialog__content");

            if (args is { Length: > 0 })
            {
                content.variables = args;
            }

            return modal;
        }

        static string ConstructExceptionMessage(Exception exception)
        {
            if (exception == null)
                return null;

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(exception.GetType().Name);
            stringBuilder.AppendLine(exception.Message);

            if (exception.InnerException != null)
            {
                stringBuilder.AppendLine("--");
                stringBuilder.AppendLine(exception.InnerException.GetType().Name);
                stringBuilder.AppendLine(exception.InnerException.Message);
            }

            return stringBuilder.ToString();
        }
    }
}
