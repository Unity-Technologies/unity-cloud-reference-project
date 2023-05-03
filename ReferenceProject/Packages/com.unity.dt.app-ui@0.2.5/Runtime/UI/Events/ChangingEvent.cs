using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Interface that must be implemented to UI components which can change their value progressively, like a <see cref="Slider"/>.
    /// </summary>
    /// <typeparam name="TValueType">The type of value handled by the UI component.</typeparam>
    public interface INotifyValueChanging<TValueType> : INotifyValueChanged<TValueType> { }

    /// <summary>
    /// Extensions for <see cref="INotifyValueChanging{TValueType}"/>.
    /// </summary>
    public static class NotifyValueChangingExtensions
    {
        /// <summary>
        /// Register a callback which will be invoked when the UI component's value is changing.
        /// </summary>
        /// <param name="control">The UI component.</param>
        /// <param name="callback">The callback.</param>
        /// <typeparam name="TValueType">The type of value handled by the UI component.</typeparam>
        /// <returns>True if the UI component can handle callbacks, False otherwise.</returns>
        [Preserve]
        public static bool RegisterValueChangingCallback<TValueType>(this INotifyValueChanging<TValueType> control, EventCallback<ChangingEvent<TValueType>> callback)
        {
            if (!(control is CallbackEventHandler callbackEventHandler))
                return false;
            callbackEventHandler.RegisterCallback(callback);
            return true;
        }

        /// <summary>
        /// Unregister a callback which has been invoked when the UI component's value was changing.
        /// </summary>
        /// <param name="control">The UI component.</param>
        /// <param name="callback">The callback.</param>
        /// <typeparam name="TValueType">The type of value handled by the UI component.</typeparam>
        /// <returns>True if the UI component can handle callbacks, False otherwise.</returns>
        [Preserve]
        public static bool UnregisterValueChangingCallback<TValueType>(this INotifyValueChanging<TValueType> control, EventCallback<ChangingEvent<TValueType>> callback)
        {
            if (!(control is CallbackEventHandler callbackEventHandler))
                return false;
            callbackEventHandler.UnregisterCallback(callback);
            return true;
        }
    }

    /// <summary>
    /// THe value changing event.
    /// </summary>
    /// <typeparam name="TValueType">The type of value handled by the UI component.</typeparam>
    public class ChangingEvent<TValueType> : EventBase<ChangingEvent<TValueType>>
    {
        /// <summary>
        /// The previous value.
        /// </summary>
        public TValueType previousValue { get; set; }

        /// <summary>
        /// The new value.
        /// </summary>
        public TValueType newValue { get; set; }
    }
}
