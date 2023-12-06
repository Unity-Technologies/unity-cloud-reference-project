
namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur during execution within UnityGeospatial.Unit namespace.
    /// </summary>
    public interface IUnitException { }
    
    /// <summary>
    /// Represents errors where <see langword="string"/> decoding fail when the content doesn't match the expected pattern.
    /// </summary>
    public interface IPatternException : IUnitException { }
}
