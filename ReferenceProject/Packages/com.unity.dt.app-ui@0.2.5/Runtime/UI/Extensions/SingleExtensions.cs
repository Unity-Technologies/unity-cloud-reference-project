using System;
using System.Globalization;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extension methods for Single and Double.
    /// </summary>
    public static class SingleExtensions
    {
        /// <summary>
        /// Returns a string representation of the value with a variable number of decimal places.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <param name="cultureInfo">The culture info to use.</param>
        /// <returns>A string representation of the value with a variable number of decimal places.</returns>
        public static string ToStringWithVariableDecimalCount(this float val, CultureInfo cultureInfo)
        {
            var abs = Mathf.Abs(val);
            return abs switch
            {
                < 10 => val.ToString("0.####", cultureInfo),
                < 100 => val.ToString("0.###", cultureInfo),
                < 1000 => val.ToString("0.##", cultureInfo),
                _ => val.ToString("0.#", cultureInfo)
            };
        }

        /// <summary>
        /// Returns a string representation of the value with a variable number of decimal places.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <param name="cultureInfo">The culture info to use.</param>
        /// <returns>A string representation of the value with a variable number of decimal places.</returns>
        public static string ToStringWithVariableDecimalCount(this double val, CultureInfo cultureInfo)
        {
            var abs = Math.Abs(val);
            return abs switch
            {
                < 10 => val.ToString("0.####", cultureInfo),
                < 100 => val.ToString("0.###", cultureInfo),
                < 1000 => val.ToString("0.##", cultureInfo),
                _ => val.ToString("0.#", cultureInfo)
            };
        }
    }
}
