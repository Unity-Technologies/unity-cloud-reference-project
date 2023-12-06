
using System;
using System.Collections.Generic;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Interface for Unit Definition allowing to convert a <see cref="Unit"/> from one <see cref="IUnitDef"/> instance to an other.
    /// </summary>
    /// <example>
    /// <code>
    /// Unit cm = new Unit(5, Si.Centimeter);
    /// Unit inch = cm.To(Imperial.Inch);
    /// </code>
    /// </example>
    public interface IUnitDef
    {

        /// <summary>
        /// Array of other possible names for this definition.
        /// Items part of this array will be used for decoding values from string.
        /// </summary>
        /// <remarks>No need to add plural names if only an "s" is added as a suffix, this is already automatically considered.</remarks>
        string[] AlternateNames { get; }
        
        /// <summary>
        /// Method with one double parameter representing a <see cref="IUnitDef"/> value as the <see cref="BaseUnitDef"/>
        /// (the <see cref="IUnitDef"/> with a <see cref="Scale"/> of 1) and returning the converted value for this
        /// <see cref="IUnitDef"/> instance.
        /// </summary>
        IFormula Formula { get; }

        /// <summary>
        /// Get if the unit represent a none defined unit which require external <see cref="Scale"/> value to be converted.
        /// </summary>
        /// <example><see cref="Storey.Abstract"/></example>
        bool IsAbstractUnit { get; }
        
        /// <summary>
        /// Main full name defining the <see cref="IUnitDef"/>.
        /// </summary>
        /// <remarks>This is the none plural form and not an abbreviation</remarks>
        string Name { get; }
        
        /// <summary>
        /// Main full name defining the <see cref="IUnitDef"/> in its plural form.
        /// </summary>
        string NamePlural { get; }
        
        /// <summary>
        /// Maximum possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.
        /// </summary>
        byte PowerMax { get; }
        
        /// <summary>
        /// Minimal possible value you can set on <see cref="Unit"/>.<see cref="Unit.Power"/>.
        /// </summary>
        byte PowerMin { get; }
        
        /// <summary>
        /// Abbreviation or utf-8 sign representing a shorter version of the <see cref="Name"/>.
        /// </summary>
        string Symbol { get; }

        
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance with this instance set as the <see cref="Unit"/>.<see cref="Unit.UnitDef"/>.
        /// </summary>
        /// <param name="value">Set <see cref="Unit"/>.<see cref="Unit.Value"/> with this double-precision floating-point.</param>
        /// <param name="power">Set <see cref="Unit"/>.<see cref="Unit.Power"/> with this value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance based on the given values.</returns>
        Unit From(double value, byte power = 0);
        
        /// <summary>
        /// Create a new <see cref="Unit"/> instance by parsing the given <paramref name="text"/>.
        /// If the <paramref name="text"/> as a unit following a number, the value will be converted to this <see cref="IUnitDef"/> instance.
        /// If the <paramref name="text"/> as no unit information, only the first double-precision floating-point will be extracted.
        /// </summary>
        /// <param name="text">Find the <see cref="Unit"/>.<see cref="Unit.Value"/> part of this text.</param>
        /// <param name="power">
        /// Override the <see cref="Unit"/>.<see cref="Unit.Power"/>. Will work only if compatible with this <see cref="IUnitDef"/> instance.
        /// Set to 0 to extract the power value from <paramref name="text"/>.
        /// </param>
        /// <returns>A newly created <see cref="Unit"/> instance based on the given values.</returns>
        /// <exception cref="DifferentTypesException">
        /// If a unit is part of the <paramref name="text"/> and is not compatible with this <see cref="IUnitDef"/> instance.
        /// </exception>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match the given <see cref="IUnitDef"/> pattern.</exception>
        /// <exception cref="ExponentPatternException">If the given <paramref name="power"/> value is not compatible with the decoded <see cref="Unit"/>.<see cref="Unit.UnitDef"/>.</exception>
        Unit From(string text, byte power = 0);
        
        /// <summary>
        /// Convert a <see cref="IUnit"/>.<see cref="IUnit.Value"/> to this instance of <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="value"><see cref="IUnit"/> to convert.</param>
        /// <returns>
        /// Create a new <see cref="Unit"/> with this instance set as the <see cref="Unit"/>.<see cref="Unit.UnitDef"/>
        /// and set the converted value to <see cref="Unit"/>.<see cref="Unit.Value"/>.
        /// </returns>
        Unit From(IUnit value);

        /// <summary>
        /// Convert a <see cref="IUnit"/>.<see cref="IUnit.Value"/> to this instance of <see cref="IUnitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="IUnit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="value"><see cref="IUnit"/> to convert.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>
        /// Create a new <see cref="Unit"/> with this instance set as the <see cref="Unit"/>.<see cref="Unit.UnitDef"/>
        /// and set the converted value to <see cref="Unit"/>.<see cref="Unit.Value"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// Unit storey = Storey.Abstract.From(2);
        /// Unit meters = Si.Meter.From(storey, 4.25)  // each storey is 4.25 meter
        /// </code>
        /// </example>
        Unit From(IUnit value, double scale);
        
        /// <summary>
        /// Convert a <see cref="IUnit"/>.<see cref="IUnit.Value"/> to this instance of <see cref="IUnitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="value"><see cref="IUnit"/> to convert.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>
        /// Create a new <see cref="Unit"/> with this instance set as the <see cref="Unit"/>.<see cref="Unit.UnitDef"/>
        /// and set the converted value to <see cref="Unit"/>.<see cref="Unit.Value"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// Unit storey = Storey.Abstract.From(2);
        /// Unit meters = Si.Meter.From(storey, x => x * 4.25)  // each storey is 4.25 meter
        /// </code>
        /// </example>
        Unit From(IUnit value, Func<double, double> formula);
        
        /// <summary>
        /// Convert a <see cref="IUnit"/>.<see cref="IUnit.Value"/> to this instance of <see cref="IUnitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="value"><see cref="IUnit"/> to convert.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>
        /// Create a new <see cref="Unit"/> with this instance set as the <see cref="Unit"/>.<see cref="Unit.UnitDef"/>
        /// and set the converted value to <see cref="Unit"/>.<see cref="Unit.Value"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// Unit storey = Storey.Abstract.From(2);
        /// Unit meters = Si.Meter.From(storey, Feet * 14)  // each storey is 14 feet
        /// </code>
        /// </example>
        Unit From(IUnit value, IFormula formula);
        
        /// <summary>
        /// Convert a double-precision floating-point by considering the source this <see cref="IUnitDef"/> instance
        /// and the destination to be <paramref name="unitDef"/>.
        /// </summary>
        /// <param name="value">Double-precision floating-point to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="value"/> to.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>The double-precision floating-point result of the conversion.</returns>
        double To(double value, IUnitDef unitDef, byte power = 0);
        
        /// <summary>
        /// Convert a double-precision floating-point by considering the source this <see cref="IUnitDef"/> instance
        /// and the destination to be <paramref name="unitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="IUnit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="value">Double-precision floating-point to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="value"/> to.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>The double-precision floating-point result of the conversion.</returns>
        /// <example>
        /// <code>
        /// double meters = Storey.Abstract.To(2, Si.Meter, 4.25)  // each storey is 4.25 meter
        /// </code>
        /// </example>
        double To(double value, IUnitDef unitDef, double scale, byte power = 0);
        
        /// <summary>
        /// Convert a double-precision floating-point by considering the source this <see cref="IUnitDef"/> instance
        /// and the destination to be <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="value">Double-precision floating-point to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="value"/> to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>The double-precision floating-point result of the conversion.</returns>
        /// <example>
        /// <code>
        /// double meters = Storey.Abstract.To(2, Si.Meter, x => x * 4.25)  // each storey is 4.25 meter
        /// </code>
        /// </example>
        double To(double value, IUnitDef unitDef, Func<double, double> formula, byte power = 0);
        
        /// <summary>
        /// Convert a double-precision floating-point by considering the source this <see cref="IUnitDef"/> instance
        /// and the destination to be <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="value">Double-precision floating-point to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="value"/> to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>The double-precision floating-point result of the conversion.</returns>
        /// <example>
        /// <code>
        /// double meters = Storey.Abstract.To(2, Si.Meter, Feet * 14)  // each storey is 14 feet
        /// </code>
        /// </example>
        double To(double value, IUnitDef unitDef, IFormula formula, byte power = 0);

        /// <summary>
        /// Convert multiple double-precision floating-point values by considering the source this <see cref="IUnitDef"/> 
        /// instance and the destination to be <paramref name="unitDef"/>.
        /// </summary>
        /// <param name="values">The double-precision floating-point values to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="values"/> to.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>Enumerate the converted double-precision floating-point values.</returns>
        IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, byte power = 0);
        
        /// <summary>
        /// Convert multiple double-precision floating-point values by considering the source this <see cref="IUnitDef"/> 
        /// instance and the destination to be <paramref name="unitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="IUnit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="values">The double-precision floating-point values to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="values"/> to.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>Enumerate the converted double-precision floating-point values.</returns>
        /// <example>
        /// <code>
        /// double meters = Storey.Abstract.To(new [] {2, 5.5, 12.6}, Si.Meter, 4.25)  // each storey is 4.25 meter
        /// </code>
        /// </example>
        IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, double scale, byte power = 0);
        
        /// <summary>
        /// Convert multiple double-precision floating-point values by considering the source this <see cref="IUnitDef"/> 
        /// instance and the destination to be <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="values">The double-precision floating-point values to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="values"/> to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>Enumerate the converted double-precision floating-point values.</returns>
        /// <example>
        /// <code>
        /// double meters = Storey.Abstract.To(new [] {2, 5.5, 12.6}, Si.Meter, x => x * 4.25)  // each storey is 4.25 meter
        /// </code>
        /// </example>
        IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, Func<double, double> formula, byte power = 0);
        
        /// <summary>
        /// Convert multiple double-precision floating-point values by considering the source this <see cref="IUnitDef"/> 
        /// instance and the destination to be <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="values">The double-precision floating-point values to convert.</param>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the <paramref name="values"/> to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>Enumerate the converted double-precision floating-point values.</returns>
        /// <example>
        /// <code>
        /// double meters = Storey.Abstract.To(new [] {2, 5.5, 12.6}, Si.Meter, Feet * 14)  // each storey is 14 feet
        /// </code>
        /// </example>
        IEnumerable<double> To(IEnumerable<double> values, IUnitDef unitDef, IFormula formula, byte power = 0);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from this <see cref="IUnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from this
        /// <see cref="IUnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        Func<double, double> ToFormula(IUnitDef unitDef, byte power = 0);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from this <see cref="IUnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="IUnit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from this
        /// <see cref="IUnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// var formula = Storey.Abstract.ToFormula(Si.Meter, 4.25)  // each storey is 4.25 meter
        /// double meters = formula(2)  // Convert 2 storey to meters
        /// </code>
        /// </example>
        Func<double, double> ToFormula(IUnitDef unitDef, double scale, byte power = 0);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from this <see cref="IUnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from this
        /// <see cref="IUnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// var formula = Storey.Abstract.ToFormula(Si.Meter, x => x * 4.25)  // each storey is 4.25 meter
        /// double meters = formula(2)  // Convert 2 storey to meters
        /// </code>
        /// </example>
        Func<double, double> ToFormula(IUnitDef unitDef, Func<double, double> formula, byte power = 0);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from this <see cref="IUnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <param name="power">If the <see cref="PowerMin"/> is not equal to <see cref="PowerMax"/>, specify the power to convert.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from this
        /// <see cref="IUnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// var formula = Storey.Abstract.ToFormula(Si.Meter, Feet * 14)  // each storey is 14 feet
        /// double meters = formula(2)  // Convert 2 storey to meters
        /// </code>
        /// </example>
        Func<double, double> ToFormula(IUnitDef unitDef, IFormula formula, byte power = 0);
        
        /// <summary>
        /// Format a double-precision floating-point value as the prefix and the <see cref="Name"/> as the suffix.
        /// </summary>
        /// <example>5 cubic units</example>
        /// <param name="value">Amount of units to set as the prefix</param>
        /// <param name="power">Insert the power value before the <see cref="IUnitDef"/>.<see cref="IUnitDef.Name"/>.</param>
        /// <param name="provider">String format provider like <see cref="IUnitFormatter"/>.</param>
        /// <returns>The formatted string.</returns>
        string ToFullString(double value, byte power = 1, IFormatProvider provider = null);

        /// <summary>
        /// Get the power equivalent as a name.
        /// </summary>
        /// <param name="power">Number to convert representing a <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.</param>
        /// <returns>The power name based on <paramref name="power"/>.</returns>
        string ToFullStringPower(byte power);
        
        /// <summary>
        /// Format a double-precision floating-point value in the short version.
        /// </summary>
        /// <example>5 x³</example>
        /// <param name="value">Amount of units to set as the prefix</param>
        /// <param name="power">Superscript value to display.</param>
        /// <param name="provider">String format provider like <see cref="IUnitFormatter"/>.</param>
        /// <returns>The formatted string.</returns>
        string ToString(double value, byte power = 1, IFormatProvider provider = null);

        /// <summary>
        /// Get the superscript equivalent of the given <paramref name="power"/>.
        /// </summary>
        /// <param name="power">Number to convert representing a <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.</param>
        /// <returns>The power symbol based on <paramref name="power"/>.</returns>
        string ToStringPower(byte power);

        
        
        /// <summary>
        /// Addition operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override + operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the addition from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the addition with.</param>
        /// <returns>The addition result as a double-precision floating-point value.</returns>
        double Add(IUnit first, IUnit second);
        
        /// <summary>
        /// Subtraction operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override - operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the subtraction from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the subtraction with.</param>
        /// <returns>The subtraction result as a double-precision floating-point value.</returns>
        double Sub(IUnit first, IUnit second);
        
        /// <summary>
        /// Multiplication operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override * operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the multiplication from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the multiplication with.</param>
        /// <returns>The multiplication result as a double-precision floating-point value.</returns>
        double Mult(IUnit first, IUnit second);
        
        /// <summary>
        /// Division operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override / operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the division from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the division with.</param>
        /// <returns>The division result as a double-precision floating-point value.</returns>
        double Div(IUnit first, IUnit second);
        
        /// <summary>
        /// Modulo operation between two <see cref="IUnit"/>.
        /// </summary>
        /// <remarks>Allow to override % operator per <see cref="IUnitDef"/> type.</remarks>
        /// <param name="first">First <see cref="IUnit"/> to execute the modulo from.</param>
        /// <param name="second">Second <see cref="IUnit"/> to execute the modulo with.</param>
        /// <returns>The modulo result as a double-precision floating-point value.</returns>
        double Modulo(IUnit first, IUnit second);

        /// <summary>
        /// The base <see cref="IUnitDef"/> is the main unit used for the same type. This is also the <see cref="IUnitDef"/> 
        /// that is normally used when serializing / storing a <see cref="Unit"/> to a unique <see cref="IUnitDef"/>.
        /// </summary>
        /// <remarks>The base unit does not always have a <see cref="Scale"/> of 1, it's a arbitrary default <see cref="IUnitDef"/> to be used per type.</remarks>
        IUnitDef BaseUnitDef { get; }
        
        /// <summary>
        /// Get all the names and symbols that can represent the <see cref="IUnitDef"/>.
        /// </summary>
        IEnumerable<string> Names { get; }

        /// <summary>
        /// Create a new formula that returns a the power result of the result of <see cref="Formula"/>.
        /// </summary>
        /// <param name="power">Calculate the <see cref="Formula"/> result with with power.</param>
        /// <returns>A new <see cref="IFormula"/> when invoke returns the<see cref="Scale"/></returns>
        IFormula Pow(double power);
        
        /// <summary>
        /// Register this <see cref="IUnitDef"/> instance as the <see cref="BaseUnitDef"/> for this instance type.
        /// </summary>
        void RegisterAsBaseUnit();
        
        /// <summary>
        /// Register a <see cref="IUnitDef"/> sibling for this instance.
        /// </summary>
        /// <param name="power">Sibling power value to register the <paramref name="powerUnitDef"/> for.</param>
        /// <param name="powerUnitDef">Sibling to register.</param>
        void RegisterPowerUnitDef(byte power, IUnitDef powerUnitDef);

        /// <summary>
        /// Register all the <see cref="IUnitDef"/> sibling by power for this instance.
        /// </summary>
        /// <param name="mapping">
        /// <see cref="Dictionary{TKey,TValue}"/> where the keys are the <see cref="Unit"/>.<see cref="Unit.Power"/> value
        /// and the value is the corresponding <see cref="IUnitDef"/> to instantiating when converting the <see cref="IUnitDef"/>
        /// to a different power value.
        /// </param>
        void RegisterPowerUnitDefs(Dictionary<byte, IUnitDef> mapping);
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnitDef"/> to a <see cref="IUnitDef"/> with a
        /// <see cref="Scale"/> factor of 1 and a <see cref="IUnit"/>.<see cref="IUnit.Power"/> value of 1.
        /// </summary>
        /// <remarks>
        /// Use <see cref="To(double,IUnitDef,byte)"/> when converting values.
        /// To multiply by the <see cref="Scale"/> value will not consider the <see cref="IUnit"/>.<see cref="IUnit.Power"/>
        /// value and some <see cref="IUnitDef"/> cannot be converted with only a multiplication.
        /// </remarks>
        double Scale { get; }
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnitDef"/> to a <see cref="IUnitDef"/> with a
        /// <see cref="Scale"/> factor of 1, a <see cref="IUnit"/>.<see cref="IUnit.Power"/> value of 1 and a multiplier
        /// if this instance <see cref="IsAbstractUnit"/> is true.
        /// </summary>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>The scaling factor with the given <paramref name="scale"/>.</returns>
        /// <remarks>
        /// Use <see cref="To(double,IUnitDef,double,byte)"/> when converting values.
        /// Some <see cref="IUnitDef"/> cannot be converted with only a multiplication.
        /// </remarks>
        double ScaleAbstract(double scale);
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnitDef"/> to a <see cref="IUnitDef"/> with a
        /// <see cref="Scale"/> factor of 1, a <see cref="IUnit"/>.<see cref="IUnit.Power"/> value of 1 and a multiplier
        /// if this instance <see cref="IsAbstractUnit"/> is true.
        /// </summary>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>The scaling factor with the given <paramref name="formula"/>.</returns>
        /// <remarks>
        /// Use <see cref="To(double,IUnitDef,Func{double,double},byte)"/> when converting values.
        /// Some <see cref="IUnitDef"/> cannot be converted with only a multiplication.
        /// </remarks>
        double ScaleAbstract(Func<double, double> formula);
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnitDef"/> to a <see cref="IUnitDef"/> with a
        /// <see cref="Scale"/> factor of 1, a <see cref="IUnit"/>.<see cref="IUnit.Power"/> value of 1 and a multiplier
        /// if this instance <see cref="IsAbstractUnit"/> is true.
        /// </summary>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>The scaling factor with the given <paramref name="formula"/>.</returns>
        /// <remarks>
        /// Use <see cref="To(double,IUnitDef,IFormula,byte)"/> when converting values.
        /// Some <see cref="IUnitDef"/> cannot be converted with only a multiplication.
        /// </remarks>
        double ScaleAbstract(IFormula formula);
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnitDef"/> to a <see cref="IUnitDef"/> with a
        /// <see cref="Scale"/> factor of 1 and considering the <paramref name="unit"/>.<see cref="IUnit.Power"/> value.
        /// </summary>
        /// <param name="unit"><see cref="IUnit"/> to calculate the scaling factor for.</param>
        /// <returns>The scaling factor for the given <paramref name="unit"/>.</returns>
        /// <remarks>
        /// Use <see cref="To(double,IUnitDef,byte)"/> when converting values.
        /// Some <see cref="IUnitDef"/> cannot be converted with only a multiplication.
        /// </remarks>
        double ScalePow(IUnit unit);

    }
}
