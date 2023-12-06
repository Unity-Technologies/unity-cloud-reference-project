
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when <see cref="IUnitDef"/>.<see cref="IUnitDef.BaseUnitDef"/> value is set twice.
    /// This value cannot be overwritten.
    /// </summary>
    [Serializable]
    public class MultiBaseUnitException : ArgumentException, IUnitException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiBaseUnitException"/> class and generate a message
        /// based on the given parameters.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="unitType">The <see cref="IUnitDef"/> type with an already registered <see cref="IUnitDef.BaseUnitDef"/>.</param>
        /// <param name="current">The <see cref="IUnitDef"/> instance that is already set as the <see cref="IUnitDef.BaseUnitDef"/> for <paramref name="unitType"/>.</param>
        /// <param name="newValue">The <see cref="IUnitDef"/> instance causing the issue.</param>
        public MultiBaseUnitException(Type unitType, IUnitDef current, IUnitDef newValue) :
            base(CreateMessage(unitType, current, newValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiBaseUnitException"/> class with the inner exception
        /// that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="unitType">The <see cref="IUnitDef"/> type with an already registered <see cref="IUnitDef.BaseUnitDef"/>.</param>
        /// <param name="current">The <see cref="IUnitDef"/> instance that is already set as the <see cref="IUnitDef.BaseUnitDef"/> for <paramref name="unitType"/>.</param>
        /// <param name="newValue">The <see cref="IUnitDef"/> instance causing the issue.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public MultiBaseUnitException(Type unitType, IUnitDef current, IUnitDef newValue, Exception innerException) :
            base(CreateMessage(unitType, current, newValue), innerException) { }


        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="unitType"><see cref="IUnitDef"/> type with an already registered <see cref="IUnitDef.BaseUnitDef"/>.</param>
        /// <param name="current">The <see cref="IUnitDef"/> instance that is currently set as the <see cref="IUnitDef.BaseUnitDef"/>.</param>
        /// <param name="newValue">The second <see cref="IUnitDef"/> instance to be set as the <see cref="IUnitDef.BaseUnitDef"/>.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(Type unitType, IUnitDef current, IUnitDef newValue) =>
            $"BaseUnit is already set for the type {unitType.Name} to {current}. It cannot be set to {newValue}.";


        /// <summary>
        /// Create an instance of <see cref="MultiBaseUnitException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected MultiBaseUnitException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
