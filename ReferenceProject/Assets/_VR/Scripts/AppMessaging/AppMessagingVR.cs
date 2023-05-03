using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.VRManager;
using UnityEngine;
using UnityEngine.Dt.App.Core;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Zenject;
using Object = UnityEngine.Object;

namespace Unity.ReferenceProject.VR
{
    public class AppMessagingVR : AppMessaging
    {
        struct ToastMessage
        {
            public readonly string Message;
            public readonly NotificationStyle Style;
            public readonly NotificationDuration Duration;
            public readonly bool Dismissable;
            public readonly object[] Args;

            public ToastMessage(string message, NotificationStyle style, NotificationDuration duration, bool dismissable, object[] args)
            {
                Message = message;
                Style = style;
                Duration = duration;
                Dismissable = dismissable;
                Args = args;
            }
        }

        struct ModalMessage
        {
            public readonly string Title;
            public readonly string Message;
            public readonly string CancelButtonLabel;
            public readonly Action CancelCallback;
            public readonly string PrimaryActionLabel;
            public readonly Action PrimaryActionCallback;
            public readonly object[] Args;

            public ModalMessage(string title, string message, string cancelButtonLabel, Action cancelCallback, string primaryActionLabel, Action primaryActionCallback, object[] args)
            {
                Title = title;
                Message = message;
                CancelButtonLabel = cancelButtonLabel;
                CancelCallback = cancelCallback;
                PrimaryActionLabel = primaryActionLabel;
                PrimaryActionCallback = primaryActionCallback;
                Args = args;
            }
        }

        struct ModalException
        {
            public readonly Exception Exception;
            public readonly string Message;
            public readonly object[] Args;

            public ModalException(Exception exception, string message, object[] args)
            {
                Exception = exception;
                Message = message;
                Args = args;
            }
        }

        IPanelManager m_PanelManager;
        IRigUIController m_RigUIController;

        DockedPanelController m_ToastDockedPanel;
        readonly Dictionary<Toast, DockedPanelController> m_ToastToPanel = new();

        DockedPanelController m_ModalDockedPanel;
        readonly Dictionary<Modal, DockedPanelController> m_ModalToPanel = new();

        static readonly Vector2 k_ToastSize = new(700, 60);
        static readonly Vector2 k_ModalSize = new(640, 480);

        [Inject]
        void Setup(IPanelManager panelManager, IRigUIController rigUIController)
        {
            m_PanelManager = panelManager;
            m_RigUIController = rigUIController;
        }

        public override void ShowMessage(string message, bool dismissable = false, params object[] args)
        {
            ShowToastMessage(message, NotificationStyle.Default, NotificationDuration.Long, dismissable, args);
        }

        public override void ShowInfo(string message, bool dismissable = false, params object[] args)
        {
            ShowToastMessage(message, NotificationStyle.Informative, NotificationDuration.Long, dismissable, args);
        }

        public override void ShowSuccess(string message, bool dismissable = false, params object[] args)
        {
            ShowToastMessage(message, NotificationStyle.Positive, NotificationDuration.Long, dismissable, args);
        }

        public override void ShowWarning(string message, bool dismissable = false, params object[] args)
        {
            ShowToastMessage(message, NotificationStyle.Warning, NotificationDuration.Long, dismissable, args);
        }

        public override void ShowError(string message, bool dismissable = false, params object[] args)
        {
            ShowToastMessage(message, NotificationStyle.Negative, NotificationDuration.Long, dismissable, args);
        }

        public override void ShowException(Exception exception, string message = null, params object[] args)
        {
            ShowModalException(exception, message, args);
        }

        public override void ShowDialog(string title, string message, string cancelButtonLabel, Action cancelCallback = null,
            string primaryActionLabel = null, Action primaryActionCallback = null, params object[] args)
        {
            ShowModalMessage(title, message, cancelButtonLabel, cancelCallback, primaryActionLabel, primaryActionCallback, args);
        }

        public void ShowModalMessage(string title, string message, string cancelButtonLabel, Action cancelCallback, string primaryActionLabel = null,
            Action primaryActionCallback = null, params object[] args)
        {
            m_ModalDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ModalSize);
            m_ModalDockedPanel.name = "ModalMessagePanel";
            m_ModalDockedPanel.WorldSpaceUIDocument.OnPanelBuilt += document =>
            {
                OnModalPanelBuilt(document, new ModalMessage(title, message, cancelButtonLabel, cancelCallback, primaryActionLabel, primaryActionCallback, args));
            };
            m_ModalDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ModalDockedPanel.transform.localPosition += (k_ModalSize.y / 2f) / 1000f * Vector3.up - 0.01f * Vector3.forward;
        }

