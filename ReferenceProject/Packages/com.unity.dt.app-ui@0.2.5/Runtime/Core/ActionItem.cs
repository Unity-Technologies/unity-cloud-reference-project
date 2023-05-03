using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// An ActionItem provides meta information about a specific Action.
    /// This is used internally by UI Components which can be bound to an Action ID.
    /// </summary>
    struct ActionItem
    {
        /// <summary>
        /// The action ID.
        /// </summary>
        public int key;

        /// <summary>
        /// The display text for this action.
        /// </summary>
        public string text;

        /// <summary>
        /// The callback which will be called when the UI Component bound to this action will be interacted with.
        /// </summary>
        public Action callback;
    }
}
