namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extensions for the <see cref="TextField"/> class.
    /// </summary>
    public static class TextFieldExtensions
    {
        /// <summary>
        /// Make the cursor blink.
        /// </summary>
        /// <param name="tf">The <see cref="TextField"/> object.</param>
        public static void BlinkingCursor(this UIElements.TextField tf)
        {
            const string transparentCursorUssClassName = "appui-text-cursor--transparent";

            tf.schedule.Execute(() =>
            {
                if (tf.ClassListContains(transparentCursorUssClassName))
                    tf.RemoveFromClassList(transparentCursorUssClassName);
                else
                    tf.AddToClassList(transparentCursorUssClassName);
            }).Every(500);
        }
    }
}
