using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.ReferenceProject.InputDisabling;
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
        IInputDisablingManager m_InputDisablingManager;

        DockedPanelController m_ToastDockedPanel;
        readonly Dictionary<Toast, DockedPanelController> m_ToastToPanel = new();

        DockedPanelController m_ModalDockedPanel;
        readonly Dictionary<Modal, DockedPanelController> m_ModalToPanel = new();

        static readonly Vector2 k_ToastSize = new(700, 60);
        static readonly Vector2 k_ModalSize = new(640, 480);

        [Inject]
        void Setup(IPanelManager panelManager, IRigUIController rigUIController, IInputDisablingManager inputDisablingManager)
        {
            m_PanelManager = panelManager;
            m_RigUIController = rigUIController;
            m_InputDisablingManager = inputDisablingManager;
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

        public override void ShowException(Exception exception, string title = null, params object[] args)
        {
            ShowModalException(exception, title, args);
        }

        public override void ShowDialog(string title, string message, string cancelButtonLabel, Action cancelCallback = null,
            string primaryActionLabel = null, Action primaryActionCallback = null, params object[] args)
        {
            ShowModalMessage(title, message, cancelButtonLabel, cancelCallback, primaryActionLabel, primaryActionCallback, args);
        }

        public void ShowModalMessage(string title, string message, string cancelButtonLabel, Action cancelCallback, string primaryActionLabel = null,
            Action primaryActionCallback = null, params object[] args)
        {
            m_PanelManager.BlockPanels();
            m_ModalDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ModalSize);
            m_ModalDockedPanel.name = "ModalMessagePanel";
            m_ModalDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ModalDockedPanel.transform.localPosition += (k_ModalSize.y / 2f) / 1000f * Vector3.up - 0.01f * Vector3.forward;
            DisplayModalPanel(m_ModalDockedPanel.UIDocument, new ModalMessage(title, message, cancelButtonLabel, cancelCallback, primaryActionLabel, primaryActionCallback, args));
        }

        public override Modal ShowCustomDialog(VisualElement content)
        {
            m_PanelManager.BlockPanels();
            m_ModalDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ModalSize);
            m_ModalDockedPanel.name = "ModalCustomPanel";
            m_ModalDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ModalDockedPanel.transform.localPosition += (k_ModalSize.y / 2f) / 1000f * Vector3.up - 0.01f * Vector3.forward;
            return DisplayCustomModalPanel(m_ModalDockedPanel.UIDocument, content);
        }

        void ShowModalException(Exception exception, string message, params object[] args)
        {
            m_PanelManager.BlockPanels();
            m_ModalDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ModalSize);
            m_ModalDockedPanel.name = "ModalMessagePanel";
            m_ModalDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ModalDockedPanel.transform.localPosition += (k_ModalSize.y / 2f) / 1000f * Vector3.up - 0.01f * Vector3.forward;
            DisplayModalExceptionPanel(m_ModalDockedPanel.UIDocument, new ModalException(exception, message, args));
        }

        void ShowToastMessage(string message, NotificationStyle style, NotificationDuration duration, bool dismissable = false, params object[] args)
        {
            m_ToastDockedPanel = m_PanelManager.CreatePanel<DockedPanelController>(k_ToastSize);
            m_ToastDockedPanel.name = "ToastMessagePanel";
            m_ToastDockedPanel.DockPoint = m_RigUIController.PermanentDockPoint;
            m_ToastDockedPanel.transform.localPosition += (-1 * k_ToastSize.y) / 1000f * Vector3.up - 0.01f * Vector3.forward;
            DisplayToastPanel(m_ToastDockedPanel.UIDocument, new ToastMessage(message, style, duration, dismissable, args));
        }

        void DisplayToastPanel(UIDocument document, ToastMessage toastMessage)
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
                    m_PanelManager.DestroyPanel(panel);
                    m_ToastToPanel.Remove(t);
                }
            };

            toast.Show();
        }

        void DisplayModalPanel(UIDocument document, ModalMessage modalMessage)
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

            m_InputDisablingManager.AddOverride(this);

            modal.dismissed += (m, type) =>
            {
                m_InputDisablingManager.RemoveOverride(this);
                m_PanelManager.BlockPanels(block:false);

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
                    m_PanelManager.DestroyPanel(panel);
                    m_ModalToPanel.Remove(m);
                }
            };

            modal.Show();
        }

        void DisplayModalExceptionPanel(UIDocument document, ModalException modalException)
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
                m_PanelManager.BlockPanels(block:false);
                if (type == DismissType.PanelDestroyed)
                {
                    // Show message again (this happens when the panel is re parented)
                    ShowModalException(exception, message, args);
                }

                if (m_ModalToPanel.TryGetValue(m, out var panel))
                {
                    m_PanelManager.DestroyPanel(panel);
                    m_ModalToPanel.Remove(m);
                }
            };

            modal.Show();
        }

        Modal DisplayCustomModalPanel(UIDocument uiDocument, VisualElement content)
        {
            m_InputDisablingManager.AddOverride(this);
            var panel = uiDocument.rootVisualElement.Q<Panel>();
            var modal = BuildCustomDialog(panel, content);
            modal.view.style.backgroundColor = Color.clear;

            m_ModalToPanel.Add(modal, m_ModalDockedPanel);

            modal.dismissed += (m, type) =>
            {
                m_PanelManager.BlockPanels(block:false);
                if (type == DismissType.PanelDestroyed)
                {
                    // Show message again (this happens when the panel is re parented)
                    ShowCustomDialog(content);
                }

                if (m_ModalToPanel.TryGetValue(m, out var panel))
                {
                    m_PanelManager.DestroyPanel(panel);
                    m_ModalToPanel.Remove(m);
                }
            };
            modal.Show();
            return modal;
        }
    }
}
