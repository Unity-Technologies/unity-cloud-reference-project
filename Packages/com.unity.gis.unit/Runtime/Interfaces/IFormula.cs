
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Formula is a intermediate struct allowing to concatenate lambda functions via operators.
    ///
    /// This is used by <see cref="IUnitDef"/>.<see cref="IUnitDef.Formula"/> allowing to specify the <see cref="IUnitDef"/>.<see cref="IUnitDef.Scale"/> factor
    /// to convert to the <see cref="IUnitDef"/>.<see cref="IUnitDef.BaseUnitDef"/>.
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
    public interface IFormula
    {
        /// <summary>
        /// Execute the <see cref="Formula"/>.<see cref="Formula.Function"/> with the <paramref name="value"/> as the main input parameter. 
        /// </summary>
        /// <param name="value">Value to pass through the <see cref="Formula"/>.<see cref="Formula.Function"/>.</param>
        /// <returns>Returns the <see cref="Formula"/>.<see cref="Formula.Function"/> result as a double.</returns>
        double Invoke(double value);
    }
}
