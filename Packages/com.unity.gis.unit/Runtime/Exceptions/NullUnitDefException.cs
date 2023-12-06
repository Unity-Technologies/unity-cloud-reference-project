using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors where an operation requires a Unit with a <see cref="IUnitDef"/>
    /// and the condition was not met.
    /// </summary>
    [Serializable]
    public class NullUnitDefException : NullReferenceException, IUnitException
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="NullUnitDefException"/>
        /// </summary>
        /// <param name="message">An error message to explain the reason for the exception</param>
        public NullUnitDefException(string message)
            : base(message) { }


        /// <summary>
        /// Create an instance of <see cref="DoublePatternException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected NullUnitDefException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