        void ShowModalException(Exception exception, string message, params object[] args)
        {
            m_ModalDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ModalSize);
            m_ModalDockedPanel.name = "ModalMessagePanel";
            m_ModalDockedPanel.WorldSpaceUIDocument.OnPanelBuilt += document =>
            {
                OnModalExceptionPanelBuilt(document, new ModalException(exception, message, args));
            };
            m_ModalDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ModalDockedPanel.transform.localPosition += (k_ModalSize.y / 2f) / 1000f * Vector3.up - 0.01f * Vector3.forward;
        }

        void ShowToastMessage(string message, NotificationStyle style, NotificationDuration duration, bool dismissable = false, params object[] args)
        {
            m_ToastDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ToastSize);
            m_ToastDockedPanel.name = "ToastMessagePanel";
            m_ToastDockedPanel.WorldSpaceUIDocument.OnPanelBuilt += document =>
            {
                OnToastPanelBuilt(document, new ToastMessage(message, style, duration, dismissable, args));
            };
            m_ToastDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ToastDockedPanel.transform.localPosition += (-1 * k_ToastSize.y) / 1000f * Vector3.up - 0.01f * Vector3.forward;
        }

        void OnToastPanelBuilt(UIDocument document, ToastMessage toastMessage)
        {
            var panel = document.rootVisualElement.Q<Panel>();

            var message = toastMessage.Message;
            var style = toastMessage.Style;
            var duration = toastMessage.Duration;
            var dismissable = toastMessage.Dismissable;
            var args = toastMessage.Args;

            var toast = BuildToastMessage(panel, message, style, duration, dismissable);

            m_ToastToPanel.Add(toast, m_ToastDockedPanel);

            if (dismissable)
            {
                toast.SetAction(-1, "@ReferenceProject:Dismiss", () => { });
            }

            toast.dismissed += (t, type) =>
            {
                if (type == DismissType.PanelDestroyed)
                {
                    // Show message again (this happens when the panel is re parented)
                    ShowToastMessage(message, style, duration, dismissable, args);
                }

                if (m_ToastToPanel.TryGetValue(t, out var panel))
                {
                    Object.Destroy(panel.gameObject);
                    m_ToastToPanel.Remove(t);
                }
            };

            toast.Show();
        }

        void OnModalPanelBuilt(UIDocument document, ModalMessage modalMessage)
        {
            var panel = document.rootVisualElement.Q<Panel>();

            var title = modalMessage.Title;
            var message = modalMessage.Message;
            var cancelLabel = modalMessage.CancelButtonLabel;
            var cancelCallback = modalMessage.CancelCallback;
            var primaryLabel = modalMessage.PrimaryActionLabel;
            var primaryAction = modalMessage.PrimaryActionCallback;
            var args = modalMessage.Args;

            var modal = BuildDialog(panel, title, message, cancelLabel, primaryLabel, primaryAction);
            modal.view.style.backgroundColor = Color.clear;

            m_ModalToPanel.Add(modal, m_ModalDockedPanel);

            modal.dismissed += (m, type) =>
            {
                if (type == DismissType.PanelDestroyed)
                {
                    // Show message again (this happens when the panel is re parented)
                    ShowModalMessage(title, message, cancelLabel, cancelCallback, primaryLabel, primaryAction, args);
                }
                else
                {
                    cancelCallback?.Invoke();
                }

                if (m_ModalToPanel.TryGetValue(m, out var panel))
                {
                    Object.Destroy(panel.gameObject);
                    m_ModalToPanel.Remove(m);
                }
            };

            modal.Show();
        }

        void OnModalExceptionPanelBuilt(UIDocument document, ModalException modalException)
        {
            var panel = document.rootVisualElement.Q<Panel>();

            var exception = modalException.Exception;
            var message = modalException.Message;
            var args = modalException.Args;

            var modal = BuildExceptionDialog(panel, exception, message, args);
            modal.view.style.backgroundColor = Color.clear;

            m_ModalToPanel.Add(modal, m_ModalDockedPanel);

            modal.dismissed += (m, type) =>
            {
                if (type == DismissType.PanelDestroyed)
                {
                    // Show message again (this happens when the panel is re parented)
                    ShowModalException(exception, message, args);
                }

                if (m_ModalToPanel.TryGetValue(m, out var panel))
                {
                    Object.Destroy(panel.gameObject);
                    m_ModalToPanel.Remove(m);
                }
            };

            modal.Show();
        }
    }
}
