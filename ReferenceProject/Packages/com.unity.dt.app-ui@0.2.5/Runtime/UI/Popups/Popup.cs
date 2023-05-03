using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// This is the base class for any UI component that needs to be displayed over the rest of the user interface.
    /// </summary>
    public abstract class Popup
    {
        /// <summary>
        /// The average duration of a frame in milliseconds. Used to delay position calculations.
        /// </summary>
        protected const int k_NextFrameDurationMs = 16;

        /// <summary>
        /// The message id used to show the popup.
        /// </summary>
        protected const int k_PopupShow = 1;

        /// <summary>
        /// The message id used to dismiss the popup.
        /// </summary>
        protected const int k_PopupDismiss = 2;

        Handler m_Handler;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="view">The popup visual element itself.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        /// <exception cref="ArgumentException">The container can't be null.</exception>
        protected Popup(VisualElement parentView, ApplicationContext context, VisualElement view, VisualElement contentView = null)
        {
            this.context = context;
            this.contentView = contentView;
            targetParent = parentView ?? throw new ArgumentException("The parent view can't be null.");
            this.view = view ?? throw new ArgumentException("The view can't be null.");

            if (contentView is IDismissInvocator invocator)
                invocator.dismissRequested += Dismiss;
        }

        /// <summary>
        /// The handler that receives and dispatches messages. This is useful in multi-threaded applications.
        /// </summary>
        protected Handler handler
        {
            get
            {
                if (m_Handler == null)
                    m_Handler = new Handler(AppUI.mainLooper, message =>
                    {
                        switch (message.what)
                        {
                            case k_PopupShow:
                                ((Popup)message.obj).ShowView();
                                return true;
                            case k_PopupDismiss:
                                ((Popup)message.obj).HideView((DismissType)message.arg1);
                                return true;
                            default:
                                return false;
                        }
                    });

                return m_Handler;
            }
        }

        /// <summary>
        /// `True` if the the popup can be dismissed by pressing the escape key or the return button on mobile, `False` otherwise.
        /// <para>
        /// The default value is `True`.
        /// </para>
        /// </summary>
        public bool keyboardDismissEnabled { get; protected set; } = true;

        /// <summary>
        /// Returns the popup's <see cref="VisualElement"/>.
        /// </summary>
        public VisualElement view { get; }

        /// <summary>
        /// The parent of the <see cref="view"/> when the popup will be displayed.
        /// </summary>
        public VisualElement targetParent { get; }
        
        /// <summary>
        /// The content of the popup.
        /// </summary>
        public VisualElement contentView { get; }

        /// <summary>
        /// The <see cref="ApplicationContext"/> linked to this popup.
        /// </summary>
        public ApplicationContext context { get; }

        /// <summary>
        /// Dismiss the <see cref="Popup"/>.
        /// </summary>
        public virtual void Dismiss()
        {
            Dismiss(DismissType.Manual);
        }

        /// <summary>
        /// Dismiss the <see cref="Popup"/>.
        /// </summary>
        /// <param name="reason">Why the element has been dismissed.</param>
        public virtual void Dismiss(DismissType reason)
        {
            if (ShouldDismiss(reason))
                handler.SendMessage(handler.ObtainMessage(k_PopupDismiss, (int)reason, this));
        }

        /// <summary>
        /// Check if the popup should be dismissed or not, depending on the reason.
        /// </summary>
        /// <param name="reason"> Why the element has been dismissed.</param>
        /// <returns> `True` if the popup should be dismissed, `False` otherwise.</returns>
        protected virtual bool ShouldDismiss(DismissType reason)
        {
            // By default, we don't allow to dismiss the popup if the user clicks outside of it.
            if (reason == DismissType.OutOfBounds)
                return false;
            return true;
        }

        /// <summary>
        /// Show the <see cref="Popup"/>.
        /// </summary>
        public virtual void Show()
        {
            handler.SendMessage(handler.ObtainMessage(k_PopupShow, this));
        }

        /// <summary>
        /// Called when the popup's <see cref="Handler"/> has received a <see cref="k_PopupShow"/> message.
        /// <remarks>
        /// In this method the view should become visible at some point (directly or via an animation).
        /// </remarks>
        /// </summary>
        protected virtual void ShowView()
        {
            if (view.parent == null)
            {
                // not added into the visual tree yet
                targetParent.Add(view);

                // set invisible in order to calculate layout before displaying the element (avoid flickering)
                view.visible = false;
            }

            view.RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            view.RegisterCallback<KeyDownEvent>(OnViewKeyDown);

            if (ShouldAnimate())
            {
                AnimateViewIn();
            }
            else
            {
                // be sure its visible
                view.visible = true;
                InvokeShownEventHandlers();
            }
        }

        /// <summary>
        /// Returns the element that will be focused when the view will become visible.
        /// <para>
        /// The default value is `null`.
        /// </para>
        /// </summary>
        /// <returns>The element that will be focused when the view will become visible.</returns>
        protected virtual VisualElement GetFocusableElement()
        {
            return null;
        }

        /// <summary>
        /// Implement this method to know if the popup should call
        /// <see cref="AnimateViewIn"/> and <see cref="AnimateViewOut"/> methods or not.
        /// </summary>
        /// <returns>`True` if you want to animate the popup, `False` otherwise.</returns>
        protected virtual bool ShouldAnimate()
        {
            // todo we can check here if any Accessibility flags is currently in use that should prevent animations.
            return false;
        }

        /// <summary>
        /// Called when the popup has become visible.
        /// </summary>
        protected virtual void InvokeShownEventHandlers()
        {
            var focusableElement = GetFocusableElement();
            if (focusableElement != null)
                focusableElement.schedule.Execute(() =>
                {
                    // Instead of force focusing an element in the popup content,
                    // we should change the current FocusController of the panel:
                    // focusableElement.panel.focusController = new FocusController(new VisualElementFocusRing(focusableElement));
                    // but UITK doesnt provide any accessible way to do it
                    focusableElement.Focus();
                }).ExecuteLater(k_NextFrameDurationMs);
        }

        /// <summary>
        /// Called when the popup has received a <see cref="KeyDownEvent"/>.
        /// <para>
        /// By default this method handles the dismiss of the popup via the Escape key or a Return button.
        /// </para>
        /// </summary>
        /// <param name="evt">The event the popup has received.</param>
        protected virtual void OnViewKeyDown(KeyDownEvent evt)
        {
            var focusableElement = GetFocusableElement();
            if (keyboardDismissEnabled && focusableElement != null && evt.keyCode == KeyCode.Escape)
            {
                evt.PreventDefault();
                evt.StopPropagation();
                Dismiss(DismissType.Cancel);
            }
        }

        /// <summary>
        /// Start the animation for this popup.
        /// </summary>
        protected virtual void AnimateViewIn()
        {
            // do nothing by default
        }

        /// <summary>
        /// Called when the popup's <see cref="Handler"/> has received a <see cref="k_PopupDismiss"/> message.
        /// </summary>
        /// <param name="reason">The reason why the popup should be dismissed.</param>
        protected virtual void HideView(DismissType reason)
        {
            view.UnregisterCallback<KeyDownEvent>(OnViewKeyDown);

            if (ShouldAnimate())
            {
                AnimateViewOut(reason);
            }
            else
            {
                InvokeDismissedEventHandlers(reason);
            }
        }

        /// <summary>
        /// Called when the popup has completed its dismiss process.
        /// </summary>
        /// <param name="reason">The reason why the popup has been dismissed.</param>
        protected virtual void InvokeDismissedEventHandlers(DismissType reason)
        {
            view.UnregisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            view.visible = false;
        }

        /// <summary>
        /// Called when the popup's <see cref="VisualElement"/> has been removed from the panel.
        /// </summary>
        /// <param name="evt"> The event that has been received.</param>
        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            // The panel has been destroyed, we need to manually do the cleanup.
            if (view.visible)
                InvokeDismissedEventHandlers(DismissType.PanelDestroyed);
        }

        /// <summary>
        /// Start the hide animation for this popup.
        /// </summary>
        protected virtual void AnimateViewOut(DismissType reason)
        {
            // do nothing
        }

        /// <summary>
        /// Find the parent <see cref="VisualElement"/> where the popup will be added.
        /// <remarks>
        /// This is usually one of the layers from the <see cref="Panel"/> root UI element.
        /// </remarks>
        /// </summary>
        /// <param name="element">An arbitrary UI element inside the panel.</param>
        /// <returns>The popup container <see cref="VisualElement"/> in the current panel.</returns>
        protected virtual VisualElement FindSuitableParent(VisualElement element)
        {
            return Panel.FindPopupLayer(element);
        }
    }

    /// <summary>
    /// A generic base class for popups.
    /// </summary>
    /// <typeparam name="T">A sealed popup class type.</typeparam>
    public abstract class Popup<T> : Popup where T : Popup<T>
    {
        /// <summary>
        /// The last focused element before the popup was shown.
        /// </summary>
        protected Focusable m_LastFocusedElement;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="context">The application context attached to this popup.</param>
        /// <param name="view">The popup visual element itself.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        protected Popup(VisualElement parentView, ApplicationContext context, VisualElement view, VisualElement contentView = null)
            : base(parentView, context, view, contentView) { }

        /// <summary>
        /// Event triggered when the popup has become visible.
        /// </summary>
        public event Action<T> shown;

        /// <summary>
        /// Event triggered when the popup has been dismissed.
        /// </summary>
        public event Action<T, DismissType> dismissed;

        /// <summary>
        /// Activate the possibility to dismiss the popup via Escape key or Return button.
        /// </summary>
        /// <param name="dismissEnabled">`True` to activate the feature, `False` otherwise.</param>
        /// <returns>The popup of type <typeparamref name="T"/>.</returns>
        public T SetKeyboardDismiss(bool dismissEnabled)
        {
            keyboardDismissEnabled = dismissEnabled;
            return (T)this;
        }

        /// <summary>
        /// Set the last focused element before the popup was shown.
        /// </summary>
        /// <param name="focusable"> The last focused element.</param>
        /// <returns> The popup of type <typeparamref name="T"/>.</returns>
        public T SetLastFocusedElement(Focusable focusable)
        {
            m_LastFocusedElement = focusable;
            return (T)this;
        }

        /// <summary>
        /// Called when the popup has become visible.
        /// This method will invoke any handlers attached to the <see cref="shown"/> event.
        /// </summary>
        protected override void InvokeShownEventHandlers()
        {
            base.InvokeShownEventHandlers();
            shown?.Invoke((T)this);
        }

        /// <summary>
        /// Called when the popup has been dismissed.
        /// This method will invoke any handlers attached to the <see cref="dismissed"/> event.
        /// </summary>
        /// <param name="reason"></param>
        protected override void InvokeDismissedEventHandlers(DismissType reason)
        {
            base.InvokeDismissedEventHandlers(reason);
            dismissed?.Invoke((T)this, reason);

            // we can safely remove the notification element from the visual tree now.
            if (view.parent == targetParent) targetParent.Remove(view);

            // focus last focused element (if any)
            if (reason != DismissType.OutOfBounds)
                m_LastFocusedElement?.Focus();
        }
    }
}
