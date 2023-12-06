
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Rules on how <see cref="UnitDef{T}"/> can be stored as a string.
    /// </summary>
    public struct UnitNaming
    {
        /// <inheritdoc cref="IUnitDef.Name"/>
        public string Name { get; }
        
        /// <inheritdoc cref="IUnitDef.NamePlural"/>
        public string NamePlural { get; }
        
        /// <inheritdoc cref="IUnitDef.Symbol"/>
        public string Symbol { get; }
        
        /// <inheritdoc cref="IUnitDef.AlternateNames"/>
        public string[] AlternativeNaming { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Main full name defining the <see cref="UnitDef{T}"/>.</param>
        /// <param name="plural">Main full name defining the <see cref="UnitDef{T}"/> in its plural form.</param>
        /// <param name="symbol">Abbreviation or utf-8 sign representing a shorter version of the <see cref="Name"/>.</param>
        /// <param name="alternativeNaming">
        /// Array of other possible names for this definition.
        /// Items part of this array will be used for decoding values from string.
        /// </param>
        public UnitNaming(string name, string plural, string symbol, params string[] alternativeNaming)
        {
            Name = name;
            NamePlural = plural;
            Symbol = symbol;
            AlternativeNaming = alternativeNaming;
        }
        
        /// <summary>
        /// Constructor that will set the <see cref="NamePlural"/> the same as <see cref="Name"/> with an "s" at the end.
        /// </summary>
        /// <param name="name">Main full name defining the <see cref="UnitDef{T}"/>.</param>
        /// <param name="symbol">Abbreviation or utf-8 sign representing a shorter version of the <see cref="Name"/>.</param>
        /// <param name="alternativeNaming">
        /// Array of other possible names for this definition.
        /// Items part of this array will be used for decoding values from string.
        /// </param>
        public UnitNaming(string name, string symbol, params string[] alternativeNaming) :
            this(name, $"{name}s", symbol, alternativeNaming) { }
        
    }
}
