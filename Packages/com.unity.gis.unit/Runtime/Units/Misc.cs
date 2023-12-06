
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// None official units.
    /// </summary>
    public static class Misc
    {
        /// <summary>
        /// NaN <see cref="Unit"/> without any assigned <see cref="Unit.UnitDef"/> nor <see cref="Unit.Value"/>.
        /// </summary>
        public static readonly Unit Null = new();
        

        /// <summary>
        /// Old measurement of radioactive decay defined in 1910 named in honour of Pierre Curie / Marie Curie.
        /// </summary>
        public static readonly Radioactivity Curie = new(new UnitNaming("curie", " Ci"), Si.Becquerel * 3.7e10);

        /// <summary>
        /// Old measurement of radioactive decay defined in 1946. Named after Lord Ernest Rutherford.
        /// </summary>
        public static readonly Radioactivity Rutherford = new(new UnitNaming("rutherford", " Rd"), Si.Becquerel * 1_000_000);

        
    }
}
