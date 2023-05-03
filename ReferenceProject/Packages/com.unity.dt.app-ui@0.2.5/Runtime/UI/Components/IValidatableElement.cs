using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Interface used on UI elements which handle value validation.
    /// <remarks>
    /// Value validation implies the UI element has a `value` property,
    /// hence this interface inherits from <see cref="INotifyValueChanged{T}"/>.
    /// </remarks>
    /// </summary>
    /// <typeparam name="TValueType">The type of the `value`.</typeparam>
    public interface IValidatableElement<TValueType> : INotifyValueChanged<TValueType>
    {
        /// <summary>
        /// **True** if the current value set on the UI element is invalid, **False** otherwise.
        /// <remarks>
        /// The invalid state is handled by the returned result of the <see cref="validateValue"/> function.
        /// </remarks>
        /// </summary>
        bool invalid { get; set; }

        /// <summary>
        /// Set this property to a reference of your custom function which will validate the current `value` of a UI element.
        /// <remarks>
        /// This function will be invoked automatically by the UI element implementation in order
        /// to update the <see cref="invalid"/> state property.
        /// <para>
        /// If the property is `null`, there wont be any validation process so by convention the `value` will be always valid.
        /// </para>
        /// </remarks>
        /// </summary>
        Func<TValueType, bool> validateValue { get; set; }
    }
}
