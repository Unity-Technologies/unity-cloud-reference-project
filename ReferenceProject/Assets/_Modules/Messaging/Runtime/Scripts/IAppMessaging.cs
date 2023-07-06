using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Messaging
{
    public interface IAppMessaging
    {
        public void ShowMessage(string message, bool dismissable = false, params object[] args);
        public void ShowInfo(string message, bool dismissable = false, params object[] args);
        public void ShowSuccess(string message, bool dismissable = false, params object[] args);
        public void ShowWarning(string message, bool dismissable = false, params object[] args);
        public void ShowError(string message, bool dismissable = false, params object[] args);
        public void ShowException(Exception exception, string title = null, params object[] args);
        public void ShowDialog(string title, string message,
            string cancelButtonLabel, Action cancelCallback = null,
            string primaryActionLabel = null, Action primaryActionCallback = null,
            params object[] args);

        public Modal ShowCustomDialog(VisualElement content);
    }
}
