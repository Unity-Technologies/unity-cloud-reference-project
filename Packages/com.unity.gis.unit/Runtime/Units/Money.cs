
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Currency units.
    /// </summary>
    public static class Money
    {
        static Money()
        {
            Usd.RegisterAsBaseUnit();
        }

        /// <summary>
        /// Main currency for the United States of America.
        /// </summary>
        public static readonly Currency Usd = new(new UnitNaming("US Dollar", "US Dollars", "$", "USD"), false);

        /// <summary>
        /// USA currency representing 1/100 of <see cref="Usd"/>.
        /// </summary>
        public static readonly Currency UsdPenny = new(new UnitNaming("US Penny", "US Pennies", "¢", "USD Penny", "USD Pennies"), Usd * 1E-2, false);
    }
}
