
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when the requested <see cref="IUnitDef"/> cannot be found in a string.
    /// </summary>
    [Serializable]
    public class UnitPatternException : ArgumentException, IPatternException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitPatternException"/> class with parameter value
        /// and the parameter name that are the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="paramValue">The invalid value that caused the exception to be thrown.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public UnitPatternException(string paramValue, string paramName) :
            base(CreateMessage(paramValue), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitPatternException"/> class with parameter value,
        /// the parameter name, and a reference to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="paramValue">The invalid value that caused the exception to be thrown.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public UnitPatternException(string paramValue, string paramName, Exception innerException) :
            base(CreateMessage(paramValue), paramName, innerException) { }


        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="paramValue">The text causing the error.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(string paramValue) =>
            $"{paramValue} as no valid unit pattern (\\d UNIT).";


        /// <summary>
        /// Create an instance of <see cref="UnitPatternException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected UnitPatternException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
