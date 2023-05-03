using System;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Interface used on UI elements which handle a *selected* state.
    /// </summary>
    public interface ISelectableElement
    {
        /// <summary>
        /// **True** of the UI element is in selected state, **False** otherwise.
        /// </summary>
        bool selected { get; set; }

        /// <summary>
        /// Set the *selected* state of a UI element without sending an event through the visual tree.
        /// </summary>
        /// <param name="newValue">The new *selected* state value.</param>
        void SetSelectedWithoutNotify(bool newValue);
    }
}
