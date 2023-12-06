
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Quantity that expresses the extent of a two-dimensional region.
    /// This is the measurement of the square value of <see cref="Length"/>.
    /// </summary>
    public class Area : UnitDefPower<Area>, IArea
    {
        /// <summary>
        /// Constructor where the <see cref="Formula"/> is a passthrough by returning the same value (<see cref="UnitDef{T}.Scale"/> of 1).
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Area(UnitNaming naming, bool hasSuperscript = false, bool isAbstract = false) :
            base(naming, 2, hasSuperscript, isAbstract) { }
        
        /// <summary>
        /// Constructor that will instantiate a <see cref="Formula"/> based on the given <paramref name="formula"/> function.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Area(UnitNaming naming, Func<double, double> formula, bool hasSuperscript = false, bool isAbstract = false) : 
            base(naming, formula, 2, hasSuperscript, isAbstract) { }

        /// <summary>
        /// Constructor that will copy the <see cref="Formula"/> of the given <paramref name="copy"/> <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="copy"><see cref="IUnitDef"/> to copy its <see cref="Formula"/>.</param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Area(UnitNaming naming, IUnitDef copy, bool hasSuperscript = false, bool isAbstract = false) : 
            base(naming, copy, 2, hasSuperscript, isAbstract) { }

        /// <summary>
        /// Main <see cref="Area"/> constructor.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Area(UnitNaming naming, IFormula formula, bool hasSuperscript = false, bool isAbstract = false) : 
            base(naming, formula, 2, hasSuperscript, isAbstract) { }
    }
}
