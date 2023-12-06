
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// System of units first defined in the British Weights and Measures Act 1824 and continued with a series of amendments.
    /// </summary>
    public static class Imperial
    {
        static Imperial()
        {
            Length.RegisterPowerUnits(Inch, Inch2, Inch3);
            Length.RegisterPowerUnits(Feet, Feet2, Feet3);
            Length.RegisterPowerUnits(Yard, Yard2, Yard3);
            Length.RegisterPowerUnits(Mile, Mile2, Mile3);
            Length.RegisterPowerUnits(Thou, Thou2, Thou3);
            Length.RegisterPowerUnits(Furlong, Furlong2, Furlong3);
            Length.RegisterPowerUnits(NauticalMile, NauticalMile2, NauticalMile3);
        }


        /// <summary>
        /// British imperial unit of length equivalent to 1/12 of a foot (<see cref="Feet"/>).
        /// </summary>
        public static readonly Length Inch = new(new UnitNaming("inch", "inches", "in", "\""), Si.Centimeter * 127 / 50, false); // 2.54 = 127 / 50; will prevent precision errors.

        /// <summary>
        /// British imperial unit of length equivalent to 12 <see cref="Inch"/>.
        /// </summary>
        public static readonly Length Feet = new(new UnitNaming("foot", "feet", "ft", "'"), Inch * 12, false);

        /// <summary>
        /// British imperial unit of length equivalent to 3 <see cref="Feet"/>.
        /// </summary>
        public static readonly Length Yard = new(new UnitNaming("yard", " yd"), Feet * 3);

        /// <summary>
        /// British imperial unit of length equivalent to 1760 <see cref="Yard"/>.
        /// </summary>
        public static readonly Length Mile = new(new UnitNaming("mile", " mi"), Feet * 5280);

        /// <summary>
        /// British imperial unit of length equivalent to 1/1000 <see cref="Inch"/>.
        /// </summary>
        public static readonly Length Thou = new(new UnitNaming("thou", " mil"), Inch * 0.001);

        /// <summary>
        /// British imperial unit of length equivalent to 220 <see cref="Yard"/>.
        /// </summary>
        public static readonly Length Furlong = new(new UnitNaming("furlong", " fur"), Feet * 660);

        /// <summary>
        /// British imperial unit of length equivalent to 6076 <see cref="Feet"/> and 1.5 <see cref="Inch"/>.
        /// </summary>
        public static readonly Length NauticalMile = new(new UnitNaming("nautical mile", " NM", "nmi"), Si.Meter * 1852, false);
        
        

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square <see cref="Inch"/>.
        /// </summary>
        public static readonly Area Inch2 = new(Inch.Naming, Inch.Pow(2), true, false);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square foot (<see cref="Feet"/>).
        /// </summary>
        public static readonly Area Feet2 = new(Feet.Naming, Feet.Pow(2), true, false);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to 43560 square <see cref="Feet"/>.
        /// </summary>
        public static readonly Area Acre = new(new UnitNaming("acre", "ac"), Feet2 * 43_560, true);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square <see cref="Yard"/>.
        /// </summary>
        public static readonly Area Yard2 = new(Yard.Naming, Yard.Pow(2), true);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square <see cref="Mile"/>.
        /// </summary>
        public static readonly Area Mile2 = new(Mile.Naming, Mile.Pow(2), true);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square <see cref="Thou"/>.
        /// </summary>
        public static readonly Area Thou2 = new(Thou.Naming, Thou.Pow(2), true);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square <see cref="Furlong"/>.
        /// </summary>
        public static readonly Area Furlong2 = new(Furlong.Naming, Furlong.Pow(2), true);

        /// <summary>
        /// British imperial unit of a two-dimensional region equivalent to a square <see cref="NauticalMile"/>.
        /// </summary>
        public static readonly Area NauticalMile2 = new(NauticalMile.Naming, NauticalMile.Pow(2), true, false);
        
        

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 16 <see cref="Pint"/>.
        /// </summary>
        public static readonly Volume Gallon = new(new UnitNaming("gallon", " gal"), Si.Liter / 0.219_969_157_380_95, true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 1/4 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Quart = new(new UnitNaming("quart", " qt"), Gallon / 4.0, true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 1/8 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Pint = new(new UnitNaming("pint", " pt"), Quart / 2.0, true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 1/16 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Cup = new(new UnitNaming("cup", " cup"), Pint / 2.0, true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 1/20 <see cref="Pint"/>.
        /// </summary>
        public static readonly Volume FluidOunce = new(new UnitNaming("fluid ounce", " foz", "founce"), Pint / 20.0, true, false);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 1/512 <see cref="Gallon"/>.
        /// </summary>
        public static readonly Volume Tablespoon = new(new UnitNaming("tablespoon", " tbsp"), FluidOunce / 1.6, true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to 1/3 <see cref="Tablespoon"/>.
        /// </summary>
        public static readonly Volume Teaspoon = new(new UnitNaming("teaspoon", " tsp"), Tablespoon / 3.0, true);
        

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic <see cref="Inch"/>.
        /// </summary>
        public static readonly Volume Inch3 = new(Inch.Naming, Inch.Pow(3), true, false);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic foot (<see cref="Feet"/>).
        /// </summary>
        public static readonly Volume Feet3 = new(Feet.Naming, Feet.Pow(3), true, false);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic <see cref="Yard"/>.
        /// </summary>
        public static readonly Volume Yard3 = new(Yard.Naming, Yard.Pow(3), true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic <see cref="Mile"/>.
        /// </summary>
        public static readonly Volume Mile3 = new(Mile.Naming, Mile.Pow(3), true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic <see cref="Thou"/>.
        /// </summary>
        public static readonly Volume Thou3 = new(Thou.Naming, Thou.Pow(3), true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic <see cref="Furlong"/>.
        /// </summary>
        public static readonly Volume Furlong3 = new(Furlong.Naming, Furlong.Pow(3), true);

        /// <summary>
        /// British imperial unit of a three-dimensional space equivalent to a cubic <see cref="NauticalMile"/>.
        /// </summary>
        public static readonly Volume NauticalMile3 = new(NauticalMile.Naming, NauticalMile.Pow(3), true, false);
        

        /// <summary>
        /// British imperial measure of resistance to acceleration when a net force is applied equivalent to 16 <see cref="Pound"/>.
        /// </summary>
        public static readonly Mass Ounce = new(new UnitNaming("ounce", " oz", "uk ounce", "uk oz"), Si.Gram * 28.349_523_125, false);

        /// <summary>
        /// British imperial measure of resistance to acceleration when a net force is applied equivalent to 1/16 <see cref="Ounce"/>.
        /// </summary>
        public static readonly Mass Pound = new(new UnitNaming("pound", " lb", "uk pound", "uk lb"), Ounce * 16.0, false);

        /// <summary>
        /// British imperial measure of resistance to acceleration when a net force is applied equivalent to 1/224 <see cref="Ounce"/>.
        /// </summary>
        public static readonly Mass Stone = new(new UnitNaming("stone", " st", "uk stone", "uk st"), Pound * 14.0, false);

        /// <summary>
        /// British imperial measure of resistance to acceleration when a net force is applied equivalent to 1/35,840 <see cref="Ounce"/>.
        /// </summary>
        public static readonly Mass LongTon = new(new UnitNaming("ton", " t", "longton", "uk ton", "uk longton", "uk t"), Pound * 2240.0, false);

        /// <summary>
        /// British imperial measure of resistance to acceleration when a net force is applied equivalent to 14.59390 <see cref="Si.Kilogram"/>.
        /// </summary>
        public static readonly Mass Slug = new(new UnitNaming("slug", " slug", "uk slug"), Pound * 32.174_049, false);

        

        /// <summary>
        /// British imperial unit expressing hot and cold.
        /// </summary>
        public static readonly Temperature Fahrenheit = new(new UnitNaming("fahrenheit", "°F"), Si.Celsius * (5.0/9.0), 32);
        
    }
}
