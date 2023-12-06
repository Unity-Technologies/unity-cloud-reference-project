using System;
using System.Collections.Generic;

namespace Unity.Geospatial.Unit
{
    internal static class MultiUnitFormatterHelper
    {
        /// <summary>
        /// Converts a single unit into a composite of output units where the remainder of larger units is converted
        /// into smaller units. Each value is formatted with the given IUnitFormatter, and concatenated into the output string.
        /// </summary>
        /// <param name="value">Input value to format into a string.</param>
        /// <param name="units">List of Unit Definitions passed in the order they are printed. The remainder of an
        /// interger of the first value is passed to the second, for that reason it is recommended to order the values
        /// from larger to smaller.</param>
        /// <param name="formatter">Configured IUnitFormatter used to format the individual units.</param>
        /// <param name="dropZeroScales">When set to true, if a unit would print a zero then it would be ignored
        /// unless it's the only unit. (Default: True)</param>
        /// <param name="precision">Smallest absolute non-zero value used (Default: 0.000001)</param>
        internal static string Format(IUnit value, IUnitDef[] units, IUnitFormatter formatter, bool dropZeroScales = true, double precision = 0.000001)
        {
            List<string> result = new ();

            var negative = value.Value < 0
                ? "-"
                : "";
            var leftOver = Math.Abs(value.Value);
            var previousDef = value.UnitDef;
            var lastIndex = units.Length - 1;

            for (int i = 0; i < units.Length; i++)
            {
                var unitDef = units[i];
                var formula = previousDef.ToFormula(unitDef);
                var unitValue = formula(leftOver);
                IUnit unit;

                if (i == lastIndex && unitValue > precision)
                {
                    unit = new Unit(unitValue, unitDef, value.Power);
                }
                else if (!dropZeroScales || unitValue >= 1)
                {
                    leftOver = unitValue % 1;
                    previousDef = unitDef;
                    unit = new Unit((int)unitValue, unitDef, value.Power);
                }
                else
                {
                    continue;
                }

                result.Add(unit.ToString(formatter));
            }

            return $"{negative}{string.Join(" ", result)}";
        }
    }
}
