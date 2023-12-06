
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// United States customary units.
    /// </summary>
    public static class Uscs
    {

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 16 <see cref="Pint"/>.
        /// </summary>
        public static readonly Volume Gallon = new(new UnitNaming("US gallon", " us gal", "usa gallon", " usa gal", "uscs gallon", " uscs gal", "usc gallon", " usc gal"), Si.Liter / 0.264_172_052, true, false);

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 1/4 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Quart = new(new UnitNaming("US quart", " us qt", "usa quart", " usa qt", "uscs quart", " uscs qt", "usc quart", " usc qt"), Gallon / 4.0, true, false);

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 1/8 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Pint = new(new UnitNaming("US pint", " us pt", "usa pint", " usa pt", "uscs pint", " uscs pt", "usc pint", " usc pt"), Quart / 2.0, true, false);

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 1/16 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Cup = new(new UnitNaming("US cup", " us cup", "usa cup", "uscs cup", "usc cup"), Pint / 2.0, true, false);

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 1/16 <see cref="Pint"/>.
        /// </summary>
        public static readonly Volume FluidOunce = new(new UnitNaming("US fluid ounce", " us foz", "usa fluid ounce", " usa foz", "usa founce", "us founce", "uscs fluid ounce", " uscs foz", "uscs founce", "usc fluid ounce", " usc foz", "usc founce"), Cup / 8.0, true, false);

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 1/32 <see cref="Pint"/>.
        /// </summary>
        public static readonly Volume Tablespoon = new(new UnitNaming("US tablespoon", " us tbsp", "usa tablespoon", " usa tbsp", "uscs tablespoon", " uscs tbsp", "usc tablespoon", " usc tbsp"), FluidOunce / 2.0, true, false);

        /// <summary>
        /// United States customary unit of a three-dimensional space equivalent to 1/3 <see cref="Tablespoon"/>.
        /// </summary>
        public static readonly Volume Teaspoon = new(new UnitNaming("US teaspoon", " us tsp", "usa teaspoon", " usa tsp", "uscs teaspoon", " uscs tsp", "usc teaspoon", " usc tsp"), Tablespoon / 3.0, true, false);



        /// <summary>
        /// United States customary measure of resistance to acceleration when a net force is applied equivalent to 2000 <see cref="Imperial.Pound"/>.
        /// </summary>
        public static readonly Mass ShortTon = new(new UnitNaming("US ton", " us t", "usa ton", " usa t", "shortton", "uscs ton", "uscs t", "usc ton", "usc t"), Imperial.Pound * 2000.0, false);

    }
}
