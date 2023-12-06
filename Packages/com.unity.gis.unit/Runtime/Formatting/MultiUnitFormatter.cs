using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Converts a single unit into a composite of output units where the remainder of larger units is converted
    /// into smaller units. Each value is formatted with the given IUnitFormatter, and concatenated into the output string.
    /// </summary>
    public class MultiUnitFormatter : IUnitFormatter
    {
        /// <summary>
        /// Units in order of use
        /// </summary>
        readonly IUnitDef[] m_Units;

        /// <summary>
        /// When set to true, if a unit would print a zero then it would be ignored
        /// unless it's the only unit.
        /// </summary>
        readonly bool m_DropZeroScales;

        /// <summary>
        /// Smallest absolute non-zero value used
        /// </summary>
        readonly double m_Precision = 0.000001;

        /// <summary>
        /// Base formatter used for individual units.
        /// </summary>
        readonly IUnitFormatter m_BaseFormatter;

        /// <summary>
        /// Converts a single unit into a composite of output units where the remainder of larger units is converted
        /// into smaller units. Each value is formatted with <see cref="UnitFormatter"/>, and concatenated into the output string.
        /// </summary>
        /// <param name="units">List of Unit Definitions passed in the order they are printed. The remainder of an
        /// interger of the first value is passed to the second, for that reason it is recommended to order the values
        /// from larger to smaller.</param>
        /// <param name="dropZeroScales">When set to true, if a unit would print a zero then it would be ignored
        /// unless it's the only unit. (Default: True)</param>
        /// <param name="fullStringFormat">When true, the full string name & power are used when formatting.
        /// (Default: False)</param>
        /// <param name="hasSuperscript">When true, if the passed unit has a power higher than 1 then the resulting
        /// string will include the power or superscript symbol. (Default: True)</param>
        /// <param name="decimalPrecision">The maximum zero place to output when formatting values.
        /// (Default: Int32.MaxValue)</param>
        /// <param name="minimumPrecision">The minimum zero place to output when formatting values. (Default: 0)</param>
        public MultiUnitFormatter(IUnitDef[] units, bool dropZeroScales = true, bool fullStringFormat = false, bool hasSuperscript = true, int decimalPrecision = Int32.MaxValue, int minimumPrecision = 0)
        {
            // Configure default scales
            if (units == null || units.Length == 0)
                throw new NullUnitDefException("List of Units required");
            m_Units = units;
            m_DropZeroScales = dropZeroScales;
            m_BaseFormatter = new UnitFormatter(fullStringFormat, hasSuperscript, decimalPrecision, minimumPrecision);
        }

        /// <inheritdoc cref="IFormatProvider.GetFormat(Type)"/>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(IUnitFormatter)
                ? this
                : null;
        }

        /// <inheritdoc cref="ICustomFormatter.Format(string, object, IFormatProvider)"/>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return arg is not IUnit unit
                ? arg.ToString()
                : Format(unit);
        }

        /// <inheritdoc cref="IUnitFormatter.Format(IUnit)"/>
        public string Format(IUnit value)
        {
            return MultiUnitFormatterHelper.Format(
                value,
                m_Units,
                m_BaseFormatter,
                m_DropZeroScales,
                m_Precision);
        }
    }
}
