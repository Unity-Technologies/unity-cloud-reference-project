using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A <see cref="VisualElement"/> that displays a numeric value and allows the user to edit it.
    /// </summary>
    public class IntField : NumericalField<int>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IntField()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out int val)
        {
            var ret = UINumericFieldsUtils.StringToLong(strValue, out var v);
            val = ret ? UINumericFieldsUtils.ClampToInt(v) : value;
            return ret;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseValueToString"/>
        protected override string ParseValueToString(int val)
        {
            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.AreEqual"/>
        protected override bool AreEqual(int a, int b)
        {
            return a == b;
        }

        /// <inheritdoc cref="NumericalField{T}.Min(T,T)"/>
        protected override int Min(int a, int b)
        {
            return Math.Min(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Max(T,T)"/>
        protected override int Max(int a, int b)
        {
            return Math.Max(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Increment"/>
        protected override int Increment(int originalValue, float delta)
        {
            return originalValue + (Mathf.Approximately(0, delta) ? 0 : Math.Sign(delta));
        }

        /// <inheritdoc cref="NumericalField{T}.GetIncrementFactor"/>
        protected override float GetIncrementFactor(int baseValue)
        {
            return Mathf.Abs(baseValue) > 100 ? Mathf.CeilToInt(baseValue * 0.1f) : 1;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="IntField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<IntField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="IntField"/>.
        /// </summary>
        public new class UxmlTraits : NumericalField<int>.UxmlTraits { }
    }
}
