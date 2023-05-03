namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extensions for the <see cref="KeyCode"/> enum.
    /// </summary>
    public static class KeyCodeExtensions
    {
        /// <summary>
        /// Check if the <see cref="KeyCode"/> is a Submit type key (Enter, Return, or SpaceBar).
        /// </summary>
        /// <param name="keyCode">The KeyCode value.</param>
        /// <returns>True if the value is a Submit type, False otherwise.</returns>
        public static bool IsSubmitType(this KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.KeypadEnter => true,
                KeyCode.Return => true,
                KeyCode.Space => true,
                _ => false
            };
        }
    }
}
