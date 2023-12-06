using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Messaging
{
    public interface IAppMessaging
    {
        public void DismissToast(Toast toast);
        public void DismissModal(Modal modal);
        public Toast ShowMessage(string message, bool dismissable = false, params object[] args);
        public Toast ShowInfo(string message, bool dismissable = false, params object[] args);
        public Toast ShowSuccess(string message, bool dismissable = false, params object[] args);
        public Toast ShowWarning(string message, bool dismissable = false, params object[] args);
        public Toast ShowError(string message, bool dismissable = false, params object[] args);
        public void ShowException(Exception exception, string title = null, params object[] args);
        public void ShowDialog(string title, string message,
            string cancelButtonLabel, Action cancelCallback = null,
            string primaryActionLabel = null, Action primaryActionCallback = null,
            params object[] args);

        public Modal ShowCustomDialog(VisualElement content);
    }
}
