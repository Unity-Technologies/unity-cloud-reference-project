using System.Reflection;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A Manipulator that adds a class to the target element when it is keyboard focused.
    /// This is useful for accessibility purposes.
    /// </summary>
    public class KeyboardFocusController : Manipulator
    {
        /// <summary>
        /// Check if the target element is keyboard focused.
        /// </summary>
        public bool keyboardFocused { get; private set; }

        readonly EventCallback<FocusInEvent> m_KeyboardFocusInEventCallback;

        readonly EventCallback<FocusInEvent> m_OtherFocusInEventCallback;

        readonly EventCallback<FocusOutEvent> m_FocusedOutCallback;

        static MethodInfo s_FocusNextInDirection;

        /// <summary>
        /// Construct a KeyboardFocusController.
        /// </summary>
        /// <param name="keyboardFocusInEventCallback"> A callback invoked when the target element receives keyboard focus.</param>
        /// <param name="otherFocusInEventCallback"> A callback invoked when the target element receives focus from another source.</param>
        /// <param name="focusedOutCallback"> A callback invoked when the target element loses focus.</param>
        public KeyboardFocusController(
            EventCallback<FocusInEvent> keyboardFocusInEventCallback = null,
            EventCallback<FocusInEvent> otherFocusInEventCallback = null,
            EventCallback<FocusOutEvent> focusedOutCallback = null)
        {
            m_KeyboardFocusInEventCallback = keyboardFocusInEventCallback;
            m_OtherFocusInEventCallback = otherFocusInEventCallback;
            m_FocusedOutCallback = focusedOutCallback;
        }

        /// <summary>
        /// Called to register event callbacks on the target element.
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<FocusInEvent>(OnFocusedIn);
            target.RegisterCallback<FocusOutEvent>(OnFocusedOut);
        }

        /// <summary>
        /// Called to unregister event callbacks from the target element.
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<FocusInEvent>(OnFocusedIn);
            target.UnregisterCallback<FocusOutEvent>(OnFocusedOut);
        }

        void OnFocusedOut(FocusOutEvent evt)
        {
            target.RemoveFromClassList(Styles.keyboardFocusUssClassName);
            m_FocusedOutCallback?.Invoke(evt);
        }

        void OnFocusedIn(FocusInEvent evt)
        {
            keyboardFocused = (int)evt.direction != (int)FocusChangeDirection.unspecified;

            if (keyboardFocused)
            {
                target.AddToClassList(Styles.keyboardFocusUssClassName);
                m_KeyboardFocusInEventCallback?.Invoke(evt);
            }
            else
            {
                m_OtherFocusInEventCallback?.Invoke(evt);
            }
        }
    }
}
