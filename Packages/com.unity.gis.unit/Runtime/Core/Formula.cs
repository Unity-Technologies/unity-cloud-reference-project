
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Formula is a intermediate struct allowing to concatenate lambda functions via operators.
    ///
    /// This is used by <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Formula"/> allowing to specify the <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Scale"/> factor
    /// to convert to the <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.BaseUnitDef"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// Formula f = new Formula(x => x * 2);
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// Formula f1 = Si.Centimeter * 2;
    /// Formula f2 = f1 * 1 / 20;
    /// </code>
    /// </example>
    public readonly struct Formula : IFormula
    {
        /// <summary>
        /// <see cref="Func{T,T}"/> to be executed to convert a value from a <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.BaseUnitDef"/> to a new <see cref="IUnitDef"/>.
        /// </summary>
        public readonly Func<double, double> Function;
        
        
        /// <summary>
        /// Create a new <see cref="Formula"/> instance based on a function transforming a double value to a new double value.
        /// </summary>
        /// <param name="function">Function with a double parameter and a returns a new double value.</param>
        public Formula(Func<double, double> function) =>
            Function = function;

        /// <summary>
        /// Join a <see cref="Formula"/> with a function by giving the <see cref="Formula"/> result to the function.
        /// </summary>
        /// <param name="formula"><see cref="Formula"/> giving its result to the function.</param>
        /// <param name="function">Function with a double parameter and a returns a new double value.</param>
        internal Formula(IFormula formula, Func<double, double> function) :
            this(value => function(formula.Invoke(value))) { }
        
        /// <summary>
        /// Join a function with a <see cref="Formula"/> by giving the function result to the <see cref="Formula"/>.
        /// </summary>
        /// <param name="function">Function with a double parameter and a returns a new double value that will be given to the formula.</param>
        /// <param name="formula"><see cref="Formula"/> giving the final result of the new <see cref="Formula"/> instance.</param>
        internal Formula(Func<double, double> function, IFormula formula) :
            this(value => formula.Invoke(function(value))) { }

        /// <summary>
        /// Join two functions by giving the first function result to the second function.
        /// </summary>
        /// <param name="function1">Function with a double parameter and a returns a new double value that will be given to the second function.</param>
        /// <param name="function2">Function with a double parameter and a returns a new double value returning the final <see cref="Formula"/> result.</param>
        internal Formula(Func<double, double> function1, Func<double, double> function2) :
            this(value => function2(function1(value))) { }

        
        
        /// <summary>
        /// Create a new <see cref="Formula"/> by adding the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="formula"><see cref="Formula"/> to add its result with the given <paramref name="value"/>.</param>
        /// <param name="value">Add this <paramref name="value"/> with the result of the <paramref name="formula"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator +(Formula formula, double value) => 
            new(formula, v => v + value);

        /// <summary>
        /// Create a new <see cref="Formula"/> by adding the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Add this <paramref name="value"/> with the result of the <paramref name="formula"/>.</param>
        /// <param name="formula"><see cref="Formula"/> to add its result with the given <paramref name="value"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator +(double value, Formula formula) => 
            new(formula, v => value + v);

        /// <summary>
        /// Create a new <see cref="Formula"/> by subtracting the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="formula"><see cref="Formula"/> to subtract its result with the given <paramref name="value"/>.</param>
        /// <param name="value">Subtract this <paramref name="value"/> with the result of the <paramref name="formula"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator -(Formula formula, double value) => 
            new(formula, v => v - value);

        /// <summary>
        /// Create a new <see cref="Formula"/> by subtracting the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Subtract this <paramref name="value"/> with the result of the <paramref name="formula"/>.</param>
        /// <param name="formula"><see cref="Formula"/> to subtract its result with the given <paramref name="value"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator -(double value, Formula formula) => 
            new(formula, v => value - v);

        /// <summary>
        /// Create a new <see cref="Formula"/> by multiplying the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="formula"><see cref="Formula"/> to multiply its result with the given <paramref name="value"/>.</param>
        /// <param name="value">Multiply this <paramref name="value"/> with the result of the <paramref name="formula"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator *(Formula formula, double value) => 
            new(formula, v => v * value);

        /// <summary>
        /// Create a new <see cref="Formula"/> by multiplying the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Multiply this <paramref name="value"/> with the result of the <paramref name="formula"/>.</param>
        /// <param name="formula"><see cref="Formula"/> to multiply its result with the given <paramref name="value"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator *(double value, Formula formula) => 
            new(formula, v => value * v);

        /// <summary>
        /// Create a new <see cref="Formula"/> by dividing the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="formula"><see cref="Formula"/> to divide its result with the given <paramref name="value"/>.</param>
        /// <param name="value">Divide by this <paramref name="value"/> the result of the <paramref name="formula"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator /(Formula formula, double value) => 
            new(formula, v => v / value);

        /// <summary>
        /// Create a new <see cref="Formula"/> by dividing the given <paramref name="value"/> with the result of the <paramref name="formula"/>.
        /// </summary>
        /// <param name="formula">Divide the given <paramref name="value"/> with its result with this <see cref="Formula"/>.</param>
        /// <param name="value">Divide this <paramref name="value"/> by the result of the <paramref name="formula"/>.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator /(double value, Formula formula) => 
            new(formula, v => value / v);

        /// <summary>
        /// Create a new <see cref="Formula"/> by applying remainder operation to the result of the <paramref name="formula"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="formula">Apply remainder operation with the result of this <see cref="Formula"/>.</param>
        /// <param name="value">Right-hand operand of the remainder operator.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator %(Formula formula, double value) => 
            new(formula, v => v % value);

        /// <summary>
        /// Create a new <see cref="Formula"/> by applying remainder operation to the given <paramref name="value"/> with the result of the <paramref name="formula"/>.
        /// </summary>
        /// <param name="value">Apply remainder operation with this <paramref name="value"/>.</param>
        /// <param name="formula">Right-hand operand of the remainder operator.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with the <paramref name="value"/>.</returns>
        public static Formula operator %(double value, Formula formula) => 
            new(formula, v => value % v);

        
        
        /// <summary>
        /// Execute the <see cref="Formula"/>.<see cref="Formula.Function"/> with the <paramref name="value"/> as the input parameter. 
        /// </summary>
        /// <param name="value">Value to pass through the <see cref="Formula"/>.<see cref="Formula.Function"/>.</param>
        /// <returns>Returns the <see cref="Formula"/>.<see cref="Formula.Function"/> result as a double.</returns>
        public double Invoke(double value) =>
            Function(value);

        /// <summary>
        /// Create a new <see cref="Formula"/> by raising the result of the <see cref="Formula"/> to the <paramref name="power"/> value.
        /// </summary>
        /// <param name="power">A double-precision floating-point number that specifies a power.</param>
        /// <returns>Returns a new <see cref="Formula"/> instance joining the formula with a power function.</returns>
        public Formula Pow(double power) =>
            new(this, v => Math.Pow(v, power));
        
    }
}
