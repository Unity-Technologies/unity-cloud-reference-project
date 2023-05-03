using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The DPI has changed.
    /// </summary>
    public class DpiChangedEvent : EventBase<DpiChangedEvent>
    {
        /// <summary>
        /// The previous value.
        /// </summary>
        public float previousValue { get; set; }

        /// <summary>
        /// The new value.
        /// </summary>
        public float newValue { get; set; }
    }
}
