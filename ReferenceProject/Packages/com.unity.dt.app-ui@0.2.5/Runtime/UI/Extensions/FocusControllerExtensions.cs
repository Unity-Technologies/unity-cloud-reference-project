using System.Reflection;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extension methods for FocusController.
    /// </summary>
    public static class FocusControllerExtensions
    {
        static readonly MethodInfo k_FocusNextInDirection = typeof(FocusController).GetMethod("FocusNextInDirection",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Focus the next element in the given direction.
        /// </summary>
        /// <param name="controller">The focus controller.</param>
        /// <param name="direction">The direction to focus.</param>
        public static void FocusNextInDirectionEx(this FocusController controller, FocusChangeDirection direction)
        {
            if (controller == null)
                return;

            k_FocusNextInDirection!.Invoke(controller, new object[] { direction });
        }
    }
}
