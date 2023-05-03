using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The animation used by Notification to appear and disappear.
    /// </summary>
    public enum AnimationMode
    {
        /// <summary>
        /// The notification slides in and out of the screen.
        /// </summary>
        Slide,

        /// <summary>
        /// The notification fades in and out on the screen.
        /// </summary>
        Fade
    }

    /// <summary>
    /// A base class for notification displayed at the bottom of the screen.
    /// </summary>
    /// <typeparam name="T">The sealed Notification popup class type.</typeparam>
    public abstract class BottomNotification<T> : Popup<T> where T : BottomNotification<T>
    {
        const int k_AnimationDuration = 250;

        const int k_AnimationFadeInDuration = 150;

        const int k_AnimationFadeOutDuration = 75;

        const float k_AnimationScaleFromValue = 0.8f;

        readonly ManagerCallback m_ManagerCallback;

        AnimationMode m_AnimationMode = AnimationMode.Fade;

        NotificationDuration m_Duration = NotificationDuration.Short;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="view">The popup visual element itself.</param>
        protected BottomNotification(VisualElement parentView, ApplicationContext context, VisualElement view)
            : base(parentView, context, view)
        {
            m_ManagerCallback = new ManagerCallback(this);
            keyboardDismissEnabled = false;
            view.usageHints = UsageHints.DynamicTransform;
        }

        /// <summary>
        /// Returns the animation used by the bar when it will be displayed.
        /// </summary>
        public AnimationMode animationMode => m_AnimationMode;

        /// <summary>
        /// Returns True if the bar is currently displayed on the screen, False otherwise.
        /// </summary>
        public bool isShown => AppUI.notificationManager.IsCurrent(m_ManagerCallback);

        /// <summary>
        /// Returns True if the bar is currently displayed or queued for display on the screen, False otherwise.
        /// </summary>
        public bool isShownOrQueued => AppUI.notificationManager.IsCurrentOrNext(m_ManagerCallback);

        /// <summary>
        /// Returns the specified display duration of the bar.
        /// </summary>
        public NotificationDuration duration => m_Duration;

        /// <summary>
        /// Set a new value for the <see cref="animationMode"/> property.
        /// </summary>
        /// <param name="animation">THe new value</param>
        /// <returns>The current object instance to continuously build the element.</returns>
        public T SetAnimationMode(AnimationMode animation)
        {
            m_AnimationMode = animation;
            return (T)this;
        }

        /// <summary>
        /// Set the duration the notification should be displayed.
        /// </summary>
        /// <param name="durationValue">A duration enum value.</param>
        /// <returns>The current object instance to continuously build the element.</returns>
        public virtual T SetDuration(NotificationDuration durationValue)
        {
            if (!isShownOrQueued)
                m_Duration = durationValue;
            else
                Debug.LogWarning("Unable to set a duration while the Bar is already shown or queued.");
            return (T)this;
        }

        /// <summary>
        /// Implement this method to know if the popup should call
        /// <see cref="AnimateViewIn"/> and <see cref="AnimateViewOut"/> methods or not.
        /// </summary>
        /// <returns>`True` if you want to animate the popup, `False` otherwise.</returns>
        protected override bool ShouldAnimate()
        {
            return true;
        }

        /// <inheritdoc cref="Popup.AnimateViewIn"/>
        protected override void AnimateViewIn()
        {
            // delay the animation of the notification to be sure the layout has been updated with UI Toolkit.
            view.schedule.Execute(() =>
            {
                if (view.parent != null) view.visible = true;

                switch (animationMode)
                {
                    case AnimationMode.Slide:
                        StartSlideInAnimation();
                        break;
                    case AnimationMode.Fade:
                        StartFadeInAnimation();
                        break;
                    default:
                        throw new ValueOutOfRangeException(nameof(animationMode), animationMode);
                }
            }).ExecuteLater(16);
        }

        /// <inheritdoc cref="Popup.AnimateViewOut"/>
        protected override void AnimateViewOut(DismissType reason)
        {
            switch (animationMode)
            {
                case AnimationMode.Slide:
                    StartSlideOutAnimation(reason);
                    break;
                case AnimationMode.Fade:
                    StartFadeOutAnimation(reason);
                    break;
                default:
                    throw new ValueOutOfRangeException(nameof(animationMode), animationMode);
            }
        }

        /// <inheritdoc cref="Popup{T}.InvokeShownEventHandlers"/>
        protected override void InvokeShownEventHandlers()
        {
            AppUI.notificationManager.OnShown(m_ManagerCallback);
            base.InvokeShownEventHandlers(); // invoke callbacks if any
        }

        /// <inheritdoc cref="Popup{T}.InvokeDismissedEventHandlers"/>
        protected override void InvokeDismissedEventHandlers(DismissType reason)
        {
            AppUI.notificationManager.OnDismissed(m_ManagerCallback);
            base.InvokeDismissedEventHandlers(reason); // invoke callbacks if any
        }

        /// <inheritdoc cref="Popup.Dismiss(DismissType)"/>
        public override void Dismiss(DismissType reason)
        {
            AppUI.notificationManager.Dismiss(m_ManagerCallback, reason);
        }

        /// <inheritdoc cref="Popup.Show"/>
        public override void Show()
        {
            AppUI.notificationManager.Show(duration, m_ManagerCallback);
        }

        /// <inheritdoc cref="Popup.FindSuitableParent"/>
        protected override VisualElement FindSuitableParent(VisualElement element)
        {
            return Panel.FindNotificationLayer(element);
        }

        void StartFadeInAnimation()
        {
            var opacityAnimation = view.experimental.animation.Start(0, 1, k_AnimationFadeInDuration, (element, f) =>
            {
                element.style.opacity = f;
            }).OnCompleted(InvokeShownEventHandlers);

            var scaleAnimation = view.experimental.animation.Start(k_AnimationScaleFromValue, 1, k_AnimationFadeInDuration, (element, f) =>
            {
                element.style.scale = new StyleScale(new Scale(new Vector3(f, f, 1.0f)));
            });

            opacityAnimation.Start();
            scaleAnimation.Start();
        }

        void StartSlideInAnimation()
        {
            var translationYBottom = GetTranslationYBottom();
            var translationAnimation = view.experimental.animation.Start(translationYBottom, 0, k_AnimationDuration, (element, f) =>
            {
                element.style.bottom = f;
            }).Ease(Easing.OutCubic).OnCompleted(InvokeShownEventHandlers);

            translationAnimation.Start();
        }

        float GetTranslationYBottom()
        {
            var result = view.resolvedStyle.height + view.resolvedStyle.marginBottom;
            return -result;
        }

        void StartFadeOutAnimation(DismissType reason)
        {
            view.experimental.animation.Start(1, 0, k_AnimationFadeOutDuration, (element, f) =>
            {
                element.style.opacity = f;
            }).OnCompleted(() => InvokeDismissedEventHandlers(reason)).Start();
        }

        void StartSlideOutAnimation(DismissType reason)
        {
            var translationYBottom = GetTranslationYBottom();
            view.experimental.animation.Start(0, translationYBottom, k_AnimationDuration, (element, f) =>
            {
                element.style.bottom = f;
            }).OnCompleted(() => InvokeDismissedEventHandlers(reason)).Start();
        }

        /// <summary>
        /// Implementation of the Notification Manager callback interface for <see cref="BottomNotification{T}"/> objects.
        /// </summary>
        class ManagerCallback : NotificationManager.ICallback
        {
            public ManagerCallback(BottomNotification<T> element)
            {
                obj = element;
            }

            public void Show()
            {
                var handler = ((BottomNotification<T>)obj).handler;
                handler.SendMessage(handler.ObtainMessage(k_PopupShow, obj));
            }

            public void Dismiss(DismissType reason)
            {
                var handler = ((BottomNotification<T>)obj).handler;
                handler.SendMessage(handler.ObtainMessage(k_PopupDismiss, (int)reason, obj));
            }

            public object obj { get; }
        }
    }
}
