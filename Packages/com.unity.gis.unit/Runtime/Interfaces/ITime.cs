
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Interface for the quantify rates of change of quantities in material reality.
    /// </summary>
    public interface ITime : IUnitDef
    {
        /// <summary>
        /// Convert a <see cref="TimeSpan"/> to this instance of <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="time"><see cref="TimeSpan"/> to convert.</param>
        /// <returns>
        /// Create a new <see cref="Unit"/> with this instance set as the <see cref="Unit"/>.<see cref="Unit.UnitDef"/>
        /// and set the converted value to <see cref="Unit"/>.<see cref="Unit.Value"/>.
        /// </returns>
        Unit From(TimeSpan time);
    }
}
