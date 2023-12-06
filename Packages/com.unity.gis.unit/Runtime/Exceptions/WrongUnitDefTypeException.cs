
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when an execution require a specific <see cref="UnitDef{T}"/> and an
    /// incompatible instance was given.
    /// </summary>
    [Serializable]
    public class WrongUnitDefTypeException : InvalidOperationException, IUnitException
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongUnitDefTypeException"/> class with only a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public WrongUnitDefTypeException(string message) :
            base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongUnitDefTypeException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public WrongUnitDefTypeException(string message, Exception innerException) :
            base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongUnitDefTypeException"/> class and generate a message
        /// based on the given parameters.
        /// </summary>
        /// <param name="expected">Expected unit definition type.</param>
        /// <param name="given">Unit definition raising the error.</param>
        public WrongUnitDefTypeException(Type expected, IUnitDef given) :
            this(CreateMessage(expected, given)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongUnitDefTypeException"/> class with a reference
        /// to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="expected">Expected unit definition type.</param>
        /// <param name="given">Unit definition raising the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public WrongUnitDefTypeException(Type expected, IUnitDef given, Exception innerException) :
            this(CreateMessage(expected, given), innerException) { }

        
        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="expected">Expected unit definition type.</param>
        /// <param name="given">Unit definition raising the error.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(Type expected, IUnitDef given) => 
            $"{given} is of type {given.GetType()} and should be of type {expected}.";


        /// <summary>
        /// Create an instance of <see cref="WrongUnitDefTypeException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected WrongUnitDefTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
