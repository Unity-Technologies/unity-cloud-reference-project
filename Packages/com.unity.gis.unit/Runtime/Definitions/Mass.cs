
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Measure of its resistance to acceleration when a net force is applied.
    /// </summary>
    public class Mass : UnitDef<Mass>, IMass
    {
        /// <summary>
        /// Constructor where the <see cref="Formula"/> is a passthrough by returning the same value (<see cref="UnitDef{T}.Scale"/> of 1).
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Mass(UnitNaming naming, bool isAbstract = false) :
            base(naming, isAbstract : isAbstract) { }

        /// <summary>
        /// Constructor that will instantiate a <see cref="Formula"/> based on the given <paramref name="formula"/> function.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Mass(UnitNaming naming, Func<double, double> formula, bool isAbstract = false) : 
            base(naming, formula, isAbstract : isAbstract) { }

        /// <summary>
        /// Constructor that will copy the <see cref="Formula"/> of the given <paramref name="copy"/> <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="copy"><see cref="IUnitDef"/> to copy its <see cref="Formula"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Mass(UnitNaming naming, IUnitDef copy, bool isAbstract = false) : 
            base(naming, copy, isAbstract : isAbstract) { }

        /// <summary>
        /// Main <see cref="Mass"/> constructor.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Mass(UnitNaming naming, IFormula formula, bool isAbstract = false) : 
            base(naming, formula, isAbstract : isAbstract) { }
    }
}
