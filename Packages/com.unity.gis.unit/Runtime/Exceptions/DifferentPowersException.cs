
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when an operation is executed between two <see cref="IUnit"/> with different <see cref="IUnit"/>.<see cref="IUnit.Power"/> value.
    /// </summary>
    [Serializable]
    public class DifferentPowersException : ArgumentException, IUnitException
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with only a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DifferentPowersException(string message) :
            base(message) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with a specified error
        /// message and the parameter name that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public DifferentPowersException(string message, string paramName) :
            base(message, paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public DifferentPowersException(string message, Exception innerException) :
            base(message, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with a specified error message,
        /// the parameter name, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public DifferentPowersException(string message, string paramName, Exception innerException) :
            base(message, paramName, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class and generate a message
        /// based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        public DifferentPowersException(IUnit first, IUnit second) :
            this(CreateMessage(first, second)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with the parameter name
        /// that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public DifferentPowersException(IUnit first, IUnit second, string paramName) :
            this(CreateMessage(first, second), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with a reference
        /// to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public DifferentPowersException(IUnit first, IUnit second, Exception innerException) :
            this(CreateMessage(first, second), innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentPowersException"/> class with the parameter name
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
        public DifferentPowersException(IUnit first, IUnit second, string paramName, Exception innerException) :
            this(CreateMessage(first, second), paramName, innerException) { }

        
        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(IUnit first, IUnit second) =>
            $"Cannot execute operator from {first} to {second} since they do not have the same power value.";


        /// <summary>
        /// Create an instance of <see cref="DifferentPowersException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected DifferentPowersException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
