
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Physical quantity that expresses hot and cold.
    /// </summary>
    public class Temperature : UnitDef<Temperature>, ITemperature
    {
        
        /// <summary>
        /// Constructor where the <see cref="Formula"/> is a passthrough by returning the same value (<see cref="UnitDef{T}.Scale"/> of 1).
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Temperature(UnitNaming naming, bool isAbstract = false) :
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
        /// <param name="corrective">Since temperatures don't have their zero at the same point, set the slope difference.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Temperature(UnitNaming naming, Func<double, double> formula, double corrective = 0.0, bool isAbstract = false) : 
            base(naming, formula, isAbstract : isAbstract) 
        { Corrective = corrective; }

        /// <summary>
        /// Constructor that will copy the <see cref="Formula"/> of the given <paramref name="copy"/> <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="copy"><see cref="IUnitDef"/> to copy its <see cref="Formula"/>.</param>
        /// <param name="corrective">Since temperatures don't have their zero at the same point, set the slope difference.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Temperature(UnitNaming naming, IUnitDef copy, double corrective = 0.0, bool isAbstract = false) : 
            base(naming, copy, isAbstract : isAbstract)
        { Corrective = corrective; }

        /// <summary>
        /// Main <see cref="Temperature"/> constructor.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="corrective">Since temperatures don't have their zero at the same point, set the slope difference.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        public Temperature(UnitNaming naming, IFormula formula, double corrective = 0.0, bool isAbstract = false) : 
            base(naming, formula, isAbstract : isAbstract)
        { Corrective = corrective; }

        /// <inheritdoc cref="ITemperature.Corrective"/>
        public double Corrective { get; }

        /// <inheritdoc cref="ITemperature.ToFormula(ITemperature, IFormula, byte)"/>
        public Func<double, double> ToFormula(ITemperature unitDef, IFormula formula, byte power = 0)
        {
            if (this == unitDef)
                return value => value;
            
            if (power == 0)
                power = PowerMin;

            double thisScale = IsAbstractUnit
                ? formula.Invoke(Scale)
                : Scale;

            double scale = Math.Pow(thisScale, power) / Math.Pow(unitDef.Scale, power);

            return value => scale * (value - Corrective) + unitDef.Corrective;
        }

        /// <inheritdoc cref="UnitDef{T}.ToFormula(IUnitDef, IFormula, byte)"/>
        public override Func<double, double> ToFormula(IUnitDef unitDef, IFormula formula, byte power = 0) =>
            ToFormula(unitDef as ITemperature, formula, power);
    }
}
