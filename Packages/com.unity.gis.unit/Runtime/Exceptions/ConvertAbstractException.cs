
using System;
using System.Runtime.Serialization;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Represents errors that occur when converting from or to a <see cref="IUnitDef"/> with an
    /// <see cref="IUnitDef.IsAbstractUnit"/> set to true and no given scale value.
    /// Use <see cref="IUnit.To(IUnitDef, IFormula)"/> instead when this happen.
    /// </summary>
    [Serializable]
    public class ConvertAbstractException : ArgumentException, IUnitException
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with only a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ConvertAbstractException(string message) :
            base(message) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with a specified error
        /// message and the parameter name that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public ConvertAbstractException(string message, string paramName) :
            base(message, paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public ConvertAbstractException(string message, Exception innerException) :
            base(message, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with a specified error message,
        /// the parameter name, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a catch block that handles the inner exception.
        /// </param>
        public ConvertAbstractException(string message, string paramName, Exception innerException) :
            base(message, paramName, innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class and generate a message
        /// based on the given parameters.
        /// </summary>
        /// <param name="convertFrom">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="convertTo">The right side <see cref="IUnit"/> of the operation</param>
        public ConvertAbstractException(IUnitDef convertFrom, IUnitDef convertTo) :
            this(CreateMessage(convertFrom, convertTo)) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with the parameter name
        /// that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="convertFrom">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="convertTo">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        public ConvertAbstractException(IUnitDef convertFrom, IUnitDef convertTo, string paramName) :
            this(CreateMessage(convertFrom, convertTo), paramName) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with a reference
        /// to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="convertFrom">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="convertTo">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public ConvertAbstractException(IUnitDef convertFrom, IUnitDef convertTo, Exception innerException) :
            this(CreateMessage(convertFrom, convertTo), innerException) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertAbstractException"/> class with the parameter name
        /// and a reference to the inner exception that is the cause of this exception.
        /// The message will be generated based on the given parameters.
        /// </summary>
        /// <param name="convertFrom">The left side <see cref="IUnit"/> of the operation.</param>
        /// <param name="convertTo">The right side <see cref="IUnit"/> of the operation</param>
        /// <param name="paramName">The name of the parameter that caused the current exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the <paramref name="innerException"/> parameter is not a null reference, the current exception is raised
        /// in a <see langword="catch"/> block that handles the inner exception.
        /// </param>
        public ConvertAbstractException(IUnitDef convertFrom, IUnitDef convertTo, string paramName, Exception innerException) :
            this(CreateMessage(convertFrom, convertTo), paramName, innerException) { }

        
        /// <summary>
        /// Create a preformatted message based on the usual parameters. 
        /// </summary>
        /// <param name="convertFrom">Convert from this unit definition.</param>
        /// <param name="convertTo">Convert to this unit definition.</param>
        /// <returns>The preformatted string.</returns>
        private static string CreateMessage(IUnitDef convertFrom, IUnitDef convertTo)
        {
            IUnitDef abstractUnit;
            if (convertFrom.IsAbstractUnit)
                abstractUnit = convertFrom;
            else
                abstractUnit = convertTo.IsAbstractUnit
                    ? convertTo
                    : null;

            return abstractUnit is null
                ? $"Cannot convert from the Abstract unit {convertFrom} to the other Abstract unit {convertTo}. Only one unit can be abstract."
                : $"Cannot convert unit from {convertFrom} to {convertTo} since {abstractUnit} is abstract and needs a conversion formula. Use IUnit.To(IUnitDef, Formula) instead.";
        }


        /// <summary>
        /// Create an instance of <see cref="ConvertAbstractException"/> from a deserialization.
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected ConvertAbstractException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
