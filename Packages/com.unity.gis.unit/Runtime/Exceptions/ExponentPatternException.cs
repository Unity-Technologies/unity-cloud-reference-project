
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when a string has multiple <see cref="IUnit.Power"/> values.
    /// </summary>
    /// <example>2'²5"³</example>
    [Serializable]
    public class ExponentPatternException : ArgumentException, IPatternException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentPatternException"/> class with parameter value
        /// and the parameter name that are the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="paramValue">The invalid value that caused the exception to be thrown.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public ExponentPatternException(string paramValue, string paramName) :
            base(CreateMessage(paramValue), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentPatternException"/> class with parameter value,
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
        public ExponentPatternException(string paramValue, string paramName, Exception innerException) :
            base(CreateMessage(paramValue), paramName, innerException) { }


        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="paramValue">The text causing this error.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(string paramValue) =>
            $"UnitValue cannot have multiple exponent values: {paramValue}";


        /// <summary>
        /// Create an instance of <see cref="ExponentPatternException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected ExponentPatternException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
