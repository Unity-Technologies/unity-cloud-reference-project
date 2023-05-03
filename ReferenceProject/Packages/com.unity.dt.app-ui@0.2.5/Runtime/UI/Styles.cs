using System;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The sizing of UI components.
    /// </summary>
    public enum Size
    {
        /// <summary>
        /// Small
        /// </summary>
        S = 1,

        /// <summary>
        /// Medium
        /// </summary>
        M,

        /// <summary>
        /// Large
        /// </summary>
        L
    }

    /// <summary>
    /// The spacing of UI components.
    /// </summary>
    public enum Spacing
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Small
        /// </summary>
        S,

        /// <summary>
        /// Medium
        /// </summary>
        M,

        /// <summary>
        /// Large
        /// </summary>
        L
    }

    /// <summary>
    /// General usage styling classes for UI components.
    /// </summary>
    public static class Styles
    {
        /// <summary>
        /// The styling class used to hide an element completely.
        /// </summary>
        public const string hiddenUssClassName = "unity-hidden";

        /// <summary>
        /// The styling class used to set an "invalid" pseudo-state on a element.
        /// </summary>
        public const string invalidUssClassName = "is-invalid";

        /// <summary>
        /// The styling class used to set a "checked" pseudo-state on a element.
        /// </summary>
        public const string checkedUssClassName = "is-checked";

        /// <summary>
        /// The styling class used to set an "intermediate" pseudo-state on a element.
        /// </summary>
        public const string intermediateUssClassName = "is-intermediate";

        /// <summary>
        /// The styling class used to set an "open" pseudo-state on a element.
        /// </summary>
        public const string openUssClassName = "is-open";

        /// <summary>
        /// The styling class used to set a "selected" pseudo-state on a element.
        /// </summary>
        public const string selectedUssClassName = "is-selected";
        
        /// <summary>
        /// The styling class used to set an "active" pseudo-state on a element.
        /// </summary>
        public const string activeUssClassName = "is-active";

        /// <summary>
        /// The styling class used to set a ":last-child" pseudo-state on a element.
        /// </summary>
        public const string lastChildUssClassName = "unity-last-child";

        /// <summary>
        /// The styling class used to set a "focus" pseudo-state on a element.
        /// </summary>
        public const string focusedUssClassName = "is-focused";

        /// <summary>
        /// Used in popups to hide the arrow/tip.
        /// </summary>
        public const string noArrowUssClassName = "no-arrow";

        /// <summary>
        /// The styling class used to set a "keyboard-focus" pseudo-state on a element.
        /// </summary>
        public const string keyboardFocusUssClassName = "keyboard-focus";
    }
}
