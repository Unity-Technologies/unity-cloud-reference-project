
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Interface of physical quantity that expresses hot and cold.
    /// </summary>
    public interface ITemperature : IUnitDef
    {
        /// <summary>
        /// Slope correction to apply when converting a <see cref="Unit"/> from one <see cref="Temperature"/> definition
        /// to an other.
        /// </summary>
        /// <remarks>By default, the <see cref="Corrective"/> value is relative to <see cref="Si.Celsius"/>.</remarks>
        double Corrective { get; }

        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from this <see cref="ITemperature"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="ITemperature"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="IUnitDef.PowerMin"/> is not equal to <see cref="IUnitDef.PowerMax"/>, specify the power to convert.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from this
        /// <see cref="ITemperature"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        Func<double, double> ToFormula(ITemperature unitDef, IFormula formula, byte power = 0);
    }
}
