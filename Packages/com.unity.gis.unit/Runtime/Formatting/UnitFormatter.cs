using System;
using System.Globalization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// <see cref="IUnitFormatter"/> to format an <see cref="IUnit"/> into a string with double precision.
    /// </summary>
    public class UnitFormatter : IUnitFormatter
    {
        /// <summary>
        /// Default configuration
        /// </summary>
        public static readonly IUnitFormatter DefaultFormatter = new UnitFormatter();

        /// <summary>
        /// Default configuration with full string
        /// </summary>
        public static readonly IUnitFormatter DefaultFullStringFormatter = new UnitFormatter(true);

        /// <summary>
        /// <see cref="IUnit.IsNull"/> = true response string
        /// </summary>
        private static string k_UnitNull = "Unit null";

        /// <summary>
        /// When true, will format with full string option.
        /// </summary>
        readonly bool m_FullStringFormat;

        /// <summary>
        /// When true, will format with power value if power is greater than 1
        /// </summary>
        readonly internal bool m_HasSuperscript;

        /// <summary>
        /// Maximum precision for outputting decimal values.
        /// </summary>
        readonly internal int m_DecimalPrecision;

        /// <summary>
        /// Mimimum precision for outputting decimal values.
        /// </summary>
        readonly internal int m_DecimalMinimumPrecision;


        /// <summary>
        /// Constructs an <see cref="IFormatProvider"/> for <see cref="IUnit"/>.
        /// </summary>
        /// <param name="fullStringFormat">When true, the full string name & power are used when formatting.
        /// (Default: False)</param>
        /// <param name="hasSuperscript">When true, if the passed unit has a power higher than 1 then the resulting
        /// string will include the power or superscript symbol. (Default: True)</param>
        /// <param name="decimalPrecision">The maximum zero place to output when formatting values.
        /// (Default: Int32.MaxValue)</param>
        /// <param name="minimumPrecision">The minimum zero place to output when formatting values. (Default: 0)</param>
        public UnitFormatter(bool fullStringFormat = false, bool hasSuperscript = true, int decimalPrecision = Int32.MaxValue, int minimumPrecision = 0)
        {
            m_FullStringFormat = fullStringFormat;
            m_HasSuperscript = hasSuperscript;
            m_DecimalPrecision = decimalPrecision;
            m_DecimalMinimumPrecision = minimumPrecision;
        }

        /// <inheritdoc cref="IFormatProvider.GetFormat(Type)"/>
        public object GetFormat(Type formatType)
        {
            return typeof(UnitFormatter).IsAssignableFrom(formatType)
                ? this
                : null;
        }

        /// <inheritdoc cref="ICustomFormatter.Format(string, object, IFormatProvider)"/>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is IUnit unit)
            {
                if (unit.IsNull)
                    return k_UnitNull;

                return Format(unit);
            }
            return arg.ToString();
        }

        /// <inheritdoc cref="IUnitFormatter.Format(IUnit)"/>
        string Format(IUnit value)
        {
            if (value.IsNull)
                return k_UnitNull;
            return m_FullStringFormat
                ? FormatToFullString(value.UnitDef, value.Value, value.Power)
                : Format(value.UnitDef, value.Value, value.Power);
        }

        /// <summary>
        /// Overrides fullStringFormat parameter in constructor to format with symbols.
        /// </summary>
        /// <param name="unitDef">Input unit definition</param>
        /// <param name="value">Input numeric value</param>
        /// <param name="power">Power</param>
        /// <returns>A formatted string.</returns>
        string Format(IUnitDef unitDef, double value, byte power = 1) =>
            Format(unitDef, value, power, false, m_DecimalPrecision, m_HasSuperscript, m_DecimalMinimumPrecision);

        /// <summary>
        /// Overrides the constructor to expose all formatting options.
        /// </summary>
        /// <param name="unitDef">Input unit definition</param>
        /// <param name="value">Input numeric value</param>
        /// <param name="power">Power</param>
        /// <param name="fullString">Full unit names and powers will be used</param>
        /// <param name="decimalPrecision">Precision of the output decimal values</param>
        /// <param name="hasSuperscript">When true, the power of the unit will be displayed if over 1.</param>
        /// <param name="minimumPrecision">Minimum precision</param>
        /// <returns>A formatted string.</returns>
        string Format(IUnitDef unitDef, double value,  byte power, bool fullString, int decimalPrecision, bool hasSuperscript, int minimumPrecision) =>
            $"{FormatDecimal(value, decimalPrecision, minimumPrecision)}{FormatUnitSymbol(unitDef, value, fullString, hasSuperscript:hasSuperscript, power:power)}";

        /// <summary>
        /// Overrides fullStringFormat parameter in constructor to format as a full string.
        /// </summary>
        /// <param name="unitDef">Input unit definition</param>
        /// <param name="value">Input numeric value</param>
        /// <param name="power">Power</param>
        /// <returns>A formatted string.</returns>
        string FormatToFullString(IUnitDef unitDef, double value, byte power = 1) =>
            Format(unitDef, value, power, true, m_DecimalPrecision, m_HasSuperscript, m_DecimalMinimumPrecision);

        /// <summary>
        /// Get a singular or plural name or symbol for the value of a given scale
        /// </summary>
        /// <param name="unitDef">Source unitDef for symbols</param>
        /// <param name="value">Value to get the symbol for</param>
        /// <param name="fullString">True if the symbol should be a full text string like "Meter" instead of "m"</param>
        /// <param name="integer">True if only the integer value should be considered when choosing a plural name.</param>
        /// <param name="hasSuperscript">If true, and power > 1 then the power symbol will be included.</param>
        /// <param name="power">If power > 1 and hasSuperscript is true then the power symbol will be included.</param>
        /// <returns>Scale symbol</returns>
        static string FormatUnitSymbol(IUnitDef unitDef, double value, bool fullString, bool integer = false, bool hasSuperscript = false, byte power = 1)
        {
            if (unitDef == null)
                return "";

            var powerSymbol = FormatPowerSymbol(unitDef, power, hasSuperscript, fullString);

            if (fullString)
                return FormatFullStringSymbol(unitDef, value, powerSymbol, integer);
            
            return FormatShorthandSymbol(unitDef, powerSymbol);
        }

        /// <summary>
        /// Retrieve a symbol for the power of this unit
        /// </summary>
        /// <param name="unitDef">Unit definition</param>
        /// <param name="power">Unit power</param>
        /// <param name="hasSuperscript">Superscript state</param>
        /// <param name="fullString">is full string state</param>
        /// <returns>Power of unit</returns>
        static string FormatPowerSymbol(IUnitDef unitDef, byte power, bool hasSuperscript, bool fullString)
        {
            var powerSymbol = "";

            if (power != 1 && hasSuperscript)
                powerSymbol = fullString ? unitDef.ToFullStringPower(power) : unitDef.ToStringPower(power);
            return powerSymbol;
        }

        /// <summary>
        /// Formats a unit into a full string
        /// </summary>
        /// <param name="unitDef">Unit definition</param>
        /// <param name="value">Unit value</param>
        /// <param name="powerSymbol">Power of unit value</param>
        /// <param name="integer">as an integer state</param>
        /// <returns>Formatted value string</returns>
        static string FormatFullStringSymbol(IUnitDef unitDef, double value, string powerSymbol, bool integer)
        {
            if (integer)
                return " " + powerSymbol + ((MathF.Abs((int)value - 1) < 0.00001) ? unitDef.Name : unitDef.NamePlural);
            return " " + powerSymbol + ((MathF.Abs((float)value - 1.0f) < 0.00001) ? unitDef.Name: unitDef.NamePlural);
        }

        /// <summary>
        /// Formats a unit into a shorthand
        /// </summary>
        /// <param name="unitDef">Unit definition</param>
        /// <param name="powerSymbol">Power symbol to use</param>
        /// <returns></returns>
        static string FormatShorthandSymbol(IUnitDef unitDef, string powerSymbol)
        {
            // Loop through alternate names, and pick the shortest.
            if (unitDef.AlternateNames != null && unitDef.AlternateNames.Length > 0)
            {
                foreach (var item in unitDef.AlternateNames)
                {
                    // Select the smallest symbol option.
                    if (item.Length < unitDef.Symbol.Length)
                        return item + powerSymbol;
                }
            }
            // Otherwise output primary symbol
            return unitDef.Symbol + powerSymbol;
        }
        
        

        /// <summary>
        /// Casts value to a value with a given precision. If precision is greater than absolutePrecision, then falls back to standard D format with given minimumPrecision.
        /// </summary>
        /// <param name="value">Input Value</param>
        /// <param name="precision">Number of zeros after ones place</param>
        /// <param name="minimumPrecision">Minimum number of zeros after ones place (Default: 0)</param>
        /// <param name="absolutePrecision">Absolute limit for precision, prevents excessive string format outputs and precisions higher than input values are capable of storing. (Default: 10)</param>
        /// <returns>Value represented to given precision as a string.</returns>
        internal virtual string FormatDecimal(double value, int precision, int minimumPrecision = 0, int absolutePrecision = 10)
        {
            string format;
            int loops = Math.Max(precision, minimumPrecision);

            if (loops <= 0)
                format = "0";

            else if (loops < absolutePrecision)
                format = $"0.{new String('0', minimumPrecision)}{new String('#', loops - minimumPrecision)}";

            else
                format = $"N{Math.Min(minimumPrecision, 10)}";

            return string.Format(CultureInfo.InvariantCulture, $"{{0:{format}}}", value);
        }
    }
}
