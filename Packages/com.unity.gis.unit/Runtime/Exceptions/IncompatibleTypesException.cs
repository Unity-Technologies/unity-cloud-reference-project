
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when an operation is executed between two <see cref="IUnitDef"/> of different types
    /// and they are not <see cref="IUnit.Power"/> siblings.
    /// </summary>
    /// <seealso cref="Mapping.GetPowerUnitDef"/>
    [Serializable]
    public class IncompatibleTypesException : ArgumentException, IUnitException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleTypesException"/> class and generate a message
        /// based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        public IncompatibleTypesException(IUnitDef first, IUnitDef second) :
            base(CreateMessage(first, second)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleTypesException"/> class with the parameter name
        /// that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="first">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="second">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public IncompatibleTypesException(IUnitDef first, IUnitDef second, string paramName) :
            base(CreateMessage(first, second), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleTypesException"/> class with a reference
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
        public IncompatibleTypesException(IUnitDef first, IUnitDef second, Exception innerException) :
            base(CreateMessage(first, second), innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleTypesException"/> class with the parameter name
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
        public IncompatibleTypesException(IUnitDef first, IUnitDef second, string paramName, Exception innerException) :
            base(CreateMessage(first, second), paramName, innerException) { }


        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="first"><see cref="IUnitDef"/> instance requested to be converted.</param>
        /// <param name="second">Tried to convert <paramref name="first"/> to this <see cref="IUnitDef"/>.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(IUnitDef first, IUnitDef second) =>
            $"Cannot convert {first.Name} <{first.GetType()}> to {second.Name} <{second.GetType()}> since they are not compatible types.";


        /// <summary>
        /// Create an instance of <see cref="IncompatibleTypesException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected IncompatibleTypesException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
