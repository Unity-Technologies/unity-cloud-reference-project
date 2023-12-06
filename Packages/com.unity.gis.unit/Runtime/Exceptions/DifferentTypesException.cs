
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when an operation is executed between two <see cref="IUnitDef"/> of different types.
    /// </summary>
    [Serializable]
    public class DifferentTypesException : ArgumentException, IUnitException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DifferentTypesException(string message) :
            base(message) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with a specified error message
        /// and the parameter name that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public DifferentTypesException(string message, string paramName) :
            base(message, paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public DifferentTypesException(string message, Exception innerException) :
            base(message, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with a specified error message,
        /// the parameter name, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public DifferentTypesException(string message, string paramName, Exception innerException) :
            base(message, paramName, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class and generate a message
        /// based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        public DifferentTypesException(IUnitDef first, IUnitDef second) :
            this(CreateMessage(first, second)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with the parameter name
        /// that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public DifferentTypesException(IUnitDef first, IUnitDef second, string paramName) :
            this(CreateMessage(first, second), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with
        /// a reference to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public DifferentTypesException(IUnitDef first, IUnitDef second, Exception innerException) :
            this(CreateMessage(first, second), innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentTypesException"/> class with the parameter name
        /// and a reference to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public DifferentTypesException(IUnitDef first, IUnitDef second, string paramName, Exception innerException) :
            this(CreateMessage(first, second), paramName, innerException) { }

        
        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="first"><see cref="IUnitDef"/> instance requested to be converted.</param>
        /// <param name="second">Tried to convert <paramref name="first"/> to this <see cref="IUnitDef"/>.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(IUnitDef first, IUnitDef second) =>
            $"Cannot convert {first.Name} <{first.GetType()}> to {second.Name} <{second.GetType()}> since they are not of the same type.";

        
        /// <summary>
        /// Create an instance of <see cref="DifferentTypesException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected DifferentTypesException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
