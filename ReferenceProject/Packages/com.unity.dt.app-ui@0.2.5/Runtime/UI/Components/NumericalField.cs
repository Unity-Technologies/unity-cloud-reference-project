using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Numerical Field UI element.
    /// </summary>
    public abstract class NumericalField<TValueType> : ExVisualElement, IValidatableElement<TValueType>, ISizeableElement
        where TValueType : struct, IComparable, IComparable<TValueType>, IFormattable
    {
        /// <summary>
        /// The NumericalField main styling class.
        /// </summary>
        public const string ussClassName = "appui-numericalfield";

        /// <summary>
        /// The NumericalField input container styling class.
        /// </summary>
        public static readonly string inputContainerUssClassName = ussClassName + "__inputcontainer";

        /// <summary>
        /// The NumericalField input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The NumericalField unit styling class.
        /// </summary>
        public static readonly string unitUssClassName = ussClassName + "__unit";

        /// <summary>
        /// The NumericalField trailing container styling class.
        /// </summary>
        public static readonly string trailingContainerUssClassName = ussClassName + "__trailingcontainer";

        /// <summary>
        /// The NumericalField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The input container.
        /// </summary>
        protected readonly VisualElement m_InputContainer;

        /// <summary>
        /// The input element.
        /// </summary>
        protected readonly UIElements.TextField m_InputElement;

        /// <summary>
        /// The size of the element.
        /// </summary>
        protected Size m_Size;

        /// <summary>
        /// The trailing container.
        /// </summary>
        protected readonly VisualElement m_TrailingContainer;

        /// <summary>
        /// The unit element.
        /// </summary>
        protected readonly LocalizedTextElement m_UnitElement;

        /// <summary>
        /// The value of the element.
        /// </summary>
        protected TValueType m_Value;

        string m_FormatString;

        /// <summary>
        /// The format string of the element.
        /// </summary>
        public string formatString
        {
            get => m_FormatString;
            set
            {
                m_FormatString = value;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected NumericalField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;
            TextField.isCompositeRootProp!.SetValue(this, true);
            TextField.excludeFromFocusRingProp!.SetValue(this, true);
            delegatesFocus = true;

            m_InputContainer = new VisualElement { name = inputContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_InputContainer.AddToClassList(inputContainerUssClassName);
            m_TrailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_TrailingContainer.AddToClassList(trailingContainerUssClassName);

            m_InputElement = new UIElements.TextField { name = inputUssClassName, pickingMode = PickingMode.Ignore };
            m_InputElement.AddToClassList(inputUssClassName);
            m_InputElement.BlinkingCursor();
            m_UnitElement = new LocalizedTextElement { name = unitUssClassName, pickingMode = PickingMode.Ignore };
            m_UnitElement.AddToClassList(unitUssClassName);

            m_InputContainer.hierarchy.Add(m_InputElement);
            m_TrailingContainer.hierarchy.Add(m_UnitElement);

            hierarchy.Add(m_InputContainer);
            hierarchy.Add(m_TrailingContainer);

            m_InputElement.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
        }

        /// <summary>
        /// The unit of the element.
        /// </summary>
        public string unit
        {
            get => m_UnitElement.text;
            set
            {
                m_UnitElement.text = value;
                m_UnitElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_UnitElement.text));
            }
        }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public TValueType? lowValue { get; set; }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public TValueType? highValue { get; set; }

        /// <summary>
        /// The content container of the element.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the element.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
            }
        }

        /// <summary>
        /// Set the value of the element without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the element. </param>
        public void SetValueWithoutNotify(TValueType newValue)
        {
            if (lowValue.HasValue)
                newValue = Max(newValue, lowValue.Value);
            if (highValue.HasValue)
                newValue = Min(newValue, highValue.Value);
            m_Value = newValue;
            var valStr = ParseValueToString(newValue);
            m_InputElement.SetValueWithoutNotify(valStr);
        }

        /// <summary>
        /// The value of the element.
        /// </summary>
        public TValueType value
        {
            get => m_Value;
            set
            {
                var val = value;
                if (lowValue.HasValue)
                    val = Max(val, lowValue.Value);
                if (highValue.HasValue)
                    val = Min(val, highValue.Value);
                if (AreEqual(m_Value, val))
                    return;

                using var evt = ChangeEvent<TValueType>.GetPooled(m_Value, val);
                evt.target = this;
                SetValueWithoutNotify(val);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the element.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// Method to validate the value.
        /// </summary>
        public Func<TValueType, bool> validateValue { get; set; }

        void OnFocusedOut(FocusOutEvent evt)
        {
            RemoveFromClassList(Styles.focusedUssClassName);
            RemoveFromClassList(Styles.keyboardFocusUssClassName);

            var val = ParseStringToValue(m_InputElement.value, out var v) ? v : value;
            if (val.Equals(value))
            {
                m_InputElement.SetValueWithoutNotify(ParseValueToString(value)); // reset previous value text if invalid text
                return;
            }

            value = val;
            SetValueWithoutNotify(val);
        }

        void OnFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            passMask = 0;
        }

        void OnKeyboardFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            AddToClassList(Styles.keyboardFocusUssClassName);
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// Define the conversion from the <see cref="string"/> value to a <typeparamref name="TValueType"/> value.
        /// </summary>
        /// <param name="strValue">The <see cref="string"/> value to convert.</param>
        /// /// <param name="val">The <typeparamref name="TValueType"/> value returned.</param>
        /// <returns>True if the conversion is possible, False otherwise.</returns>
        protected abstract bool ParseStringToValue(string strValue, out TValueType val);

        /// <summary>
        /// Define the conversion from a <typeparamref name="TValueType"/> value to a <see cref="string"/> value.
        /// </summary>
        /// <param name="val">The <typeparamref name="TValueType"/> value to convert.</param>
        /// <returns>The converted value.</returns>
        protected abstract string ParseValueToString(TValueType val);

        /// <summary>
        /// Check if two values of type <typeparamref name="TValueType"/> are equal.
        /// </summary>
        /// <param name="a">The first value to test.</param>
        /// <param name="b">The second value to test.</param>
        /// <returns>True if both values are considered equals, false otherwise.</returns>
        protected abstract bool AreEqual(TValueType a, TValueType b);

        /// <summary>
        /// Increment a given value with a given delta.
        /// </summary>
        /// <param name="originalValue">The original value.</param>
        /// <param name="delta">The delta used for increment.</param>
        protected abstract TValueType Increment(TValueType originalValue, float delta);

        /// <summary>
        /// Return the smallest value between a and b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected abstract TValueType Min(TValueType a, TValueType b);

        /// <summary>
        /// Return the biggest value between a and b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected abstract TValueType Max(TValueType a, TValueType b);

        /// <summary>
        /// Calculate the increment factor based on a base value.
        /// </summary>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        protected abstract float GetIncrementFactor(TValueType baseValue);

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="NumericalField{TValueType}"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_HighValue = new UxmlStringAttributeDescription { name = "high-value", defaultValue = null };

            readonly UxmlStringAttributeDescription m_LowValue = new UxmlStringAttributeDescription { name = "low-value", defaultValue = null };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlStringAttributeDescription m_Unit = new UxmlStringAttributeDescription
            {
                name = "unit",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value", defaultValue = "0" };

            readonly UxmlStringAttributeDescription m_Format = new UxmlStringAttributeDescription { name = "format", defaultValue = null };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (NumericalField<TValueType>)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.unit = m_Unit.GetValueFromBag(bag, cc);

                var strValue = m_Value.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(strValue) && element.ParseStringToValue(strValue, out var value))
                    element.SetValueWithoutNotify(value);

                var strLowValue = m_LowValue.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(strLowValue) && element.ParseStringToValue(strLowValue, out var lowValue))
                    element.lowValue = lowValue;

                var strHighValue = m_HighValue.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(strHighValue) && element.ParseStringToValue(strHighValue, out var highValue))
                    element.highValue = highValue;

                string formatStr = null;
                if (m_Format.TryGetValueFromBag(bag, cc, ref formatStr) && !string.IsNullOrEmpty(formatStr))
                    element.formatString = formatStr;

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
