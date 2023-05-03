using System;
using System.Collections.Generic;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The style used to display the Notification element.
    /// </summary>
    public enum NotificationStyle
    {
        /// <summary>
        /// Default style. This is for general purpose notification.
        /// </summary>
        Default,

        /// <summary>
        /// Informative style. This style is used to display an informative message.
        /// </summary>
        Informative,

        /// <summary>
        /// Positive style. This style is used to display a success message.
        /// </summary>
        Positive,

        /// <summary>
        /// Negative style. This style is used to display an error message.
        /// </summary>
        Negative,

        /// <summary>
        /// Warning style. This style is used to display a warning message.
        /// </summary>
        Warning
    }

    /// <summary>
    /// A toast is a view containing a quick little message for the user.
    /// </summary>
    public sealed class Toast : BottomNotification<Toast>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="contentView">The content inside the popup.</param>
        Toast(VisualElement parentView, ApplicationContext context, ToastVisualElement contentView)
            : base(parentView, context, contentView)
        {
            contentView.actionClicked += OnToastActionClicked;
        }

        /// <summary>
        /// The Action ID of the triggered Action (if any) just before dismissing the element.
        /// </summary>
        public int triggeredActionId { get; private set; } = -1;

        ToastVisualElement toast => (ToastVisualElement)view;

        /// <summary>
        /// Returns the styling used by the bar. See <see cref="NotificationStyle"/> for more information.
        /// </summary>
        public NotificationStyle style => toast.notificationStyle;

        /// <summary>
        /// Returns the raw message or Localization dictionary key used by the bar.
        /// </summary>
        public string text => toast.text;

        void OnToastActionClicked(int actionId)
        {
            triggeredActionId = actionId;
            Dismiss(DismissType.Action);
        }

        /// <summary>
        /// Set an Action to display in the Toast bar.
        /// </summary>
        /// <param name="actionId">The Action ID</param>
        /// <param name="msg"> The raw message or Localization dictionary key for the message to be displayed.</param>
        /// <param name="callback">The Action callback.</param>
        /// <returns>The current toast.</returns>
        public Toast SetAction(int actionId, string msg, Action callback)
        {
            toast.SetAction(actionId, msg, callback);
            return this;
        }

        /// <summary>
        /// Remove an already existing action.
        /// </summary>
        /// <param name="actionId">The Action ID.</param>
        /// <returns>The <see cref="Toast"/> instance, if no exception has occured.</returns>
        public Toast RemoveAction(int actionId)
        {
            toast.RemoveAction(actionId);
            return this;
        }

        /// <summary>
        /// Build and return a <see cref="Toast"/> UI element.
        /// <para>
        /// The method will find the best suitable parent view which will contain the Toast element.
        /// </para>
        /// </summary>
        /// <remarks>The snackbar is not displayed directly, you have to call <see cref="BottomNotification{TValueType}.Show"/>.</remarks>
        /// <param name="referenceView">An arbitrary <see cref="VisualElement"/> which is currently present in the UI panel.</param>
        /// <param name="text">The raw message or Localization dictionary key for the message to be displayed inside
        /// the <see cref="Toast"/>.</param>
        /// <param name="duration"></param>
        /// <returns>The <see cref="Toast"/> instance, if no exception has occured.</returns>
        /// <exception cref="ArgumentException">The provided view is not contained in a valid UI panel.</exception>
        public static Toast Build(VisualElement referenceView, string text, NotificationDuration duration)
        {
            var context = referenceView.GetContext();
            var parentView = context.panel.notificationContainer;
            var bar = new Toast(parentView, context, new ToastVisualElement()).SetText(text).SetDuration(duration);
            return bar;
        }

        /// <summary>
        /// The icon used inside the Toast as leading UI element.
        /// </summary>
        public string icon => toast.icon;

        /// <summary>
        /// Set a new value for the <see cref="icon"/> property.
        /// </summary>
        /// <param name="iconName">The name of the icon.</param>
        /// <returns>The <see cref="Toast"/> to continuously build the element.</returns>
        public Toast SetIcon(string iconName)
        {
            toast.icon = iconName;
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="notificationStyle"></param>
        /// <returns>The <see cref="Toast"/> to continuously build the element.</returns>
        public Toast SetStyle(NotificationStyle notificationStyle)
        {
            toast.notificationStyle = notificationStyle;
            return this;
        }

        /// <summary>
        /// Update the text in the <see cref="Toast"/>.
        /// </summary>
        /// <param name="txt"> The raw message or Localization dictionary key for the message to be displayed.</param>
        /// <returns>The <see cref="Toast"/> to continuously build the element.</returns>
        public Toast SetText(string txt)
        {
            toast.text = txt;
            return this;
        }

        /// <summary>
        /// The Toast UI Element.
        /// </summary>
        sealed class ToastVisualElement : ExVisualElement
        {
            public static readonly string ussClassName = "appui-toast";

            public static readonly string messageUssClassName = ussClassName + "__message";

            public static readonly string iconUssClassName = ussClassName + "__icon";

            public static readonly string dividerUssClassName = ussClassName + "__divider";

            public static readonly string actionContainerUssClassName = ussClassName + "__actioncontainer";

            public static readonly string actionUssClassName = ussClassName + "__action";

            readonly VisualElement m_ActionContainer;

            readonly Dictionary<int, ActionItem> m_Actions = new Dictionary<int, ActionItem>();

            readonly Divider m_Divider;

            readonly Icon m_Icon;

            NotificationStyle m_Style;

            readonly LocalizedTextElement m_TextElement;

            public ToastVisualElement()
            {
                AddToClassList(ussClassName);

                passMask = Passes.Clear | Passes.OutsetShadows;

                style.position = Position.Absolute;
                style.bottom = 0;

                m_Icon = new Icon { name = iconUssClassName };
                m_Icon.AddToClassList(iconUssClassName);
                hierarchy.Add(m_Icon);

                m_TextElement = new LocalizedTextElement { name = messageUssClassName };
                m_TextElement.AddToClassList(messageUssClassName);
                hierarchy.Add(m_TextElement);

                m_Divider = new Divider { name = dividerUssClassName, size = Size.M, spacing = Spacing.L, vertical = true };
                m_Divider.AddToClassList(dividerUssClassName);
                hierarchy.Add(m_Divider);

                m_ActionContainer = new VisualElement { name = actionContainerUssClassName };
                m_ActionContainer.AddToClassList(actionContainerUssClassName);
                hierarchy.Add(m_ActionContainer);

                notificationStyle = NotificationStyle.Default;
                text = "";
                icon = null;
                RefreshActionContainer();
            }

            public string icon
            {
                get => m_Icon.iconName;
                set
                {
                    m_Icon.iconName = value;
                    m_Icon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Icon.iconName));
                }
            }

            public string text
            {
                get => m_TextElement.text;
                set => m_TextElement.text = value;
            }

            public AnimationMode animationMode { get; set; } = AnimationMode.Fade;

            public NotificationStyle notificationStyle
            {
                get => m_Style;

                set
                {
                    RemoveFromClassList(ussClassName + "--" + m_Style.ToString().ToLower());
                    m_Style = value;
                    AddToClassList(ussClassName + "--" + m_Style.ToString().ToLower());
                }
            }

            public event Action<int> actionClicked;

            void RefreshActionContainer()
            {
                var noActions = m_Actions.Count == 0;
                foreach (var child in m_ActionContainer.Children())
                {
                    child.UnregisterCallback<ClickEvent>(ActionClicked);
                    child.userData = null;
                }

                m_ActionContainer.Clear();
                foreach (var actionKvp in m_Actions) CreateActionItem(actionKvp.Value);
                m_Divider.EnableInClassList(Styles.hiddenUssClassName, noActions);
                m_ActionContainer.EnableInClassList(Styles.hiddenUssClassName, noActions);
            }

            void CreateActionItem(ActionItem item)
            {
                //todo use ActionButton in order to use Clickable for events
                var actionButton = new LocalizedTextElement { focusable = true, text = item.text };
                actionButton.AddToClassList(actionUssClassName);
                m_ActionContainer.Add(actionButton);
                actionButton.userData = item;
                actionButton.RegisterCallback<ClickEvent>(ActionClicked);
            }

            void ActionClicked(ClickEvent evt)
            {
                if (evt.target is VisualElement ve && ve.userData is ActionItem actionItem)
                {
                    actionItem.callback?.Invoke();
                    actionClicked?.Invoke(actionItem.key);
                }
            }

            public void SetAction(int key, string displayText, Action callback)
            {
                m_Actions[key] = new ActionItem { callback = callback, key = key, text = displayText };
                RefreshActionContainer();
            }

            public void RemoveAction(int key)
            {
                if (m_Actions.ContainsKey(key))
                {
                    m_Actions.Remove(key);
                    RefreshActionContainer();
                }
            }
        }
    }
}
