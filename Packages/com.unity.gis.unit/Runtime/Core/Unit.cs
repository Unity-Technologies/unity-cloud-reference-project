
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Unit value linking a double-precision floating-point with a <see cref="IUnitDef"/> allowing to convert
    /// the value to other <see cref="IUnitDef"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// Unit cm = new Unit(5.0, Si.Centimeter);
    /// cm = Si.Centimeter.From(5.0);
    /// cm = Length.From("5 cm");
    /// cm = (Length)"5.0 cm";
    ///
    /// inch = cm.To(Imperial.Inch);  // 1.9685 in
    ///
    /// Unit addition = cm + inch;  // 10 cm
    ///
    /// Unit subtraction = inch - cm;  // 0 in
    ///
    /// Unit multiplication = cm * inch;  // 25 cm²
    ///
    /// Unit division = multiplication / inch;  // 5 cm
    /// </code>
    /// </example>
    [Serializable]
    public readonly struct Unit : IUnit
    {
        /// <summary>
        /// When comparing two <see cref="Value"/>s, accept this difference and prevent floating point precision issues.
        /// </summary>
        internal const float Tolerance = float.Epsilon;

        /// <inheritdoc cref="IUnit.AbstractFormula"/>
        public Func<double, double> AbstractFormula { get; }
        
        /// <inheritdoc cref="IUnit.Power"/>
        public byte Power { get; }

        /// <inheritdoc cref="IUnit.Scale"/>
        public double Scale =>
            UnitDef.ScalePow(this);
        
        /// <inheritdoc cref="IUnit.UnitDef"/>
        public IUnitDef UnitDef { get; }
        
        /// <inheritdoc cref="IUnit.Value"/>
        public double Value { get; }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Amount of units part of this instance. With 5 cm², 5 is the value.</param>
        /// <param name="unitDef"><see cref="IUnitDef"/> defining the measurement unit. With 5 cm², <see cref="Si.Centimeter2"/> is the unit.</param>
        /// <param name="power">Unit power defining the amount of axis this <see cref="IUnit"/> represent.</param>
        /// <param name="abstractFormula">
        /// If <paramref name="unitDef"/>.<see cref="IUnitDef.IsAbstractUnit"/> is true, you can set a formula to
        /// convert the unit to a <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </param>
        /// <exception cref="PowerOutOfRangeException">
        /// If the given power is outside the range of
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMin"/> - <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
        /// </exception>
        public Unit(double value, IUnitDef unitDef, byte power = 1, Func<double, double> abstractFormula = null)
        {
            Power = unitDef.PowerMin == unitDef.PowerMax
                ? unitDef.PowerMin
                : power;
            Value = value;
            UnitDef = unitDef;

            if (Power < unitDef.PowerMin)
                throw new PowerOutOfRangeException(Power, unitDef, nameof(power));
            
            if (unitDef.PowerMax >= unitDef.PowerMin && Power > unitDef.PowerMax)
                throw new PowerOutOfRangeException(Power, unitDef, nameof(power));

            AbstractFormula = abstractFormula;
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Amount of units part of this instance. With 5 cm², 5 is the value.</param>
        /// <param name="unitDef"><see cref="IUnitDef"/> defining the measurement unit. With 5 cm², <see cref="Si.Centimeter2"/> is the unit.</param>
        /// <param name="abstractScale">
        /// If <paramref name="unitDef"/>.<see cref="IUnitDef.IsAbstractUnit"/> is true, you can set a multiplier to
        /// convert the unit to a <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </param>
        /// <param name="power">Unit power defining the amount of axis this <see cref="IUnit"/> represent.</param>
        /// <exception cref="PowerOutOfRangeException">
        /// If the given power is outside the range of
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMin"/> - <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
        /// </exception>
        public Unit(double value, IUnitDef unitDef, double abstractScale, byte power = 1) :
            this(value, unitDef, power, v => v * abstractScale) { }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Amount of units part of this instance. With 5 cm², 5 is the value.</param>
        /// <param name="unitDef"><see cref="IUnitDef"/> defining the measurement unit. With 5 cm², <see cref="Si.Centimeter2"/> is the unit.</param>
        /// <param name="abstractFormula">
        /// If <paramref name="unitDef"/>.<see cref="IUnitDef.IsAbstractUnit"/> is true, you can set a formula to
        /// convert the unit to a <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </param>
        /// <param name="power">Unit power defining the amount of axis this <see cref="IUnit"/> represent.</param>
        /// <exception cref="PowerOutOfRangeException">
        /// If the given power is outside the range of
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMin"/> - <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
        /// </exception>
        public Unit(double value, IUnitDef unitDef, IFormula abstractFormula, byte power = 1) :
            this(value, unitDef, power, abstractFormula.Invoke) { }

        /// <summary>
        /// Constructor when copying a <see cref="IUnit"/> to a new <see cref="Unit"/> instance.
        /// </summary>
        /// <param name="unit"><see cref="IUnit"/> to copy.</param>
        public Unit(IUnit unit) :
            this(unit.Value, unit.UnitDef, unit.Power, unit.AbstractFormula) { }

        /// <summary>
        /// Constructor when copying a <see cref="IUnit"/> to a new <see cref="Unit"/> instance and change
        /// the <see cref="AbstractFormula"/> to a new multiplier.
        /// </summary>
        /// <param name="unit"><see cref="IUnit"/> to copy.</param>
        /// <param name="abstractScale">Set the formula to this new multiplier value.</param>
        public Unit(IUnit unit, double abstractScale) :
            this(unit.Value, unit.UnitDef, unit.Power, value => value * abstractScale) { }
        
        /// <summary>
        /// Constructor when copying a <see cref="IUnit"/> to a new <see cref="Unit"/> instance and change
        /// the <see cref="AbstractFormula"/> to a new formula.
        /// </summary>
        /// <param name="unit"><see cref="IUnit"/> to copy.</param>
        /// <param name="abstractFormula">Set the formula to this new function.</param>
        public Unit(IUnit unit, Func<double, double> abstractFormula) :
            this(unit.Value, unit.UnitDef, unit.Power, abstractFormula) { }
        
        /// <summary>
        /// Constructor when copying a <see cref="IUnit"/> to a new <see cref="Unit"/> instance and change
        /// the <see cref="AbstractFormula"/> to a new formula.
        /// </summary>
        /// <param name="unit"><see cref="IUnit"/> to copy.</param>
        /// <param name="abstractFormula">Set the formula to this new instance.</param>
        public Unit(IUnit unit, IFormula abstractFormula) :
            this(unit.Value, unit.UnitDef, unit.Power, abstractFormula.Invoke) { }

        /// <summary>
        /// Constructor for a null <see cref="Unit"/>.
        /// </summary>
        /// <param name="unitDef">Define a null <see cref="Unit"/> for this unit definition.</param>
        internal Unit(IUnitDef unitDef = null)
        {
            Power = 0;
            Value = 0;
            UnitDef = unitDef;
            AbstractFormula = null;
        }

        /// <summary>
        /// Constructor used when deserializing binary data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated data.</param>
        /// <param name="context">The source (see <see cref="StreamingContext"/>) for the serialization to decode.</param>
        Unit(SerializationInfo info, StreamingContext context) :
            this(
                info.GetDouble("value"),
                DeserializeUnitDef(info),
                info.GetByte("power")) { }

        /// <summary>
        /// Constructor used when deserializing a dictionary representing a <see cref="Unit"/> instance.
        /// This constructor is used when deserializing json data (see <see cref="Newtonsoft.Json.JsonSerializer"/>).
        /// </summary>
        /// <param name="info">Dictionary to deserialize.</param>
        /// <seealso cref="Json.UnitConverter"/>
        public Unit(IReadOnlyDictionary<string, object> info) :
            this(
                (double)info["value"],
                DeserializeUnitDef(info),
                (byte)(long)info["power"]) { }


        /// <inheritdoc cref="Value"/>
        public static explicit operator double(Unit value) => value.Value;

        /// <summary>
        /// Explicit conversion from <see cref="Unit"/> to <see langword="float"/>.
        /// </summary>
        /// <param name="value">Instance to convert.</param>
        /// <returns>The double <see cref="Value"/> this Unit holds converted to a float.</returns>
        public static explicit operator float(Unit value) => (float)value.Value;

        /// <summary>
        /// Explicit conversion from <see cref="Unit"/> to <see langword="int"/>.
        /// </summary>
        /// <param name="value">Instance to convert.</param>
        /// <returns>The double <see cref="Value"/> this Unit holds converted to a int.</returns>
        public static explicit operator int(Unit value) => (int)value.Value;

        /// <inheritdoc cref="ToString"/>
        public static explicit operator string(Unit value) => value.ToString();

        /// <inheritdoc cref="From(TimeSpan)"/>
        public static explicit operator Unit(TimeSpan value) => From(value);

        /// <inheritdoc cref="ToTimeSpan"/>
        public static explicit operator TimeSpan(Unit value) => value.ToTimeSpan();

        /// <inheritdoc cref="From(string)"/>
        public static explicit operator Unit(string value) => From(value);


        /// <summary>
        /// Create a new <see cref="Unit"/> instance by adding the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">First <see cref="Unit"/> to add.</param>
        /// <param name="second">Second <see cref="Unit"/> to add.</param>
        /// <returns>The addition result as a new <see cref="Unit"/> instance.</returns>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        public static Unit operator +(Unit first, IUnit second) =>
            new(
                first.UnitDef.Add(first, second),
                first.UnitDef,
                first.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by adding the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.
        /// </summary>
        /// <param name="first">First <see cref="Unit"/> to add.</param>
        /// <param name="second">Second double-precision floating-point to add.</param>
        /// <returns>The addition result as a new <see cref="Unit"/> instance.</returns>
        public static Unit operator +(Unit first, double second) => 
            new(
                first.Value + second,
                first.UnitDef,
                first.Power);
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance by adding the
        /// <paramref name="first"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">First double-precision floating-point to add.</param>
        /// <param name="second">Second <see cref="Unit"/> to add.</param>
        /// <returns>The addition result as a new <see cref="Unit"/> instance.</returns>
        public static Unit operator +(double first, Unit second) => 
            new(
                first + second.Value,
                second.UnitDef,
                second.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by subtracting the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first"><see cref="Unit"/> to subtract from.</param>
        /// <param name="second">The <see cref="Unit"/> to subtract with the <paramref name="first"/>.</param>
        /// <returns>The subtraction result as a new <see cref="Unit"/> instance.</returns>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        public static Unit operator -(Unit first, IUnit second) =>
            new(
                first.UnitDef.Sub(first, second),
                first.UnitDef,
                first.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by subtracting the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.
        /// </summary>
        /// <param name="first"><see cref="Unit"/> to subtract from.</param>
        /// <param name="second">The double-precision floating-point to subtract with the <paramref name="first"/>.</param>
        /// <returns>The subtraction result as a new <see cref="Unit"/> instance.</returns>
        public static Unit operator -(Unit first, double second) => 
            new(
                first.Value - second,
                first.UnitDef,
                first.Power);
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance by subtracting the
        /// <paramref name="first"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">The double-precision floating-point to subtract from.</param>
        /// <param name="second"><see cref="Unit"/> to subtract with the <paramref name="first"/>.</param>
        /// <returns>The subtraction result as a new <see cref="Unit"/> instance.</returns>
        public static Unit operator -(double first, Unit second) => 
            new(
                first - second.Value,
                second.UnitDef,
                second.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by multiplying the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">First <see cref="Unit"/> to multiply.</param>
        /// <param name="second">Second <see cref="Unit"/> to multiply.</param>
        /// <returns>The multiplication result as a new <see cref="Unit"/> instance with an increased <see cref="Power"/>.</returns>
        /// <example>
        /// <code>
        /// Unit unitA = Si.Meter.From(6);
        /// Unit unitB = Si.Meter.From(2);
        /// Unit result = unitA * unitB;  // 12 m²
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// Unit unitA = Si.Meter.From(6);
        /// Unit unitB = Si.Meter2.From(2);
        /// Unit result = unitA * unitB;  // 12 m³
        /// </code>
        /// </example>
        /// <exception cref="IncompatibleTypesException">If both <see cref="UnitDef{T}"/> are not compatible.</exception>
        public static Unit operator *(Unit first, IUnit second)
        {
            double value = first.UnitDef.Mult(first, second);
            byte power = (byte)(first.Power + second.Power);
            
            IUnitDef newUnitDef = CorrespondingPowerUnit(power, 1, x => x > first.UnitDef.PowerMax, first.UnitDef);
            ValidatePowerInRange(power, newUnitDef);

            return new Unit(value, newUnitDef, power);
        }

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by multiplying the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.
        /// </summary>
        /// <param name="first">First <see cref="Unit"/> to multiply.</param>
        /// <param name="second">Second double-precision floating-point to multiply.</param>
        /// <returns>The multiplication result as a new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit unitA = Si.Meter.From(6);
        /// Unit result = unitA * 2;  // 12 m
        /// </code>
        /// </example>
        public static Unit operator *(Unit first, double second) => 
            new(
                first.Value * second,
                first.UnitDef,
                first.Power);
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance by multiplying the
        /// <paramref name="first"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">First double-precision floating-point to multiply.</param>
        /// <param name="second">Second <see cref="Unit"/> to multiply.</param>
        /// <returns>The multiplication result as a new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit unitB = Si.Meter.From(6);
        /// Unit result = 2 * unitB;  // 12 m
        /// </code>
        /// </example>
        public static Unit operator *(double first, Unit second) => 
            new(
                first * second.Value,
                second.UnitDef,
                second.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by dividing the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first"><see cref="Unit"/> to divide from.</param>
        /// <param name="second">The <see cref="Unit"/> to divide with the <paramref name="first"/>.</param>
        /// <returns>The division result as a new <see cref="Unit"/> instance with a reduced <see cref="Power"/>.</returns>
        /// <example>
        /// <code>
        /// Unit unitA = Si.Meter3.From(6);
        /// Unit unitB = Si.Meter.From(2);
        /// Unit result = unitA / unitB;  // 3 m²
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// Unit unitA = Si.Meter3.From(6);
        /// Unit unitB = Si.Meter2.From(2);
        /// Unit result = unitA / unitB;  // 3 m
        /// </code>
        /// </example>
        /// <exception cref="IncompatibleTypesException">If both <see cref="UnitDef{T}"/> are not compatible.</exception>
        public static Unit operator /(Unit first, IUnit second)
        {
            double value = first.UnitDef.Div(first, second);
            byte power = (byte)(first.Power - second.Power);
            
            IUnitDef newUnitDef = CorrespondingPowerUnit(power, -1, x => x < first.UnitDef.PowerMin, first.UnitDef);
            ValidatePowerInRange(power, newUnitDef);

            return new Unit(value, newUnitDef, power);
        }

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by dividing the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.
        /// </summary>
        /// <param name="first"><see cref="Unit"/> to divide from.</param>
        /// <param name="second">The double-precision floating-point to divide with the <paramref name="first"/>.</param>
        /// <returns>The division result as a new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit unitA = Si.Meter3.From(6);
        /// Unit result = unitA / 2;  // 3 m³
        /// </code>
        /// </example>
        public static Unit operator /(Unit first, double second) => 
            new(
                first.Value / second,
                first.UnitDef,
                first.Power);
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance by dividing the
        /// <paramref name="first"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">The double-precision floating-point to divide from.</param>
        /// <param name="second"><see cref="Unit"/> to divide with the <paramref name="first"/>.</param>
        /// <returns>The division result as a new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit unitB = Si.Meter3.From(2);
        /// Unit result = 6 / unitB;  // 3 m³
        /// </code>
        /// </example>
        public static Unit operator /(double first, Unit second) => 
            new(
                first / second.Value,
                second.UnitDef,
                second.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by calculating the remaining between the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first"><see cref="Unit"/> to calculate modulo from.</param>
        /// <param name="second">The <see cref="Unit"/> to apply modulo with the <paramref name="first"/>.</param>
        /// <returns>The modulo result as a new <see cref="Unit"/> instance.</returns>
        /// <exception cref="DifferentPowersException">
        /// If both <see cref="IUnit"/> have a different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
        /// </exception>
        public static Unit operator %(Unit first, IUnit second) =>
            new(
                first.UnitDef.Modulo(first, second),
                first.UnitDef,
                first.Power);

        /// <summary>
        /// Create a new <see cref="Unit"/> instance by calculating the remaining between the
        /// <paramref name="first"/>.<see cref="Value"/> with the <paramref name="second"/>.
        /// </summary>
        /// <param name="first"><see cref="Unit"/> to calculate modulo from.</param>
        /// <param name="second">The double-precision floating-point to apply modulo with the <paramref name="first"/>.</param>
        /// <returns>The modulo result as a new <see cref="Unit"/> instance.</returns>
        public static Unit operator %(Unit first, double second) => 
            new(
                first.Value % second,
                first.UnitDef,
                first.Power);
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance by calculating the remaining between the
        /// <paramref name="first"/> with the <paramref name="second"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">The double-precision floating-point to calculate modulo from.</param>
        /// <param name="second"><see cref="Unit"/> to apply modulo with the <paramref name="first"/>.</param>
        /// <returns>The modulo result as a new <see cref="Unit"/> instance.</returns>
        public static Unit operator %(double first, Unit second) => 
            new(
                first % second.Value,
                second.UnitDef,
                second.Power);

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is greater of an other by converting
        /// the <paramref name="second"/> to the same <see cref="UnitDef"/> as the <paramref name="first"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is greater.</param>
        /// <param name="second">Compare if this <see cref="IUnit"/>.<see cref="IUnit.Value"/> is smaller.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a greater value than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> has a greater or equal value or if the
        /// <see cref="UnitDef"/> is of a different type.
        /// </returns>
        public static bool operator >(Unit first, IUnit second) =>
            first.CompareTo(second) > 0;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is greater of an other <see langword="double"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is greater.</param>
        /// <param name="second">Compare if this <see langword="double"/> is smaller.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a greater value than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is greater or equals.
        /// </returns>
        public static bool operator >(Unit first, double second) => 
            first.Value > second;

        /// <summary>
        /// Evaluate if a <see langword="double"/> is greater of a <see cref="Unit"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">Compare if this <see langword="double"/> is greater.</param>
        /// <param name="second">Compare if this <see cref="Unit"/>.<see cref="Value"/> is smaller.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> is greater than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is greater or equals.
        /// </returns>
        public static bool operator >(double first, Unit second) => 
            first > second.Value;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is greater or equals an other by converting
        /// the <paramref name="second"/> to the same <see cref="UnitDef"/> as the <paramref name="first"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is greater or equal.</param>
        /// <param name="second">Compare if this <see cref="IUnit"/>.<see cref="IUnit.Value"/> is smaller.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a greater or equal value than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> has a greater or if the
        /// <see cref="UnitDef"/> is of a different type.
        /// </returns>
        public static bool operator >=(Unit first, IUnit second) => 
            first.CompareTo(second) >= 0;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is greater or equals an other <see langword="double"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is greater or equal.</param>
        /// <param name="second">Compare if this <see langword="double"/> is smaller.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a greater or equals <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is greater.
        /// </returns>
        public static bool operator >=(Unit first, double second) => 
            first.Value >= second;

        /// <summary>
        /// Evaluate if a <see langword="double"/> is greater or equals a <see cref="Unit"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">Compare if this <see langword="double"/> is greater or equal.</param>
        /// <param name="second">Compare if this <see cref="Unit"/>.<see cref="Value"/> is smaller.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> is greater or equals <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is greater or equals.
        /// </returns>
        public static bool operator >=(double first, Unit second) => 
            first >= second.Value;
        

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is smaller of an other by converting
        /// the <paramref name="second"/> to the same <see cref="UnitDef"/> as the <paramref name="first"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is smaller.</param>
        /// <param name="second">Compare if this <see cref="IUnit"/>.<see cref="IUnit.Value"/> is greater.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a smaller value than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> has a smaller or equal value or if the
        /// <see cref="UnitDef"/> is of a different type.
        /// </returns>
        public static bool operator <(Unit first, IUnit second) => 
            first.CompareTo(second) < 0;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is smaller of an other <see langword="double"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is smaller.</param>
        /// <param name="second">Compare if this <see langword="double"/> is greater.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a smaller value than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is smaller or equals.
        /// </returns>
        public static bool operator <(Unit first, double second) => 
            first.Value < second;

        /// <summary>
        /// Evaluate if a <see langword="double"/> is smaller of a <see cref="Unit"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">Compare if this <see langword="double"/> is smaller.</param>
        /// <param name="second">Compare if this <see cref="Unit"/>.<see cref="Value"/> is greater.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> is smaller than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is smaller or equals.
        /// </returns>
        public static bool operator <(double first, Unit second) => 
            first < second.Value;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is smaller or equals an other by converting
        /// the <paramref name="second"/> to the same <see cref="UnitDef"/> as the <paramref name="first"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is smaller or equal.</param>
        /// <param name="second">Compare if this <see cref="IUnit"/>.<see cref="IUnit.Value"/> is greater.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a smaller or equal value than <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> has a smaller or if the
        /// <see cref="UnitDef"/> is of a different type.
        /// </returns>
        public static bool operator <=(Unit first, IUnit second) => 
            first.CompareTo(second) <= 0;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is smaller or equals an other <see langword="double"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> is smaller or equal.</param>
        /// <param name="second">Compare if this <see langword="double"/> is greater.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> has a smaller or equals <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is smaller.
        /// </returns>
        public static bool operator <=(Unit first, double second) => 
            first.Value <= second;

        /// <summary>
        /// Evaluate if a <see langword="double"/> is smaller or equals a <see cref="Unit"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">Compare if this <see langword="double"/> is smaller or equal.</param>
        /// <param name="second">Compare if this <see cref="Unit"/>.<see cref="Value"/> is greater.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> is smaller or equals <paramref name="second"/>;
        /// <see langword="false"/> if <paramref name="second"/> is smaller or equals.
        /// </returns>
        public static bool operator <=(double first, Unit second) => 
            first <= second.Value;

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> equals an other by converting
        /// the <paramref name="second"/> to the same <see cref="UnitDef"/> as the <paramref name="first"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> equals.</param>
        /// <param name="second">Compare if this <see cref="IUnit"/>.<see cref="IUnit.Value"/> equals.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/>.<see cref="Value"/> equals <paramref name="second"/>.<see cref="IUnit.Value"/>;
        /// <see langword="false"/> if both values are not the same or if the
        /// <see cref="UnitDef"/> is of a different type.
        /// </returns>
        public static bool operator ==(Unit first, IUnit second) => 
            first.Equals(second);

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> equals an other <see langword="double"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> equals.</param>
        /// <param name="second">Compare if this <see langword="double"/> equals.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/>.<see cref="Value"/> equals <paramref name="second"/>;
        /// <see langword="false"/> if both values are not the same.
        /// </returns>
        public static bool operator ==(Unit first, double second) => 
            Equals(first.Value, second);

        /// <summary>
        /// Evaluate if a <see langword="double"/> equals the given <see cref="Unit"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">Compare if this <see langword="double"/> equals.</param>
        /// <param name="second">Compare if this <see cref="Unit"/>.<see cref="Value"/> equals.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> equals <paramref name="second"/>.<see cref="Unit.Value"/>;
        /// <see langword="false"/> if both values are not the same.
        /// </returns>
        public static bool operator ==(double first, Unit second) => 
            Equals(first, second.Value);

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is not the same of an other by converting
        /// the <paramref name="second"/> to the same <see cref="UnitDef"/> as the <paramref name="first"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> does not equals.</param>
        /// <param name="second">Compare if this <see cref="IUnit"/>.<see cref="IUnit.Value"/> does not equals.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/>.<see cref="Value"/> does not equals
        /// <paramref name="second"/>.<see cref="IUnit.Value"/> or if the <see cref="UnitDef"/> is of a different type;
        /// <see langword="false"/> if both values are the same.
        /// </returns>
        public static bool operator !=(Unit first, IUnit second) => 
            !first.Equals(second);

        /// <summary>
        /// Evaluate if a <see cref="Unit"/>.<see cref="Value"/> is not the same of an other <see langword="double"/>.
        /// </summary>
        /// <param name="first">Compare if this <see cref="Unit"/>.<see cref="Value"/> does not equals.</param>
        /// <param name="second">Compare if this <see langword="double"/> does not equals.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/>.<see cref="Value"/> does not equals <paramref name="second"/>;
        /// <see langword="false"/> if both values are the same.
        /// </returns>
        public static bool operator !=(Unit first, double second) => 
            !Equals(first.Value, second);

        /// <summary>
        /// Evaluate if a <see langword="double"/> is not the same of a given <see cref="Unit"/>.<see cref="Value"/>.
        /// </summary>
        /// <param name="first">Compare if this <see langword="double"/> does not equals.</param>
        /// <param name="second">Compare if this <see cref="Unit"/>.<see cref="Value"/> does not equals.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="first"/> does not equals <paramref name="second"/>.<see cref="Unit.Value"/>;
        /// <see langword="false"/> if both values are the same.
        /// </returns>
        public static bool operator !=(double first, Unit second) => 
            !Equals(first, second.Value);


        /// <summary>
        /// Find the corresponding <see cref="IUnitDef"/> when converting to a <see cref="IUnitDef"/>
        /// of a different <see cref="Power"/> value.
        /// </summary>
        /// <param name="unitDef"><see cref="IUnitDef"/> to convert.</param>
        /// <returns>
        /// A <see cref="IUnitDef"/> instance if one is registered for this instance <see cref="Power"/> value;
        /// <paramref name="unitDef"/> otherwise.
        /// </returns>
        /// <exception cref="NullReferenceException">If the given <see cref="IUnitDef"/> is null.</exception>
        private IUnitDef FindCorrespondingUnitDef(IUnitDef unitDef)
        {
            if (IsNull)
                throw new NullUnitDefException(
                    "Null UnitValue cannot be converted.");

            if (Power >= unitDef.PowerMin
                && (unitDef.PowerMax < unitDef.PowerMin
                    || Power <= unitDef.PowerMax))
                return unitDef;

            return Mapping.PowerUnits.TryGetValue(unitDef, out Dictionary<byte, IUnitDef> mapping)
                   && mapping.TryGetValue(Power, out IUnitDef newUnit)
                ? newUnit
                : unitDef;
        }

        /// <summary>
        /// Decode a string and create a new <see cref="Unit"/> instance based on it.
        /// </summary>
        /// <param name="text">String to extract unit data.</param>
        /// <returns>A new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit unit = Unit.From("5 cm");
        /// unit = Unit.From("5 cm²");
        /// unit = Unit.From("5cm");
        /// unit = Unit.From("5.5 centimeters");
        /// unit = Unit.From("5'3\"");
        /// unit = (Unit)"5.3e-2 cm";
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        public static Unit From(string text)
        {
            Parser.FromStringToComponentUnit(text, out double value, out IUnitDef unit, out byte power);
            return unit.From(value, power);
        }

        /// <summary>
        /// Convert a <see cref="TimeSpan"/> to a new instance of <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="time"><see cref="TimeSpan"/> to convert.</param>
        /// <returns>
        /// Create a new <see cref="Unit"/> with the converted value set to <see cref="Unit"/>.<see cref="Unit.Value"/>.
        /// </returns>
        public static Unit From(TimeSpan time) =>
            Si.Millisecond.From(time.TotalMilliseconds);

        /// <summary>
        /// Decode a string and create a new <see cref="Unit"/> instance based on it and restrict the search with
        /// only <see cref="IUnitDef"/> instances of the given type.
        /// This allow to decode <see cref="IUnitDef"/> that has the same <see cref="IUnitDef.Symbol"/>
        /// like <see cref="Si.ArcMinute"/> and <see cref="Imperial.Feet"/>.
        /// </summary>
        /// <typeparam name="TUnitDef">Restrict the search with only with this <see cref="IUnitDef"/> type.</typeparam>
        /// <param name="text">String to extract unit data.</param>
        /// <returns>A new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit feet = Unit.From{Length}("5'6\"");
        /// Unit arcMin = Unit.From{Angle}("5'6\"");
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        /// <seealso cref="UnitDef{T}.From(string)"/>
        public static Unit From<TUnitDef>(string text) =>
            From(text, typeof(TUnitDef));

        /// <summary>
        /// Decode a string and create a new <see cref="Unit"/> instance based on it and restrict the search with
        /// only <see cref="IUnitDef"/> instances of the given types.
        /// This allow to decode <see cref="IUnitDef"/> that has the same <see cref="IUnitDef.Symbol"/>
        /// like <see cref="Si.ArcMinute"/> and <see cref="Imperial.Feet"/>.
        /// </summary>
        /// <param name="text">String to extract unit data.</param>
        /// <param name="unitTypes">Restrict the search with only <see cref="IUnitDef"/> instances of these types.</param>
        /// <returns>A new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit feet = Unit.From("5'6\"", typeof(Length));
        /// Unit arcMin = Unit.From("5'6\"", typeof(Angle));
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        /// <seealso cref="UnitDef{T}.From(string)"/>
        public static Unit From(string text, params Type[] unitTypes) =>
            From(text, unitTypes.AsEnumerable());
        
        /// <summary>
        /// Decode a string and create a new <see cref="Unit"/> instance based on it and restrict the search with
        /// only <see cref="IUnitDef"/> instances of the given types.
        /// This allow to decode <see cref="IUnitDef"/> that has the same <see cref="IUnitDef.Symbol"/>
        /// like <see cref="Si.ArcMinute"/> and <see cref="Imperial.Feet"/>.
        /// </summary>
        /// <param name="text">String to extract unit data.</param>
        /// <param name="unitTypes">Restrict the search with only <see cref="IUnitDef"/> instances of these types.</param>
        /// <returns>A new <see cref="Unit"/> instance.</returns>
        /// <example>
        /// <code>
        /// Unit feet = Unit.From("5'6\"", new [] { typeof(Length), typeof(Area), typeof(Volume) });
        /// Unit arcMin = Unit.From("5'6\"", new [] { typeof(Angle) });
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        /// <seealso cref="UnitDef{T}.From(string)"/>
        public static Unit From(string text, IEnumerable<Type> unitTypes)
        {
            Parser.FromStringToComponentUnit(text, unitTypes, out double value, out IUnitDef unit, out byte power);
            return unit.From(value, power);
        }

        /// <summary>
        /// Decode multiple string and create a new <see cref="Unit"/> instance for each of them.
        /// </summary>
        /// <param name="text">String instances to extract unit data.</param>
        /// <returns>A new <see cref="Unit"/> instance for each given string.</returns>
        /// <example>
        /// <code>
        /// var units = Unit.From({"5 cm", "6cm", "7in"});
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        public static IEnumerable<Unit> From(IEnumerable<string> text)
        {
            foreach (string each in text)
            {
                Parser.FromStringToComponentUnit(each, out double value, out IUnitDef unit, out byte power);
                yield return unit.From(value, power);
            }
        }

        /// <summary>
        /// Decode multiple string and create a new <see cref="Unit"/> instance based on it and restrict the search with
        /// only <see cref="IUnitDef"/> instances of the given type.
        /// This allow to decode <see cref="IUnitDef"/> that has the same <see cref="IUnitDef.Symbol"/>
        /// like <see cref="Si.ArcMinute"/> and <see cref="Imperial.Feet"/>.
        /// </summary>
        /// <typeparam name="TUnitDef">Restrict the search with only with this <see cref="IUnitDef"/> type.</typeparam>
        /// <param name="text">String instances to extract unit data.</param>
        /// <returns>A new <see cref="Unit"/> instance for each given string.</returns>
        /// <example>
        /// <code>
        /// var units = Unit.From({"5 cm", "6cm", "7in", "8'6\""}, typeof(Length), typeof(Area), typeof(Volume));
        /// var arcMin = Unit.From({"5'6\"", "5 arc min"}, typeof(Angle));
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        public static IEnumerable<Unit> From<TUnitDef>(IEnumerable<string> text) =>
            From(text, typeof(TUnitDef));

        /// <summary>
        /// Decode multiple string and create a new <see cref="Unit"/> instance for each of them and restrict the search with
        /// only <see cref="IUnitDef"/> instances of the given types.
        /// This allow to decode <see cref="IUnitDef"/> that has the same <see cref="IUnitDef.Symbol"/>
        /// like <see cref="Si.ArcMinute"/> and <see cref="Imperial.Feet"/>.
        /// </summary>
        /// <param name="text">String instances to extract unit data.</param>
        /// <param name="unitTypes">Restrict the search with only <see cref="IUnitDef"/> instances of these types.</param>
        /// <returns>A new <see cref="Unit"/> instance for each given string.</returns>
        /// <example>
        /// <code>
        /// var units = Unit.From({"5 cm", "6cm", "7in", "8'6\""}, typeof(Length), typeof(Area), typeof(Volume));
        /// var arcMin = Unit.From({"5'6\"", "5 arc min"}, typeof(Angle));
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        public static IEnumerable<Unit> From(IEnumerable<string> text, params Type[] unitTypes)
        {
            foreach (string each in text)
            {
                Parser.FromStringToComponentUnit(each, unitTypes, out double value, out IUnitDef unit, out byte power);
                yield return unit.From(value, power);
            }
        }

        /// <summary>
        /// Decode multiple string and create a new <see cref="Unit"/> instance for each of them and restrict the search with
        /// only <see cref="IUnitDef"/> instances of the given types.
        /// This allow to decode <see cref="IUnitDef"/> that has the same <see cref="IUnitDef.Symbol"/>
        /// like <see cref="Si.ArcMinute"/> and <see cref="Imperial.Feet"/>.
        /// </summary>
        /// <param name="text">String instances to extract unit data.</param>
        /// <param name="unitTypes">Restrict the search with only <see cref="IUnitDef"/> instances of these types.</param>
        /// <returns>A new <see cref="Unit"/> instance for each given string.</returns>
        /// <example>
        /// <code>
        /// var units = Unit.From({"5 cm", "6cm", "7in", "8'6\""}, new [] { typeof(Length), typeof(Area), typeof(Volume) });
        /// var arcMin = Unit.From({"5'6\"", "5 arc min"}, new [] { typeof(Angle) });
        /// </code>
        /// </example>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">
        /// If the decoded <see cref="Unit"/>.<see cref="Power"/> is out of the range of
        /// <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMin"/> - <see cref="UnitDef"/>.<see cref="UnitDef{T}.PowerMax"/>.
        /// </exception>
        public static IEnumerable<Unit> From(IEnumerable<string> text, IEnumerable<Type> unitTypes) =>
            From(text, unitTypes.ToArray());

        /// <summary>
        /// Decode a string to a <see cref="Unit"/> instance with its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="From(string)"/> instead.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,Type[])"/> if you already know the unit type.
        /// </remarks>
        /// <returns>A new <see cref="Unit"/> instance based on <paramref name="text"/>.</returns>
        public static Unit FromStringToSingleUnit(string text)
        {
            Parser.FromStringToSingleUnit(text, out double value, out IUnitDef unit, out byte power);
            return unit.From(value, power);
        }

        /// <summary>
        /// Decode a string to a <see cref="Unit"/> instance with its corresponding <see cref="IUnitDef"/>, value and power.
        /// This method restrict the search to only <see cref="IUnitDef"/> instances part of the <typeparamref name="TUnitDef"/>.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="From(string,Type[])"/> instead.
        /// </remarks>
        /// <typeparam name="TUnitDef">Restrict the search with only with this <see cref="IUnitDef"/> type.</typeparam>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,IEnumerable{Type})"/> if you already know the unit type.
        /// </remarks>
        /// <returns>A new <see cref="Unit"/> instance based on <paramref name="text"/>.</returns>
        public static Unit FromStringToSingleUnit<TUnitDef>(string text) =>
            FromStringToSingleUnit(text, typeof(TUnitDef));

        /// <summary>
        /// Decode a string to a <see cref="Unit"/> instance with its corresponding <see cref="IUnitDef"/>, value and power.
        /// This method restrict the search to only <see cref="IUnitDef"/> instances part of the <paramref name="unitTypes"/>.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="From(string,Type[])"/> instead.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,IEnumerable{Type})"/> if you already know the unit type.
        /// </remarks>
        /// <param name="unitTypes">Restrict the search with only <see cref="UnitDef{T}"/> instances of those types.</param>
        /// <returns>A new <see cref="Unit"/> instance based on <paramref name="text"/>.</returns>
        public static Unit FromStringToSingleUnit(string text, params Type[] unitTypes) =>
            FromStringToSingleUnit(text, unitTypes.AsEnumerable());
        

        /// <summary>
        /// Decode a string to a <see cref="Unit"/> instance with its corresponding <see cref="IUnitDef"/>, value and power.
        /// This method restrict the search to only <see cref="IUnitDef"/> instances part of the <paramref name="unitTypes"/>.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="From(string,IEnumerable{Type})"/> instead.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,IEnumerable{Type})"/> if you already know the unit type.
        /// </remarks>
        /// <param name="unitTypes">Restrict the search with only <see cref="UnitDef{T}"/> instances of those types.</param>
        /// <returns>A new <see cref="Unit"/> instance based on <paramref name="text"/>.</returns>
        public static Unit FromStringToSingleUnit(string text, IEnumerable<Type> unitTypes)
        {
            Parser.FromStringToSingleUnit(text, unitTypes, out double value, out IUnitDef unit, out byte power);
            return unit.From(value, power);
        }

        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        /// <exception cref="NullReferenceException">If the given <see cref="IUnitDef"/> is null.</exception>
        /// <exception cref="DifferentTypesException">If both <see cref="UnitDef{T}"/> are not of the same type.</exception>
        public Unit To(IUnitDef unitDef)
        {
            unitDef = FindCorrespondingUnitDef(unitDef);
            
            if (!UnitDef.IsAbstractUnit)
                return new Unit(
                    UnitDef.To(Value, unitDef, Power),
                    unitDef,
                    Power);
            
            if (AbstractFormula is null)
                throw new ConvertAbstractException(UnitDef, unitDef);
            
            return new Unit(
                UnitDef.To(Value, unitDef, AbstractFormula, Power),
                unitDef,
                Power);
        }

        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="Unit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        /// <exception cref="NullReferenceException">If the given <see cref="IUnitDef"/> is null.</exception>
        /// <exception cref="DifferentTypesException">If both <see cref="IUnitDef"/> are not of the same type.</exception>
        /// <exception cref="ConvertAbstractException">If both <see cref="IUnitDef"/> are abstract.</exception>
        public Unit To(IUnitDef unitDef, double scale) =>
            unitDef.IsAbstractUnit || IsAbstractUnit && AbstractFormula is null
                ? To(unitDef, new Formula(value => value))
                : To(unitDef);

        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        /// <exception cref="NullReferenceException">If the given <see cref="IUnitDef"/> is null.</exception>
        /// <exception cref="DifferentTypesException">If both <see cref="IUnitDef"/> are not of the same type.</exception>
        /// <exception cref="ConvertAbstractException">If both <see cref="IUnitDef"/> are abstract.</exception>
        public Unit To(IUnitDef unitDef, Func<double, double> formula) =>
            unitDef.IsAbstractUnit || IsAbstractUnit && AbstractFormula is null
                ? To(unitDef, new Formula(formula))
                : To(unitDef);

        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        /// <exception cref="NullReferenceException">If the given <see cref="IUnitDef"/> is null.</exception>
        /// <exception cref="DifferentTypesException">If both <see cref="IUnitDef"/> are not of the same type.</exception>
        /// <exception cref="ConvertAbstractException">If both <see cref="IUnitDef"/> are abstract.</exception>
        public Unit To(IUnitDef unitDef, IFormula formula)
        {
            bool abstractSrc = IsAbstractUnit;  // is abstract and have no formula conversion
            bool abstractDst = unitDef.IsAbstractUnit;
            
            if (!abstractSrc && !abstractDst)
                return To(unitDef);

            unitDef = FindCorrespondingUnitDef(unitDef);
            
            if (abstractSrc && abstractDst)
                if (AbstractFormula is null)
                    throw new ConvertAbstractException(UnitDef, unitDef);
                else
                    return new Unit(
                        UnitDef.To(Value, unitDef, AbstractFormula, Power),
                        unitDef,
                        formula,
                        Power);
            
            // destination is not abstract
            if (abstractSrc)  
                return new Unit(
                    UnitDef.To(Value, unitDef, formula, Power),
                    unitDef,
                    Power);

            double value = AbstractFormula?.Invoke(Value) ?? Value;
            
            // source is not abstract or has a formula
            return new Unit(
                value / formula.Invoke(1.0),
                unitDef,
                formula,
                Power);
        }

        /// <inheritdoc cref="IUnit.ToBaseUnit"/>
        public Unit ToBaseUnit()
        {
            if (IsNull)
                throw new NullUnitDefException(
                    "Null UnitValue cannot be converted.");

            IUnitDef unitDef = UnitDef.BaseUnitDef;
            return To(unitDef);
        }
        
        /// <summary>
        /// Convert the <paramref name="unit"/>.<see cref="Value"/> to the <see cref="IUnitDef"/> defined by <see cref="IUnitDef.BaseUnitDef"/>
        /// for the <see cref="IUnitDef"/> type.
        /// </summary>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        private static double ToBaseUnit(IUnit unit)
        {
            if (unit.IsNull)
                return unit.Value;

            IUnitDef unitDef = unit.UnitDef.BaseUnitDef;
            return unit.UnitDef.To(unit.Value, unitDef, unit.Power);
        }

        /// <inheritdoc cref="IUnit.ToFormula(IUnitDef)"/>
        public Func<double, double> ToFormula(IUnitDef unitDef)
        {
            unitDef = FindCorrespondingUnitDef(unitDef);
            
            if (!UnitDef.IsAbstractUnit)
                return UnitDef.ToFormula(unitDef, Power);
            
            if (AbstractFormula is null)
                throw new ConvertAbstractException(UnitDef, unitDef);

            return UnitDef.ToFormula(unitDef, AbstractFormula, Power);
        }

        /// <inheritdoc cref="IUnit.ToFormula(IUnitDef, double)"/>
        public Func<double, double> ToFormula(IUnitDef unitDef, double scale) =>
            unitDef.IsAbstractUnit || IsAbstractUnit && AbstractFormula is null
                ? ToFormula(unitDef, new Formula(value => value))
                : ToFormula(unitDef);

        /// <inheritdoc cref="IUnit.ToFormula(IUnitDef, Func{double, double})"/>
        public Func<double, double> ToFormula(IUnitDef unitDef, Func<double, double> formula) =>
            unitDef.IsAbstractUnit || IsAbstractUnit && AbstractFormula is null
                ? ToFormula(unitDef, new Formula(formula))
                : ToFormula(unitDef);

        /// <inheritdoc cref="IUnit.ToFormula(IUnitDef, IFormula)"/>
        public Func<double, double> ToFormula(IUnitDef unitDef, IFormula formula)
        {
            bool abstractSrc = IsAbstractUnit; // is abstract and have no formula conversion
            bool abstractDst = unitDef.IsAbstractUnit;

            if (!abstractSrc && !abstractDst)
                return ToFormula(unitDef);

            unitDef = FindCorrespondingUnitDef(unitDef);

            // source is not abstract or has a formula
            IUnitDef unitFrom = UnitDef;
            byte power = Power;
            double scale;
            Func<double, double> thisFormula;

            if (abstractSrc && abstractDst)
                if (AbstractFormula is null)
                    throw new ConvertAbstractException(UnitDef, unitDef);
                else
                {
                    thisFormula = unitFrom.ToFormula(unitDef, AbstractFormula, power);
                    scale = formula.Invoke(1.0);

                    return value => thisFormula(value) / scale;
                }

            // destination is not abstract
            if (abstractSrc)
                return UnitDef.ToFormula(unitDef, formula, Power);

            thisFormula = unitFrom.ToFormula(unitDef, power);
            scale = formula.Invoke(1.0);
            
            return value => thisFormula(value) / scale;
        }

        /// <inheritdoc cref="IUnit.ToFullString()"/>
        public string ToFullString() => ToFullString(UnitFormatter.DefaultFullStringFormatter);

        /// <inheritdoc cref="IUnit.ToFullString(IUnitFormatter)"/>
        public string ToFullString(IUnitFormatter provider)
        {
            provider ??= UnitFormatter.DefaultFullStringFormatter;
            return provider.Format("{0}", this, provider);
        }

        /// <summary>
        /// Format the <see cref="Unit"/> with the double-precision floating-point value as the prefix and the
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Symbol"/> as the suffix.
        /// </summary>
        /// <example>5 u³</example>
        /// <returns>The formatted string.</returns>
        public override string ToString()
        {
            return ToString(UnitFormatter.DefaultFormatter);
        }

        /// <inheritdoc cref="IUnit.ToString(IUnitFormatter)"/>
        public string ToString(IUnitFormatter provider)
        {
            provider ??= UnitFormatter.DefaultFormatter;
            return provider.Format("{0}", this, provider);
        }

        /// <summary>
        /// If the <see cref="Unit"/>.<see cref="UnitDef"/> is a <see cref="Time"/> type, convert the value to a TimeSpan instance.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <exception cref="WrongUnitDefTypeException">If the <see cref="Unit"/>.<see cref="UnitDef"/> is not a <see cref="Time"/> type.</exception>
        public TimeSpan ToTimeSpan() =>
            UnitDef is Time
                ? new TimeSpan(0, 0, 0, 0, (int)To(Si.Millisecond).Value)
                : throw new WrongUnitDefTypeException(typeof(Time), UnitDef);


        /// <summary>
        /// Compares the current instance with another <see cref="IUnit"/> and returns an integer that indicates whether
        /// the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">A <see cref="IUnit"/> to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <term>Meaning</term>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <term>This instance precedes <paramref name="other"/> in the sort order.</term>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <term>This instance occurs in the same position in the sort order as <paramref name="other"/>.</term>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <term>This instance follows <paramref name="other"/> in the sort order.</term>
        ///     </item>
        /// </list>
        /// </returns>
        public int CompareTo(IUnit other)
        {
            bool isNull = IsNull;
            bool otherNull = other.IsNull;

            switch (isNull)
            {
                case true when otherNull:
                    return 0;
                case true:
                    return 1;
                case false when otherNull:
                    return -1;
            }
            
            if (UnitDef.GetType() != other.UnitDef.GetType())
                return 0;

            return Power != other.Power
                ? Power.CompareTo(other.Power)
                : Value.CompareTo(
                    other.UnitDef.To(other.Value, UnitDef));
        }

        /// <summary>
        /// Compares the current instance with a double-precision floating-point and returns an integer that indicates whether
        /// the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">Consider this number with the same <see cref="IUnitDef"/>.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <term>Meaning</term>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <term>This instance precedes <paramref name="other"/> in the sort order.</term>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <term>This instance occurs in the same position in the sort order as <paramref name="other"/>.</term>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <term>This instance follows <paramref name="other"/> in the sort order.</term>
        ///     </item>
        /// </list>
        /// </returns>
        public int CompareTo(double other) => 
            Value.CompareTo(other);
        
        /// <summary>
        /// Get the corresponding <see cref="IUnitDef"/> with a different <see cref="IUnit.Power"/> value.
        /// This method is used by the * / operators allowing conversion based on <see cref="Mapping.PowerUnits"/>.
        /// </summary>
        /// <param name="power">Search the corresponding <see cref="IUnitDef"/> registered for this <see cref="IUnit.Power"/>.</param>
        /// <param name="incrementPower">The delta between the initial <see cref="IUnit"/> and the <paramref name="power"/> value.</param>
        /// <param name="compareLimit">Function to validate if <paramref name="power"/> value is inside the available range.</param>
        /// <param name="unitDef"><see cref="IUnitDef"/> instance to search its equivalent power instance.</param>
        /// <returns>The corresponding <see cref="IUnitDef"/> instance for the given <see cref="IUnit.Power"/>.</returns>
        /// <exception cref="PowerOutOfRangeException">
        /// If the <paramref name="power"/> is outside the range of
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMin"/> - <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
        /// </exception>
        private static IUnitDef CorrespondingPowerUnit(byte power, int incrementPower, Func<byte, bool> compareLimit, IUnitDef unitDef)
        {
            IUnitDef newUnitDef = Mapping.GetPowerUnitDef(unitDef, power);
            
            if (!(newUnitDef is null))
                return newUnitDef;

            if (!compareLimit(power) || unitDef.PowerMin > unitDef.PowerMax) 
                return unitDef;
            
            byte newPower = (byte)(power - incrementPower);

            while (compareLimit(newPower))
            {
                newUnitDef = Mapping.GetPowerUnitDef(unitDef, newPower);

                if (!(newUnitDef is null))
                    return newUnitDef;

                newPower = (byte)(newPower - incrementPower);
            }

            if (unitDef.PowerMin == unitDef.PowerMax)
                return unitDef;
            
            throw new PowerOutOfRangeException(
                $"Cannot apply operator on two units with a total power outside the range of {unitDef.PowerMin}-{unitDef.PowerMax}");
        }

        /// <summary>
        /// Search all registered <see cref="IUnitDef"/> instances matching the content of <paramref name="info"/>.
        /// </summary>
        /// <remarks>This method is used to deserialize <see cref="IUnitDef"/> data.</remarks>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated data.</param>
        /// <returns>The first <see cref="IUnitDef"/> instance corresponding to the given <paramref name="info"/>.</returns>
        /// <exception cref="InvalidOperationException">If no registered <see cref="IUnitDef"/> correspond <paramref name="info"/>.</exception>
        private static IUnitDef DeserializeUnitDef(SerializationInfo info) =>
            DeserializeUnitDef(
                info.GetString("unit_name"),
                info.GetString("unit_type"));

        /// <summary>
        /// Search all registered <see cref="IUnitDef"/> instances matching the given serialized dictionary.
        /// </summary>
        /// <remarks>This method is used to deserialize <see cref="IUnitDef"/> data.</remarks>
        /// <returns>The first <see cref="IUnitDef"/> instance corresponding to the given <paramref name="info"/>.</returns>
        /// <exception cref="InvalidOperationException">If no registered <see cref="IUnitDef"/> correspond <paramref name="info"/>.</exception>
        private static IUnitDef DeserializeUnitDef(IReadOnlyDictionary<string, object> info) =>
            DeserializeUnitDef(
                (string)info["unit_name"],
                (string)info["unit_type"]);

        /// <inheritdoc cref="Mapping.GetUnitDefByName"/>
        private static IUnitDef DeserializeUnitDef(string name, string type) =>
            Mapping.GetUnitDefByName(name, type);

        /// <summary>
        /// Indicates whether this instance is equal to another <see cref="Unit"/> instance.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(IUnit other) => 
            Equals(other, Tolerance);

        /// <summary>
        /// Indicates whether this instance is equal to another <see cref="Unit"/> instance by comparing the <see cref="Value"/> with a <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        /// <param name="tolerance">Set how many digit should be considered when comparing both <see cref="Value"/>.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        /// <remarks>
        /// This is useful when comparing two <see cref="Unit"/> with <see cref="UnitDef"/> of huge different
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/>.
        /// For example, when comparing <see cref="Si.Micrometer"/> with <see cref="Si.LightYear"/>.
        /// </remarks>
        public bool Equals(IUnit other, double tolerance)
        {
            if (other is null)
                return false;
            
            if (IsNull || other.IsNull)
                return IsNull && other.IsNull;

            if (UnitDef.GetType() != other.UnitDef.GetType())
                return false;
            
            return Power == other.Power 
                   && Equals(Value, other.UnitDef.To(other.Value, UnitDef, Power), tolerance);
        }

        /// <summary>
        /// Indicates if the difference between two double-precision floating-point are under the given <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="first">The first value to compare with.</param>
        /// <param name="second">The second value to compare with.</param>
        /// <param name="tolerance">Set how many digit should be considered when comparing both numbers.</param>
        /// <returns>true if the <paramref name="first"/> parameter is equal to the <paramref name="second"/> parameter; otherwise, false.</returns>
        private static bool Equals(double first, double second, double tolerance) => 
            Math.Abs(first - second) < tolerance;

        /// <summary>
        /// Indicates whether this instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if the current object is equal to the <paramref name="obj"/> parameter; otherwise, false.</returns>
        public override bool Equals(object obj) => 
            obj is Unit other && Equals(other);
        
        /// <summary>
        /// Hash function
        /// </summary>
        /// <returns>A hash code for current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return UnitDef is null 
                    ? 0 
                    : (UnitDef.GetType().GetHashCode() * 397) ^ ToBaseUnit(this).GetHashCode();
            }
        }
        
        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target <see cref="Unit"/>.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", Value);
            info.AddValue("power", Power);
            info.AddValue("unit_name", UnitDef.Name);
            info.AddValue("unit_type", UnitDef.GetType().Name);
        }

        /// <inheritdoc cref="IUnit.IsAbstractUnit"/>
        public bool IsAbstractUnit => 
            UnitDef is null || UnitDef.IsAbstractUnit && AbstractFormula is null;

        /// <inheritdoc cref="IUnit.IsNull"/>
        public bool IsNull =>
            UnitDef is null || double.IsNaN(Value);


        /// <summary>
        /// Validate the given <paramref name="power"/> is in the range of the given <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="power">Value to validate.</param>
        /// <param name="unitDef">
        /// Make sure the given <paramref name="power"/> is withing the range of
        /// unitDef.<see cref="IUnitDef.PowerMin"/> - unitDef.<see cref="IUnitDef.PowerMax"/>.
        /// </param>
        /// <exception cref="PowerOutOfRangeException">If the given <paramref name="power"/> is out of range.</exception>
        private static void ValidatePowerInRange(byte power, IUnitDef unitDef)
        {
            if (power < unitDef.PowerMin
                || unitDef.PowerMax >= unitDef.PowerMin 
                && power > unitDef.PowerMax)
                
                throw new PowerOutOfRangeException(
                    $"Cannot apply operator on two units with a total power outside the range of {unitDef.PowerMin}-{unitDef.PowerMax}");
        }
    }
}
