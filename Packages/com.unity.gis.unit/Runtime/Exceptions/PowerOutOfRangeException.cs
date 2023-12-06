
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when the <see cref="IUnit.Power"/> value is either
    /// lower than <see cref="IUnit"/>.<see cref="IUnit.UnitDef"/>.<see cref="IUnitDef.PowerMin"/>
    /// or higher than <see cref="IUnit"/>.<see cref="IUnit.UnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
    /// </summary>
    [Serializable]
    public class PowerOutOfRangeException : ArgumentOutOfRangeException, IUnitException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with only a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public PowerOutOfRangeException(string message) :
            base(message) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with a specified error
        /// message and the parameter name that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public PowerOutOfRangeException(string message, string paramName) :
            base(paramName, message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with a specified error
        /// message, the <paramref name="paramName"/> value and the parameter name that is the cause of this exception.
        /// </summary>
        /// <param name="value">The invalid value that caused the exception to be thrown.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public PowerOutOfRangeException(byte value, string message, string paramName) :
            base(paramName, value, message) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public PowerOutOfRangeException(string message, Exception innerException) :
            base(message, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with the parameter value
        /// and the <see cref="IUnitDef"/> that are the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="value">The invalid value that caused the exception to be thrown.</param>
        /// <param name="unitDef">The <see cref="IUnitDef"/> that specify the <see cref="IUnit.Power"/> range restriction.</param>
        public PowerOutOfRangeException(byte value, IUnitDef unitDef) :
            base(CreateMessage(value, unitDef)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with the parameter value,
        /// the <see cref="IUnitDef"/> and the parameter name that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="value">The invalid value that caused the exception to be thrown.</param>
        /// <param name="unitDef">The <see cref="IUnitDef"/> that specify the <see cref="IUnit.Power"/> range restriction.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public PowerOutOfRangeException(byte value, IUnitDef unitDef, string paramName) :
            base(paramName, value, CreateMessage(value, unitDef)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOutOfRangeException"/> class with the parameter value,
        /// the <see cref="IUnitDef"/> and a reference to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="value">The invalid value that caused the exception to be thrown.</param>
        /// <param name="unitDef">The <see cref="IUnitDef"/> that specify the <see cref="IUnit.Power"/> range restriction.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public PowerOutOfRangeException(byte value, IUnitDef unitDef, Exception innerException) :
            base(CreateMessage(value, unitDef), innerException) { }


        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="value">The <see cref="IUnit.Value"/> that is outside of the permitted range.</param>
        /// <param name="unitDef"><see cref="IUnitDef"/> instance flagging the issue based on its range.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(byte value, IUnitDef unitDef) =>
            value < unitDef.PowerMin 
                ? $"{unitDef.Name} power value cannot be set to {value} since the lowest permitted value is {unitDef.PowerMin}" 
                : $"{unitDef.Name} power value cannot be set to {value} since the highest permitted value is {unitDef.PowerMax}";


        /// <summary>
        /// Create an instance of <see cref="PowerOutOfRangeException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected PowerOutOfRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
