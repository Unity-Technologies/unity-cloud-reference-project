
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Unit Definition allowing to convert a <see cref="Unit"/> from one <see cref="UnitDefPower{T}"/> instance to an other
    /// or to a <see cref="UnitDef{T}"/> with the <see cref="Unit"/>.<see cref="Unit.Power"/> value set to the same value.
    /// </summary>
    /// <typeparam name="T">Set the class <see cref="UnitDefPower{T}"/> derives from.</typeparam>
    public abstract class UnitDefPower<T> : UnitDef<T>
        where T : UnitDefPower<T>
    {
        /// <summary>
        /// true if the superscript power is part of the string format; false otherwise.
        /// </summary>
        /// <example>
        /// <code>
        /// Volume cm3 = Si.Centimeter3.From(5);
        /// Console.WriteLine(cm3);  // 5 cm³
        /// Volume liter = Si.Liter.From(5);
        /// Console.WriteLine(liter);  // 5 l
        /// </code>
        /// </example>
        private readonly bool m_HasSuperscript;
        
        /// <inheritdoc cref="UnitDef{T}.ScalePow(IUnit)"/>
        public override double ScalePow(IUnit unit) =>
            Scale;
        
        /// <summary>
        /// Constructor where the <see cref="Formula"/> is a passthrough by returning the same value (Scale of 1).
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="power">Restrict <see cref="Unit"/>.<see cref="Unit.Power"/> to this value.</param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDefPower(UnitNaming naming, byte power, bool hasSuperscript = false, bool isAbstract = false, bool register = true) :
            base(naming, power, power, isAbstract, register) 
        { m_HasSuperscript = hasSuperscript; }

        /// <summary>
        /// Constructor that will instantiate a <see cref="Formula"/> based on the given <paramref name="formula"/> function.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="power">Restrict <see cref="Unit"/>.<see cref="Unit.Power"/> to this value.</param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDefPower(UnitNaming naming, Func<double, double> formula, byte power, bool hasSuperscript = false, bool isAbstract = false, bool register = true) : 
            base(naming, formula, power, power, isAbstract, register) 
        { m_HasSuperscript = hasSuperscript; }

        /// <summary>
        /// Constructor that will copy the <see cref="Formula"/> of the given <paramref name="copy"/> <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="copy"><see cref="IUnitDef"/> to copy its <see cref="Formula"/>.</param>
        /// <param name="power">Restrict <see cref="Unit"/>.<see cref="Unit.Power"/> to this value.</param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDefPower(UnitNaming naming, IUnitDef copy, byte power, bool hasSuperscript = false, bool isAbstract = false, bool register = true) : 
            base(naming, copy, power, power, isAbstract, register) 
        { m_HasSuperscript = hasSuperscript; }

        /// <summary>
        /// Main <see cref="UnitDefPower{T}"/> constructor.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="UnitDef{T}.BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="UnitDef{T}.Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="power">Restrict <see cref="Unit"/>.<see cref="Unit.Power"/> to this value.</param>
        /// <param name="hasSuperscript">true if the superscript power is part of the string format; false otherwise.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="UnitDef{T}.Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDefPower(UnitNaming naming, IFormula formula, byte power, bool hasSuperscript = false, bool isAbstract = false, bool register = true) : 
            base(naming, formula, power, power, isAbstract, register) 
        { m_HasSuperscript = hasSuperscript; }

        /// <inheritdoc cref="UnitDef{T}.ToFormula(IUnitDef, IFormula, byte)"/>
        public override Func<double, double> ToFormula(IUnitDef unitDef, IFormula formula, byte power = 0)
            {
                if (this == unitDef)
                    return value => value;

                double thisScale = IsAbstractUnit
                    ? formula.Invoke(Scale)
                    : Scale;

                double scale = thisScale / unitDef.Scale;

                return value => scale * value;
            }
        
        /// <inheritdoc cref="UnitDef{T}.ToFullStringPower(byte)"/>
        public override string ToFullStringPower(byte power) => 
            m_HasSuperscript
                ? base.ToFullStringPower(power) 
                : "";

        /// <inheritdoc cref="UnitDef{T}.ToStringPower(byte)"/>
        public override string ToStringPower(byte power) => 
            m_HasSuperscript
                ? base.ToStringPower(power) 
                : "";
    }
}
