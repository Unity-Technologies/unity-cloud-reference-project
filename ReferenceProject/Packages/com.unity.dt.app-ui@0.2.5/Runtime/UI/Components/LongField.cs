using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A <see cref="NumericalField{T}"/> that only accepts long values.
    /// </summary>
    public class LongField : NumericalField<long>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LongField()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out long val)
        {
            var ret = UINumericFieldsUtils.StringToLong(strValue, out var v);
            val = ret ? v : value;
            return ret;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseValueToString"/>
        protected override string ParseValueToString(long val)
        {
            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.AreEqual"/>
        protected override bool AreEqual(long a, long b)
        {
            return a == b;
        }

        /// <inheritdoc cref="NumericalField{T}.Min(T,T)"/>
        protected override long Min(long a, long b)
        {
            return Math.Min(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Max(T,T)"/>
        protected override long Max(long a, long b)
        {
            return Math.Max(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Increment"/>
        protected override long Increment(long originalValue, float delta)
        {
            return originalValue + (Mathf.Approximately(0, delta) ? 0 : Math.Sign(delta));
        }

        /// <inheritdoc cref="NumericalField{T}.GetIncrementFactor"/>
        protected override float GetIncrementFactor(long baseValue)
        {
            return Math.Abs(baseValue) > 100 ? (float)Math.Ceiling(baseValue * 0.1f) : 1;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="LongField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<LongField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="LongField"/>.
        /// </summary>
        public new class UxmlTraits : NumericalField<long>.UxmlTraits { }
    }
}
