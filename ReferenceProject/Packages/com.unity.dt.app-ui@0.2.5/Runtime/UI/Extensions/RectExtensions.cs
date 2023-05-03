namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extension methods for the <see cref="Rect"/> class.
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// Check if a given <see cref="Rect"/> has a valid width and height.
        /// </summary>
        /// <param name="rect">The given <see cref="Rect"/> object.</param>
        /// <returns>True if its width and height are valid, False otherwise.</returns>
        public static bool IsValid(this Rect rect)
        {
            return rect != default &&
                !float.IsNaN(rect.width) && !float.IsNaN(rect.height) &&
                !float.IsInfinity(rect.width) && !float.IsInfinity(rect.height) &&
                !float.IsNegative(rect.width) && !float.IsNegative(rect.height) &&
                !Mathf.Approximately(0, rect.width) && !Mathf.Approximately(0, rect.height);
        }

        /// <summary>
        /// Check if a given <see cref="Rect"/> has a valid width and height to be used as Texture size.
        /// </summary>
        /// <param name="rect">The given <see cref="Rect"/> object.</param>
        /// <returns>True if its width and height are valid, False otherwise.</returns>
        public static bool IsValidForTextureSize(this Rect rect)
        {
            return rect.IsValid() && rect.size.IsValidForTextureSize();
        }

        /// <summary>
        /// Check if a given <see cref="Vector2"/> has a valid value to be used as Texture size.
        /// </summary>
        /// <param name="vec">The given <see cref="Vector2"/> object.</param>
        /// <returns>True if its width and height are valid, False otherwise.</returns>
        public static bool IsValidForTextureSize(this Vector2 vec)
        {
            return vec.x is > 0 and <= 4096 && vec.y is > 0 and <= 4096;
        }
    }
}
