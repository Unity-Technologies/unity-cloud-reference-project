using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// <see cref="IUnitFormatter"/> to format an <see cref="IUnit"/> into a string where decimal values become mixed number fractions.
    /// 1.25 becomes 1 1/4
    /// </summary>
    public class FractionalUnitFormatter : UnitFormatter
    {
        /// <summary>
        /// Default Fractional unit formatter
        /// </summary>
        public static readonly IUnitFormatter FractionalFormatter = new FractionalUnitFormatter();

        /// <summary>
        /// Default Full String Fractional formatter
        /// </summary>
        public static readonly IUnitFormatter FractionalFullStringFormatter = new FractionalUnitFormatter(true);

        /// <summary>
        /// When true, the fraction is wrapped in rich text symbols
        /// `<sup>` and `<sub>` for TextMeshPro & HTML.
        /// </summary>
        readonly bool m_RichText = false;

        const string k_FormatFractionRichText = "<sup>{0}</sup>/<sub>{1}</sub>";

        const string k_FormatFraction = "{0}/{1}";

        /// <summary>
        /// Formats units so decimal values are fractions. This is common with Imperial units.
        /// </summary>
        /// <param name="fullStringFormat">When true, the full string name & power are used when formatting.
        /// (Default: False)</param>
        /// <param name="hasSuperscript">When true, if the passed unit has a power higher than 1 then the resulting
        /// string will include the power or superscript symbol. (Default: True)</param>
        /// <param name="precision">The finest precision that should be used. 8 = 1/8, 16 = 1/16, 32 = 1/32.
        /// (Default: 16)</param>
        /// <param name="richText">When true, the fraction is wrapped in rich text symbols
        /// `<sup>` and `<sub>` for TextMeshPro & HTML. (Default: False)</param>
        public FractionalUnitFormatter(bool fullStringFormat = false, bool hasSuperscript = false, int precision = 16, bool richText = false) : base(fullStringFormat, hasSuperscript, decimalPrecision: precision)
        {
            m_RichText = richText;
        }

        /// <summary>
        /// Overrides the constructor to expose all formatting options.
        /// </summary>
        /// <param name="unitDef">Input unit definition</param>
        /// <param name="value">Input numeric value</param>
        /// <param name="fullString">format as a full string</param>
        /// <param name="power">power</param>
        /// <returns>A formatted string.</returns>
        internal override string FormatDecimal(double value, int precision, int minimumPrecision = 0, int absolutePrecision = 10)
        {
            var intValue = (int)value;
            var remainder = Math.Abs(value) % 1;
            if (remainder + 1.0 / (precision * 2.0) > 1.0)
            {
                return $"{intValue + 1}";
            }

            var fraction = Fractional(remainder, precision, m_RichText);
            if (!string.IsNullOrWhiteSpace(fraction))
            {
                if (intValue != 0)
                    return $"{intValue} {fraction}";
                return $"{((value < 0)?"-":"")}{fraction}";
            }
            return  $"{intValue}";
        }


        /// <summary>
        /// Round a floating point number into an imperial fractional value.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="fractionPrecision">Finest precision to round to</param>
        /// <param name="richText">When true, the fraction is wrapped in rich text symbols
        /// `<sup>` and `<sub>` for TextMeshPro & HTML. (Default: False)</param>
        /// <returns></returns>
        static string Fractional(double value, int fractionPrecision, bool richText = false)
        {
            var denominator = fractionPrecision;
            var numerator = value * fractionPrecision;
            var intNumerator = (int)Math.Round(numerator);


            while (intNumerator > 1 && (intNumerator % 2 == 0 && denominator / 2 > 1))
            {
                numerator /= 2;
                intNumerator = (int)Math.Round(numerator);
                denominator /= 2;
            }

            if (intNumerator <= 0)
                return "";

            return richText
                ? string.Format(k_FormatFractionRichText, intNumerator, denominator)
                : string.Format(k_FormatFraction, intNumerator, denominator);
        }
    }
}
