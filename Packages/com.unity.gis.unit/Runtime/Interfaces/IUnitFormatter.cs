using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Interface for custom <see cref="IFormatProvider"/> for <see cref="IUnit"/> to provide formatting for units.
    /// </summary>
    public interface IUnitFormatter : IFormatProvider, ICustomFormatter {}
}
