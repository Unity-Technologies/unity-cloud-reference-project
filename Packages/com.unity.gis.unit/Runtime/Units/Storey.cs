
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Abstract and predefined building storey units.
    /// </summary>
    public static class Storey
    {
        /// <summary>
        /// Represent an abstract building storey height is unknown.
        /// </summary>
        public static readonly Length Abstract = new(new UnitNaming("storey", " stry", "level", "lvl"), true);

        /// <summary>
        /// Minimum storey height for the average buildings in Australia.
        /// </summary>
        public static readonly Length AustraliaMin = new(new UnitNaming(" storey", " uk stry", "uk level", "uk lvl"), Si.Meter * 2.4, false);

        /// <summary>
        /// Minimum storey height for the average buildings in South Africa.
        /// </summary>
        public static readonly Length SouthAfricaMin = new(new UnitNaming(" storey", " uk stry", "uk level", "uk lvl"), Si.Meter * 2.4, false);

        /// <summary>
        /// Average storey height for the buildings in the United States of America.
        /// </summary>
        public static readonly Length UsaAvg = new(new UnitNaming(" storey", " usa stry", "us stry", "usa level", "us level", "usa lvl", "us lvl"), Imperial.Feet * 10.0, false);

        /// <summary>
        /// Minimum storey height for the average buildings in the United Kingdom.
        /// </summary>
        public static readonly Length UkMin = new(new UnitNaming(" storey", " uk stry", "uk level", "uk lvl"), Si.Meter * 2.4, false);
    }
}
