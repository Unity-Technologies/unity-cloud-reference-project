
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Unit of measurement interface linking a double-precision floating-point with a <see cref="IUnitDef"/> allowing to convert
    /// the value to other <see cref="IUnitDef"/>.
    /// </summary>
    public interface IUnit : 
        IEquatable<IUnit>, 
        IComparable<IUnit>, 
        IComparable<double>,
        ISerializable
    {
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnit"/> to a <see cref="IUnitDef"/>
        /// if <see cref="IsAbstractUnit"/> is true. Without a <see cref="AbstractFormula"/>,
        /// <see cref="To(IUnitDef, IFormula)"/> needs to be used when converting to
        /// a different <see cref="IUnitDef"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// Unit src = new Unit(2, Storey.Abstract, Imperial.Feet * 14)
        /// bool absU = src.IsAbstractUnit  // false
        /// Unit dst = src.To(Imperial.Feet)  // 28 ft
        /// </code>
        /// <code>
        /// Unit src = new Unit(2, Storey.Abstract)
        /// bool absU = src.IsAbstractUnit  // true
        /// Unit dst = src.To(Si.Meter)  // Will fail since no AbstractFormula is assigned and <see cref="IsAbstractUnit"/> is true.
        /// </code>
        /// </example>
        /// <seealso cref="To(IUnitDef, IFormula)"/>
        Func<double, double> AbstractFormula { get; }
        
        /// <summary>
        /// Unit power defining the amount of axis this <see cref="IUnit"/> represent.
        /// </summary>
        /// <example>
        /// Unit pow1 = Si.Centimeter.From(5.0);
        /// Unit pow2 = Si.Centimeter2.From(5.0);
        /// Unit pow3 = Si.Centimeter3.From(5.0);
        ///
        /// pow2 = pow1 * pow1;
        /// pow3 = pow1 * pow1 * pow1;
        /// pow3 = pow1 * pow2;
        /// </example>
        byte Power { get; }
        
        /// <summary>
        /// The scaling factor when converting this <see cref="IUnit"/> to a <see cref="IUnitDef"/> with a
        /// <see cref="Scale"/> factor of 1.
        /// </summary>
        /// <remarks>
        /// Use <see cref="To(IUnitDef)"/> when converting values.
        /// Multiplication by the <see cref="Scale"/> value is not always the conversion operation.
        /// </remarks>
        double Scale { get; }
        
        /// <summary>
        /// Unit of measurement allowing conversion to an other Unit Definition.
        /// </summary>
        IUnitDef UnitDef { get; }
        
        /// <summary>
        /// Amount / Quantity of units.
        /// </summary>
        double Value { get; }
        

        
        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        Unit To(IUnitDef unitDef);
        
        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="IUnit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        Unit To(IUnitDef unitDef, double scale);
        
        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        Unit To(IUnitDef unitDef, Func<double, double> formula);
        
        /// <summary>
        /// Convert the <see cref="Value"/> to a different <see cref="IUnitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Convert the <see cref="Value"/> to this new <see cref="IUnitDef"/>.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        Unit To(IUnitDef unitDef, IFormula formula);

        /// <summary>
        /// Convert the <see cref="Value"/> to the <see cref="IUnitDef"/> defined by <see cref="IUnitDef.BaseUnitDef"/>
        /// for the <see cref="IUnitDef"/> type.
        /// </summary>
        /// <returns>A newly created <see cref="Unit"/> instance.</returns>
        Unit ToBaseUnit();
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from <see cref="UnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from
        /// <see cref="UnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        Func<double, double> ToFormula(IUnitDef unitDef);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from <see cref="UnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="scale"/> defining the abstract <see cref="IUnit"/> value for 1 <see cref="Unit"/>
        /// of this instance.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="scale">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from
        /// <see cref="UnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        Func<double, double> ToFormula(IUnitDef unitDef, double scale);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from <see cref="UnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from
        /// <see cref="UnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        Func<double, double> ToFormula(IUnitDef unitDef, Func<double, double> formula);
        
        /// <summary>
        /// Get a method allowing to convert a double-precision floating-point value from <see cref="UnitDef"/>
        /// to <paramref name="unitDef"/>.
        /// Uses the <paramref name="formula"/> to convert the abstract <see cref="IUnit"/> to a
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> of 1.
        /// </summary>
        /// <param name="unitDef">Destination of the <see cref="IUnitDef"/> to convert the method parameter input to.</param>
        /// <param name="formula">Specify the scale factor to convert the abstract value to a 1:1 value.</param>
        /// <returns>
        /// A method receiving a double-precision floating-point value to convert from
        /// <see cref="UnitDef"/> instance and output the converted value to <paramref name="unitDef"/>.
        /// </returns>
        Func<double, double> ToFormula(IUnitDef unitDef, IFormula formula);
        
        /// <summary>
        /// Format the <see cref="IUnit"/> with the double-precision floating-point value as the prefix and the
        /// <see cref="IUnitDef"/>.<see cref="IUnitDef.Name"/> as the suffix.
        /// </summary>
        /// <example>5 cubic units</example>
        /// <returns>The formatted string.</returns>
        string ToFullString();

        /// <summary>
        /// Format the <see cref="IUnit"/> with a configured IUnitFormat provider.
        /// </summary>
        /// <param name="provider"><see cref="IUnitFormatter"/> used to format the unit to a string.</param>
        /// <returns>A formatted string.</returns>
        string ToFullString(IUnitFormatter provider);

        /// <summary>
        /// Format the <see cref="IUnit"/> with a configured IUnitFormat provider.
        /// </summary>
        /// <param name="provider"><see cref="IUnitFormatter"/> used to format the unit to a string.</param>
        /// <returns>A formatted string</returns>
        string ToString(IUnitFormatter provider);

        /// <summary>
        /// Get if the <see cref="UnitDef"/> represent a none defined unit which require external <see cref="Scale"/> value to be converted.
        /// </summary>
        /// <example><see cref="Storey.Abstract"/></example>
        bool IsAbstractUnit { get; }
        
        /// <summary>
        /// true if the <see cref="UnitDef"/> is not set.
        /// Null <see cref="IUnit"/> instances have no value and represent an invalid unit.
        /// </summary>
        bool IsNull { get; }

    }
}
