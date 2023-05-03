using System;
using System.Runtime.Serialization;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Exception thrown when a Visual Element hasn't been found.
    /// </summary>
    [Serializable]
    public sealed class VisualElementNotFoundException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="message">THe exception message.</param>
        public VisualElementNotFoundException(string message) : base(message) { }

        VisualElementNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
