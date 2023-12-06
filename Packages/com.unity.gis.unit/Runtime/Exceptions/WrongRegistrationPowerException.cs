
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when registering a <see cref="IUnit.Power"/> value with <see cref="Mapping.RegisterPowerUnitDef"/>
    /// lower than <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMin"/>
    /// or higher than <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
    /// </summary>
    [Serializable]
    public class WrongRegistrationPowerException : ArgumentException, IUnitException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongRegistrationPowerException"/> class and generate a message
        /// based on the given parameters.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="unitDef">The <see cref="IUnitDef"/> requested to be registered.</param>
        /// <param name="registeredPower">Tried to register the <see cref="IUnitDef"/> under this <see cref="IUnit.Power"/> value.</param>
        public WrongRegistrationPowerException(IUnitDef unitDef, int registeredPower) :
            base(CreateMessage(unitDef, registeredPower)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongRegistrationPowerException"/> class with the parameter name
        /// that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="unitDef">The <see cref="IUnitDef"/> requested to be registered.</param>
        /// <param name="registeredPower">Tried to register the <see cref="IUnitDef"/> under this <see cref="IUnit.Power"/> value.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public WrongRegistrationPowerException(IUnitDef unitDef, int registeredPower, string paramName) :
            base(CreateMessage(unitDef, registeredPower), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongRegistrationPowerException"/> class with a reference
        /// to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="unitDef">The <see cref="IUnitDef"/> requested to be registered.</param>
        /// <param name="registeredPower">Tried to register the <see cref="IUnitDef"/> under this <see cref="IUnit.Power"/> value.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public WrongRegistrationPowerException(IUnitDef unitDef, int registeredPower, Exception innerException) :
            base(CreateMessage(unitDef, registeredPower), innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongRegistrationPowerException"/> class with the parameter name
        /// and a reference to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="unitDef">The <see cref="IUnitDef"/> requested to be registered.</param>
        /// <param name="registeredPower">Tried to register the <see cref="IUnitDef"/> under this <see cref="IUnit.Power"/> value.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public WrongRegistrationPowerException(IUnitDef unitDef, int registeredPower, string paramName, Exception innerException) :
            base(CreateMessage(unitDef, registeredPower), paramName, innerException) { }


        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="unitDef"><see cref="IUnitDef"/> causing the error.</param>
        /// <param name="registeredPower">Invalid <see cref="IUnit.Power"/> registered value.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(IUnitDef unitDef, int registeredPower) =>
            $"Cannot register {unitDef} power under {registeredPower} since it's outside its power range of {unitDef.PowerMin}-{unitDef.PowerMax}.";


        /// <summary>
        /// Create an instance of <see cref="WrongRegistrationPowerException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected WrongRegistrationPowerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
