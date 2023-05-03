using System;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extensions for working with colors.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Check if a string is a valid hex color.
        /// </summary>
        /// <param name="hexColor">The hex color to check.</param>
        /// <returns>True if the string is a valid hex color.</returns>
        public static bool IsValidHex(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return false;
            
            var allHexDigits = true;
            foreach (var hexColorChar in hexColor)
            {
                if (!Uri.IsHexDigit(hexColorChar))
                {
                    allHexDigits = false;
                    break;
                }
            }
            
            return allHexDigits && hexColor is { Length: 3 or 4 or 6 or 8 };
        }

        /// <summary>
        /// Check if a string is a valid RGB hex color.
        /// </summary>
        /// <param name="hexColor">The hex color to check.</param>
        /// <returns>True if the string is a valid RGB hex color.</returns>
        public static bool IsRgbHex(string hexColor)
        {
            return IsValidHex(hexColor) && hexColor is { Length: 3 or 6 };
        }

        /// <summary>
        /// Convert a ARGB hex color to RGBA hex color.
        /// </summary>
        /// <param name="argbHex">The ARGB hex color.</param>
        /// <returns>The RGBA hex color if the string is a valid ARGB hex color, null otherwise.</returns>
        public static string ArgbToRgbaHex(string argbHex)
        {
            if (!IsValidHex(argbHex))
                return null;

            if (IsRgbHex(argbHex))
                return argbHex;

            var aLength = argbHex.Length == 4 ? 1 : 2;
            var aIndex = argbHex.Length == 4 ? 3 : 6;
            var a = argbHex.Substring(0, aLength);
            return argbHex.Remove(0, aLength).Insert(aIndex, $"{a}");
        }

        /// <summary>
        /// Convert a RGBA hex color to ARGB hex color.
        /// </summary>
        /// <param name="rgbaHex">The RGBA hex color.</param>
        /// <returns>The ARGB hex color if the string is a valid RGBA hex color, null otherwise.</returns>
        public static string RgbaToArgbHex(string rgbaHex)
        {
            if (!IsValidHex(rgbaHex))
                return null;

            if (IsRgbHex(rgbaHex))
                return rgbaHex;

            var aLength = rgbaHex.Length == 4 ? 1 : 2;
            var aIndex = rgbaHex.Length == 4 ? 3 : 6;
            var a = rgbaHex.Substring(aIndex, aLength);
            return rgbaHex.Remove(aIndex, aLength).Insert(0, $"{a}");
        }

        /// <summary>
        /// Convert a <see cref="Color"/> to a RGBA hex color.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The RGBA hex color.</returns>
        public static string ColorToRgbaHex(Color color)
        {
            var argb = System.Drawing.Color.FromArgb(
                Mathf.RoundToInt(color.a * 255),
                Mathf.RoundToInt(color.r * 255),
                Mathf.RoundToInt(color.g * 255),
                Mathf.RoundToInt(color.b * 255)).ToArgb();
            return ArgbToRgbaHex(argb.ToString("X8"));
        }
    }
}
