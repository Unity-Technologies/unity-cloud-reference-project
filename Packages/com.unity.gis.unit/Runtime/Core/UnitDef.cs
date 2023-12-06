
using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Unit Definition allowing to convert a <see cref="Unit"/> from one <see cref="UnitDef{T}"/> instance to an other.
    /// </summary>
    /// <typeparam name="T">Set the class <see cref="UnitDef{T}"/> derives from.</typeparam>
    public abstract class UnitDef<T> : IUnitDef
        where T : UnitDef<T>
    {
        /// <inheritdoc cref="UnitNaming"/>
        public readonly UnitNaming Naming;

        /// <inheritdoc cref="IUnitDef.AlternateNames"/>
        public string[] AlternateNames => Naming.AlternativeNaming;

        /// <summary>
        /// Store a unique base <see cref="UnitDef{T}"/> per T.
        /// The base <see cref="UnitDef{T}"/> is the main unit used for a type. This is also the type that is normally
        /// used when serializing / storing a <see cref="Unit"/> to a unique <see cref="UnitDef{T}"/>.
        /// </summary>
        /// <remarks>The base unit does not always have a <see cref="Scale"/> of 1, it's a arbitrary default <see cref="UnitDef{T}"/> to use with T.</remarks>
        private static UnitDef<T> s_BaseUnitDef;
        
        /// <inheritdoc cref="IUnitDef.Formula"/>
        public IFormula Formula { get; }
        
        /// <inheritdoc cref="IUnitDef.IsAbstractUnit"/>
        public bool IsAbstractUnit { get; }

        /// <inheritdoc cref="IUnitDef.Name"/>
        public string Name => Naming.Name;

        /// <summary>
        /// Main full name defining the <see cref="UnitDef{T}"/> in its plural form.
        /// </summary>
        /// <remarks>If no plural form was given at construction time, this will be same as <see cref="Name"/> ending with an "s".</remarks>
        public string NamePlural => Naming.NamePlural;
        
        /// <summary>
        /// Maximum possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.
        /// </summary>
        /// <remarks>If their is no maximum value, it will be set to a lower value than <see cref="PowerMin"/>.</remarks>
        public byte PowerMax { get; }
        
        /// <inheritdoc cref="IUnitDef.PowerMin"/>
        public byte PowerMin { get; }

        /// <inheritdoc cref="IUnitDef.Symbol"/>
        public string Symbol => Naming.Symbol;
        
        
        /// <summary>
        /// Initialize a <see cref="Registered"/> key by creating a <see cref="List{T}"/> for this <see cref="UnitDef{T}"/>.
        /// </summary>
        static UnitDef() => 
            Mapping.Register<T>();


        /// <summary>
        /// Main <see cref="UnitDef{T}"/> constructor.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="powerMin">Minimal possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="powerMax">Maximum possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDef(UnitNaming naming, IFormula formula, byte powerMin = 1, byte powerMax = 3, bool isAbstract = false, bool register = true)
        {
            Naming = naming;
            IsAbstractUnit = isAbstract;
            Formula = formula;
            PowerMin = powerMin;
            PowerMax = powerMax;
            
            if (register)
                Mapping.Register(this);
        }
        
        /// <summary>
        /// Constructor that will instantiate a <see cref="Formula"/> based on the given <paramref name="formula"/> function.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="formula">
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </param>
        /// <param name="powerMin">Minimal possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="powerMax">Maximum possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDef(UnitNaming naming, Func<double, double> formula, byte powerMin = 1, byte powerMax = 3, bool isAbstract = false, bool register = true) :
            this(naming, new Formula(formula), powerMin, powerMax, isAbstract, register) { }
        
        /// <summary>
        /// Constructor that will copy the <see cref="Formula"/> of the given <paramref name="copy"/> <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="copy"><see cref="IUnitDef"/> to copy its <see cref="Formula"/>.</param>
        /// <param name="powerMin">Minimal possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="powerMax">Maximum possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDef(UnitNaming naming, IUnitDef copy, byte powerMin = 1, byte powerMax = 3, bool isAbstract = false, bool register = true) :
            this(naming, copy.Formula, powerMin, powerMax, isAbstract, register) { }

        /// <summary>
        /// Constructor where the <see cref="Formula"/> is a passthrough by returning the same value (<see cref="Scale"/> of 1).
        /// </summary>
        /// <param name="naming">Naming definition of the <see cref="UnitDef{T}"/>.</param>
        /// <param name="powerMin">Minimal possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="powerMax">Maximum possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.</param>
        /// <param name="isAbstract">true if the unit represent a none defined unit which require external <see cref="Scale"/> value to be converted; false otherwise.</param>
        /// <param name="register">
        /// true if the unit will be part of the parsing when converting a string to a <see cref="Unit"/>;
        /// false if the definition is to be used in a self contained scope.
        /// </param>
        protected UnitDef(UnitNaming naming, byte powerMin = 1, byte powerMax = 3, bool isAbstract = false, bool register = true) :
            this(naming, value => value, powerMin, powerMax, isAbstract, register) { }


        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing an addition between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <param name="value">Add this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator +(UnitDef<T> unitDef, double value) => 
            new(u => unitDef.Formula.Invoke(u) + value);

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing an addition between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="value">Add this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator +(double value, UnitDef<T> unitDef) => 
            new(u => value + unitDef.Formula.Invoke(u));

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a subtraction between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <param name="value">Subtract this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator -(UnitDef<T> unitDef, double value) => 
            new(u => unitDef.Formula.Invoke(u) - value);

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a subtraction between a double value and
        /// a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>.
        /// </summary>
        /// <param name="value">Subtract <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result with this value.</param>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator -(double value, UnitDef<T> unitDef) => 
            new(u => value - unitDef.Formula.Invoke(u));
        
        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a multiplication between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <param name="value">Multiply this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator *(UnitDef<T> unitDef, double value) => 
            new(u => unitDef.Formula.Invoke(u) * value);

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a multiplication between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="value">Multiply this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator *(double value, UnitDef<T> unitDef) => 
            new(u => value * unitDef.Formula.Invoke(u));

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a division between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <param name="value">Divide this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator /(UnitDef<T> unitDef, double value) => 
            new(u => unitDef.Formula.Invoke(u) / value);

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a division between a double value and
        /// a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>.
        /// </summary>
        /// <param name="value">Divide <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result with this value.</param>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator /(double value, UnitDef<T> unitDef) => 
            new(u => value / unitDef.Formula.Invoke(u));

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a modulo between a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>
        /// and a double value.
        /// </summary>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <param name="value">Calculate the remaining of this value to the <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator %(UnitDef<T> unitDef, double value) => 
            new(u => unitDef.Formula.Invoke(u) % value);

        /// <summary>
        /// Construct a new <see cref="Formula"/> by doing a modulo between a double value and
        /// a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/>.
        /// </summary>
        /// <param name="value">Get the remaining of <paramref name="unitDef"/>.<see cref="UnitDef{T}.Formula"/> result with this value.</param>
        /// <param name="unitDef">The <see cref="UnitDef{T}"/> to use its <see cref="Formula"/>.</param>
        /// <returns>The newly created <see cref="Formula"/>.</returns>
        public static Formula operator %(double value, UnitDef<T> unitDef) => 
            new(u => value % unitDef.Formula.Invoke(u));
        

        
        /// <inheritdoc cref="IUnitDef.From(double, byte)"/>
        public Unit From(double value, byte power = 0)
        {
            if (power == 0)
                power = PowerMin;
                
            return new Unit(value, this, power);
        }

        /// <inheritdoc cref="IUnitDef.From(string, byte)"/>
        public Unit From(string text, byte power = 0)
        {
            double value;
            IUnitDef unitParsed;
            byte powerParsed;
            
            try
            {
                Parser.FromStringToComponentUnit(text, new[] {GetType()}, out value, out unitParsed, out powerParsed);
            }
            catch (UnitPatternException)  // If the string has no unit, only digits.
            {
                return From(
                    Parser.FromStringToDouble(text),
                    power);
            }

            if (power == 0)  // If no power is specified, extract it from the string.
                power = powerParsed;
            
            if (ReferenceEquals(this, unitParsed))  // If the unit is the same from the text.
                return From(value, power);

            if (GetType() != unitParsed.GetType())  // If the unit is not of the same type, it cannot be converted.
                throw new DifferentTypesException(
                    $"The expected type of {this} does not match the unit type {unitParsed} part of '{text}'.",
                    nameof(text));

            // The unit from the text is different, convert it.
            value = unitParsed.To(value, this, power);
            return From(value, power);
        }

        /// <summary>
        /// Create a new <see cref="Unit"/> instance for each the given <paramref name="text"/> and restricting the search
        /// with only <see cref="IUnitDef"/> of this type.
        /// </summary>
        /// <param name="text">Find the <see cref="Unit"/>.<see cref="Unit.Value"/> part of each item.</param>
        /// <returns>A newly created <see cref="Unit"/> instance for each on the given string.</returns>
        /// <exception cref="DifferentTypesException">
        /// If a unit is part of the <paramref name="text"/> and is not compatible with this <see cref="UnitDef{T}"/> instance.
        /// </exception>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match the given <see cref="UnitDef{T}"/> pattern.</exception>
        /// <exception cref="ExponentPatternException">If the given <see cref="Unit"/>.<see cref="Unit.Power"/> value is not compatible with the decoded <see cref="Unit"/>.<see cref="Unit.UnitDef"/>.</exception>
        public static IEnumerable<Unit> From(IEnumerable<string> text) =>
            Unit.From(text, typeof(T));

        /// <inheritdoc cref="IUnitDef.From(IUnit)"/>
        public Unit From(IUnit value) =>
            value.To(this);

        /// <inheritdoc cref="IUnitDef.From(IUnit, double)"/>
        public Unit From(IUnit value, double scale) =>
            value.To(this, scale);

        /// <inheritdoc cref="IUnitDef.From(IUnit, Func{double, double})"/>
        public Unit From(IUnit value, Func<double, double> formula) =>
            value.To(this, formula);

        /// <inheritdoc cref="IUnitDef.From(IUnit, IFormula)"/>
        public Unit From(IUnit value, IFormula formula) =>
            value.To(this, formula);

        /// <inheritdoc cref="IUnitDef.To(double, IUnitDef, byte)"/>
        public double To(double value, IUnitDef unitDef, byte power = 0)
        {
            ValidateSameType(unitDef);
            return ToFormula(unitDef, power)(value);
        }

        /// <inheritdoc cref="IUnitDef.To(double, IUnitDef, double, byte)"/>
        public double To(double value, IUnitDef unitDef, double scale, byte power = 0)
        {
            ValidateSameType(unitDef);
            return ToFormula(unitDef, scale, power)(value);
        }

        /// <inheritdoc cref="IUnitDef.To(double, IUnitDef, Func{double, double}, byte)"/>
        public double To(double value, IUnitDef unitDef, Func<double, double> formula, byte power = 0)
        {
            ValidateSameType(unitDef);
            return ToFormula(unitDef, formula, power)(value);
        }

        /// <inheritdoc cref="IUnitDef.To(double, IUnitDef, IFormula, byte)"/>
        public double To(double value, IUnitDef unitDef, IFormula formula, byte power = 0)
        {
            ValidateSameType(unitDef);
            return ToFormula(unitDef, formula, power)(value);
        }

        /// <inheritdoc cref="IUnitDef.To(IEnumerable{double}, IUnitDef, byte)"/>
        public IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, byte power = 0)
        {
            ValidateSameType(unitDef);
            Func<double, double> toInvoke = ToFormula(unitDef, power);

            return values.Select(each => toInvoke(each)).AsEnumerable();
        }

        /// <inheritdoc cref="IUnitDef.To(IEnumerable{double}, IUnitDef, double, byte)"/>
        public IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, double scale, byte power = 0)
        {
            ValidateSameType(unitDef);
            Func<double, double> toInvoke = ToFormula(unitDef, scale, power);

            return values.Select(each => toInvoke(each)).AsEnumerable();
        }

        /// <inheritdoc cref="IUnitDef.To(IEnumerable{double}, IUnitDef, Func{double, double}, byte)"/>
        public IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, Func<double, double> formula, byte power = 0)
        {
            ValidateSameType(unitDef);
            Func<double, double> toInvoke = ToFormula(unitDef, formula, power);

            return values.Select(each => toInvoke(each)).AsEnumerable();
        }

        /// <inheritdoc cref="IUnitDef.To(IEnumerable{double}, IUnitDef, IFormula, byte)"/>
        public IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, IFormula formula, byte power = 0)
        {
            ValidateSameType(unitDef);
            Func<double, double> toInvoke = ToFormula(unitDef, formula, power);

            return values.Select(each => toInvoke(each)).AsEnumerable();
        }

        /// <inheritdoc cref="IUnitDef.ToFormula(IUnitDef, byte)"/>
        public Func<double, double> ToFormula(IUnitDef unitDef, byte power = 0) =>
            ToFormula(unitDef, 1.0, power);

        /// <inheritdoc cref="IUnitDef.ToFormula(IUnitDef, double, byte)"/>
        public Func<double, double> ToFormula(IUnitDef unitDef, double scale, byte power = 0) =>
            ToFormula(unitDef, new Formula(value => value), power);

        /// <inheritdoc cref="IUnitDef.ToFormula(IUnitDef, Func{double, double}, byte)"/>
        public Func<double, double> ToFormula(IUnitDef unitDef, Func<double, double> formula, byte power = 0) =>
            ToFormula(unitDef, new Formula(formula), power);

        /// <inheritdoc cref="IUnitDef.ToFormula(IUnitDef, IFormula, byte)"/>
        public virtual Func<double, double> ToFormula(IUnitDef unitDef, IFormula formula, byte power = 0)
        {
            if (this == unitDef)
                return value => value;
            
            if (power == 0)
                power = PowerMin;

            double thisScale = IsAbstractUnit
                ? formula.Invoke(Scale)
                : Scale;

            double scale = Math.Pow(thisScale, power) / Math.Pow(unitDef.Scale, power);

            return value => scale * value;
        }

        /// <inheritdoc cref="IUnitDef.ToFullString(double, byte, IFormatProvider)"/>
        public virtual string ToFullString(double value, byte power = 1, IFormatProvider provider = null) =>
            Math.Abs(value - 1.0) < Unit.Tolerance
                ? $"{value} {ToFullStringPower(power)}{Name.ToLower()}"
                : $"{value} {ToFullStringPower(power)}{NamePlural.ToLower()}";

        /// <inheritdoc cref="IUnitDef.ToFullStringPower(byte)"/>
        public virtual string ToFullStringPower(byte power) =>
            power switch
            {
                1 => "",
                2 => "square ",
                3 => "cubic ",
                4 => "quartic ",
                5 => "quintic ",
                6 => "sextic ",
                7 => "septic ",
                8 => "octic ",
                9 => "nonic ",
                10 => "decic ",
                _ => $"power of {power} "
            };

        /// <summary>
        /// Get the short name (<see cref="Symbol"/>) that should be used as a suffix for
        /// <see cref="Unit"/>.<see cref="Unit.ToString()"/> with the <see cref="PowerMin"/> set as the power value.
        /// </summary>
        /// <returns>The default short name.</returns>
        public override string ToString() => 
            $"{Name}{ToStringPower(PowerMin)}";

        /// <inheritdoc cref="IUnitDef.ToString(double, byte, IFormatProvider)"/>
        public virtual string ToString(double value, byte power = 1, IFormatProvider provider = null) =>
            $"{value}{Symbol}{ToStringPower(power)}";

        /// <inheritdoc cref="IUnitDef.ToStringPower(byte)"/>
        public virtual string ToStringPower(byte power)
        {
            if (power <= 1)
                return "";
            
            Dictionary<char, char> mapping = new Dictionary<char, char>
            {
                {'0', '⁰'},
                {'1', '¹'},
                {'2', '²'},
                {'3', '³'},
                {'4', '⁴'},
                {'5', '⁵'},
                {'6', '⁶'},
                {'7', '⁷'},
                {'8', '⁸'},
                {'9', '⁹'}
            };

            IEnumerable<char> output = power
                .ToString()
                .Select(each => mapping[each]);

            return string.Join("", output);
        }

        
        
        /// <summary>
        /// Addition operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override + operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the addition from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the addition with.</param>
        /// <returns>The addition result as a double-precision floating-point value.</returns>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        public virtual double Add(IUnit first, IUnit second)
        {
            ValidateOpSamePower(first, second);
            return first.Value + second.UnitDef.To(second.Value, first.UnitDef);
        }

        /// <summary>
        /// Subtraction operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override - operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the subtraction from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the subtraction with.</param>
        /// <returns>The subtraction result as a double-precision floating-point value.</returns>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        public virtual double Sub(IUnit first, IUnit second)
        {
            ValidateOpSamePower(first, second);
            return first.Value - second.UnitDef.To(second.Value, first.UnitDef);
        }

        /// <summary>
        /// Multiplication operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override * operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the multiplication from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the multiplication with.</param>
        /// <returns>The multiplication result as a double-precision floating-point value.</returns>
        /// <exception cref="IncompatibleTypesException">If both <see cref="UnitDef{T}"/> are not compatible.</exception>
        public virtual double Mult(IUnit first, IUnit second)
        {
            ValidateOpCompatibleType(second.UnitDef);
            IUnitDef secondUnitDef = first.Power == second.Power
                ? first.UnitDef
                : Mapping.GetPowerUnitDef(first.UnitDef, second.Power);
            return first.Value * second.UnitDef.ToFormula(secondUnitDef, second.Power)(second.Value);
        }

        /// <summary>
        /// Division operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override / operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the division from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the division with.</param>
        /// <returns>The division result as a double-precision floating-point value.</returns>
        /// <exception cref="IncompatibleTypesException">If both <see cref="UnitDef{T}"/> are not compatible.</exception>
        public virtual double Div(IUnit first, IUnit second)
        {
            ValidateOpCompatibleType(second.UnitDef);
            IUnitDef secondUnitDef = first.Power == second.Power
                ? first.UnitDef
                : Mapping.GetPowerUnitDef(first.UnitDef, second.Power);
            return first.Value / second.UnitDef.ToFormula(secondUnitDef, second.Power)(second.Value);
        }

        /// <summary>
        /// Modulo operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override % operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the modulo from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the modulo with.</param>
        /// <returns>The modulo result as a double-precision floating-point value.</returns>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        public virtual double Modulo(IUnit first, IUnit second)
        {
            ValidateOpSamePower(first, second);
            return first.Value % second.UnitDef.To(second.Value, first.UnitDef);
        }

        /// <inheritdoc cref="IUnitDef.BaseUnitDef"/>
        public IUnitDef BaseUnitDef
        {
            get
            {
                Mapping.CallStaticUnits();
                return s_BaseUnitDef;
            }
        }

        /// <inheritdoc cref="IUnitDef.Names"/>
        public IEnumerable<string> Names
        {
            get
            {
                yield return Name;
                yield return NamePlural;
                yield return Symbol;
                foreach (string alt in AlternateNames)
                    yield return alt;
            }
        }

        /// <inheritdoc cref="IUnitDef.Pow(double)"/>
        public IFormula Pow(double power) =>
            new Formula(Formula, value => Math.Pow(value, power));
        
        /// <summary>
        /// Get all the <see cref="UnitDef{T}"/> registered for this type.
        /// These are the types used to generate the <see cref="System.Text.RegularExpressions.Regex"/> for this type.
        /// </summary>
        public static IEnumerable<UnitDef<T>> Registered =>
            Mapping.GetRegistered<UnitDef<T>>();

        /// <inheritdoc cref="IUnitDef.RegisterAsBaseUnit"/>
        public void RegisterAsBaseUnit() => RegisterBaseUnit(this);
        
        /// <summary>
        /// Sets or updates the base unit
        /// </summary>
        /// <param name="unit">Unit to set as the base</param>
        /// <exception cref="MultiBaseUnitException">If there is already a unit set that's not this object.</exception>
        static void RegisterBaseUnit(UnitDef<T> unit) => 
            s_BaseUnitDef = s_BaseUnitDef is null || ReferenceEquals(s_BaseUnitDef, unit)
                ? unit
                : throw new MultiBaseUnitException(typeof(T), s_BaseUnitDef, unit);

        /// <inheritdoc cref="IUnitDef.RegisterPowerUnitDef(byte, IUnitDef)"/>
        public void RegisterPowerUnitDef(byte power, IUnitDef powerUnitDef) =>
            Mapping.RegisterPowerUnitDef(this, power, powerUnitDef);
        
        /// <inheritdoc cref="IUnitDef.RegisterPowerUnitDefs(Dictionary{byte,IUnitDef})"/>
        public void RegisterPowerUnitDefs(Dictionary<byte, IUnitDef> mapping) =>
            Mapping.RegisterPowerUnitDefs(this, mapping);
        
        /// <inheritdoc cref="Mapping.RegisterPowerUnitDefs(IUnitDef[])"/>
        public static void RegisterPowerUnits(params IUnitDef[] unitDefs) =>
            Mapping.RegisterPowerUnitDefs(unitDefs);
        
        /// <inheritdoc cref="IUnitDef.Scale"/>
        public double Scale =>
            Formula.Invoke(1.0);

        /// <inheritdoc cref="IUnitDef.ScaleAbstract(double)"/>
        public double ScaleAbstract(double scale) =>
            Scale * scale;

        /// <inheritdoc cref="IUnitDef.ScaleAbstract(Func{double,double})"/>
        public double ScaleAbstract(Func<double, double> formula) =>
            ScaleAbstract(formula(1));

        /// <inheritdoc cref="IUnitDef.ScaleAbstract(IFormula)"/>
        public double ScaleAbstract(IFormula formula) =>
            ScaleAbstract(formula.Invoke(1));

        /// <inheritdoc cref="IUnitDef.ScalePow(IUnit)"/>
        public virtual double ScalePow(IUnit unit)
        {
            double scale = IsAbstractUnit
                ? ScaleAbstract(unit.AbstractFormula)
                : Scale;
            
            return Math.Pow(scale, unit.Power);
        }

        
        
        /// <summary>
        /// Validate this <see cref="UnitDef{T}"/> instance as a corresponding type for an other <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </summary>
        /// <param name="unitDef">The other <see cref="UnitDef{T}"/> to compare with.</param>
        /// <exception cref="IncompatibleTypesException">If both <see cref="UnitDef{T}"/> are not compatible.</exception>
        private void ValidateOpCompatibleType(IUnitDef unitDef)
        {
            if (!Mapping.AreCompatibleTypes(this, unitDef))
                throw new IncompatibleTypesException(this, unitDef, nameof(unitDef));
        }
        
        /// <summary>
        /// Validate two <see cref="IUnit"/> have the same <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </summary>
        /// <param name="first">First <see cref="IUnit"/> to compare with.</param>
        /// <param name="second">The other <see cref="IUnit"/> to compare with.</param>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        private static void ValidateOpSamePower(IUnit first, IUnit second)
        {
            if (first.Power != second.Power)
                throw new DifferentPowersException(first, second);
        }

        /// <summary>
        /// Validate this <see cref="UnitDef{T}"/> instance is of the same type of an other <see cref="UnitDef{T}"/>.
        /// </summary>
        /// <param name="unitDef">The other <see cref="UnitDef{T}"/> to compare with.</param>
        /// <exception cref="DifferentTypesException">If both <see cref="UnitDef{T}"/> are not of the same type.</exception>
        private void ValidateSameType(IUnitDef unitDef)
        {
            if (GetType() != unitDef.GetType()) 
                
                throw new DifferentTypesException(this, unitDef, nameof(unitDef));
        }

    }
}
