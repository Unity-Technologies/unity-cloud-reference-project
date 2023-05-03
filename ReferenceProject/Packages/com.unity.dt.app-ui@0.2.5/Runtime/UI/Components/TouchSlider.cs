using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Base class for any Slider (<see cref="TouchSliderFloat"/>, <see cref="TouchSliderInt"/>,
    /// <see cref="SliderFloat"/>, <see cref="SliderInt"/>).
    /// </summary>
    /// <typeparam name="TValueType">A comparable value type.</typeparam>
    public abstract class BaseSlider<TValueType> : ExVisualElement, IValidatableElement<TValueType>, INotifyValueChanging<TValueType>
        where TValueType : IEquatable<TValueType>
    {
        /// <summary>
        /// The dragger manipulator used to move the slider.
        /// </summary>
        protected Draggable m_DraggerManipulator;

        /// <summary>
        /// Slider max value.
        /// </summary>
        protected TValueType m_HighValue;

        /// <summary>
        /// Slider min value.
        /// </summary>
        protected TValueType m_LowValue;

        /// <summary>
        /// The previous value of the slider before the user started interacting with it.
        /// </summary>
        protected TValueType m_PreviousValue;

        /// <summary>
        /// The current value of the slider.
        /// </summary>
        protected TValueType m_Value;

        string m_FormatString;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected BaseSlider()
        {
            passMask = Passes.Clear;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// Specify the minimum value in the range of this slider.
        /// </summary>
        public TValueType lowValue
        {
            get => m_LowValue;
            set
            {
                if (!EqualityComparer<TValueType>.Default.Equals(m_LowValue, value))
                {
                    m_LowValue = value;
                    OnSliderRangeChanged();
                }
            }
        }

        /// <summary>
        /// Specify the maximum value in the range of this slider.
        /// </summary>
        public TValueType highValue
        {
            get => m_HighValue;
            set
            {
                if (!EqualityComparer<TValueType>.Default.Equals(m_HighValue, value))
                {
                    m_HighValue = value;
                    OnSliderRangeChanged();
                }
            }
        }

        /// <summary>
        /// The format string used to display the value of the slider.
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
        /// Set the value of the slider without sending any event.
        /// </summary>
        /// <param name="newValue"> The new value of the slider.</param>
        public abstract void SetValueWithoutNotify(TValueType newValue);

        /// <summary>
        /// The current value of the slider.
        /// </summary>
        public TValueType value
        {
            get => m_Value;
            set
            {
                var newValue = GetClampedValue(value);

                if (!EqualityComparer<TValueType>.Default.Equals(m_Value, newValue))
                {
                    if (panel != null)
                    {
                        using var evt = ChangeEvent<TValueType>.GetPooled(m_Value, newValue);
                        evt.target = this;
                        SetValueWithoutNotify(newValue);
                        SendEvent(evt);
                    }
                    else
                    {
                        SetValueWithoutNotify(newValue);
                    }
                }
            }
        }

        /// <summary>
        /// The invalid state of the slider.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// The validation function used to validate the value of the slider.
        /// </summary>
        public Func<TValueType, bool> validateValue { get; set; }

        /// <summary>
        /// Called when the low or high value of Slider has changed.
        /// </summary>
        protected virtual void OnSliderRangeChanged()
        {
            ClampValue();
        }

        /// <summary>
        /// Event callback called when the geometry of the slider has changed in the layout.
        /// </summary>
        /// <param name="evt"> The geometry changed event.</param>
        protected virtual void OnGeometryChanged(GeometryChangedEvent evt)
        {
            SetValueWithoutNotify(value);
        }

        /// <summary>
        /// Event callback called when a pointer up event is received.
        /// </summary>
        /// <param name="dragger"> The dragger manipulator.</param>
        protected virtual void OnTrackUp(Draggable dragger)
        {
            if (value.Equals(m_PreviousValue))
                return;

            using var evt = ChangeEvent<TValueType>.GetPooled(m_PreviousValue, value);
            evt.target = this;
            SendEvent(evt);
        }

        /// <summary>
        /// Event callback called when a pointer down event is received.
        /// </summary>
        /// <param name="dragger"> The dragger manipulator.</param>
        protected virtual void OnTrackDown(Draggable dragger)
        {
            m_PreviousValue = value;
        }

        /// <summary>
        /// Event callback called when the dragger is dragged.
        /// </summary>
        /// <param name="dragger"> The dragger manipulator.</param>
        protected virtual void OnTrackDragged(Draggable dragger)
        {
            SetValueFromDrag(m_DraggerManipulator.localPosition.x);
        }

        /// <summary>
        /// Custom implementation of the slider value from the drag position.
        /// </summary>
        /// <param name="newPos"> The new position of the dragger.</param>
        protected virtual void SetValueFromDrag(float newPos)
        {
            var sliderRect = GetSliderRect();
            var newValue = ComputeValueDraggerPosition(sliderRect.width, newPos - sliderRect.x);
            SetValueWithoutNotify(newValue);

            using var evt = ChangingEvent<TValueType>.GetPooled();
            evt.previousValue = m_PreviousValue;
            evt.newValue = newValue;
            evt.target = this;
            SendEvent(evt);
        }

        /// <summary>
        /// Returns the rect of the interactive part of the slider.
        /// </summary>
        /// <returns> The rect of the interactive part of the slider.</returns>
        protected virtual Rect GetSliderRect() => new Rect(0, 0, contentRect.width, contentRect.height);

        /// <summary>
        /// Returns the value to set as slider value based on a given dragger position.
        /// </summary>
        /// <param name="sliderLength"> The length of the slider.</param>
        /// <param name="dragElementPos"> The position of the dragger.</param>
        /// <returns> The value to set as slider value based on a given dragger position.</returns>
        protected virtual TValueType ComputeValueDraggerPosition(float sliderLength, float dragElementPos)
        {
            if (sliderLength < Mathf.Epsilon)
                return lowValue;

            var normalizedDragElementPosition = Mathf.Max(0f, Mathf.Min(dragElementPos, sliderLength)) / sliderLength;
            return SliderLerpUnclamped(lowValue, highValue, normalizedDragElementPosition);
        }

        /// <summary>
        /// Called when the track has received a click event.
        /// <remarks>
        /// Always check if the mouse has moved using <see cref="Draggable.hasMoved"/>.
        /// </remarks>
        /// </summary>
        protected virtual void OnTrackClicked()
        {
            if (!m_DraggerManipulator.hasMoved)
            {
                OnTrackDragged(m_DraggerManipulator);
                OnTrackUp(m_DraggerManipulator);
            }
        }

        /// <summary>
        /// Return the clamped value using current <see cref="lowValue"/> and <see cref="highValue"/> values.
        /// <remarks>
        /// The method also checks if low and high values are inverted.
        /// </remarks>
        /// </summary>
        /// <param name="newValue">The value to clamp.</param>
        /// <returns></returns>
        protected virtual TValueType GetClampedValue(TValueType newValue)
        {
            TValueType lowest = lowValue, highest = highValue;
            if (lowest is IComparable lowC && highest is IComparable highC && lowC.CompareTo(highC) > 0)
            {
                // ReSharper disable once SwapViaDeconstruction
                var t = lowest;
                lowest = highest;
                highest = t;
            }

            return Clamp(newValue, lowest, highest);
        }

        /// <summary>
        /// Called when the low or high value has changed and needs to check if the current value fits in this new range.
        /// </summary>
        protected virtual void ClampValue()
        {
            value = m_Value;
        }

        /// <summary>
        /// Utility method to clamp a <typeparamref name="TValueType"/> value between specified bounds.
        /// </summary>
        /// <param name="v">The value to clamp.</param>
        /// <param name="lowBound">Minimum</param>
        /// <param name="highBound">Maximum</param>
        /// <returns>The clamped value.</returns>
        protected virtual TValueType Clamp(TValueType v, TValueType lowBound, TValueType highBound)
        {
            var result = v;
            if (lowBound is IComparable lowC && lowC.CompareTo(v) > 0)
                result = lowBound;
            if (highBound is IComparable highC && highC.CompareTo(v) < 0)
                result = highBound;
            return result;
        }

        /// <summary>
        /// Method to implement to resolve a <typeparamref name="TValueType"/> value into a <see cref="string"/> value.
        /// <para>You can use <see cref="object.ToString"/> for floating point value types for example.</para>
        /// <para>You can also round the value if you want a specific number of decimals.</para>
        /// </summary>
        /// <param name="val">The <typeparamref name="TValueType"/> value to convert.</param>
        /// <returns></returns>
        protected virtual string ParseValueToString(TValueType val)
        {
            return val.ToString();
        }

        /// <summary>
        /// Method to implement to resolve a <see cref="string"/> value into a <typeparamref name="TValueType"/> value.
        /// <para>You can use <see cref="float.TryParse(string, out float)"/> for floating point value types for example.</para>
        /// </summary>
        /// <param name="strValue">The <see cref="string"/> value to convert.</param>
        /// <param name="value">The <see cref="string"/>The converted value.</param>
        /// <returns>True if can be parsed, False otherwise.</returns>
        protected abstract bool ParseStringToValue(string strValue, out TValueType value);

        /// <summary>
        /// Method to implement which returns a value based on the linear interpolation of a given interpolant between
        /// a specific range.
        /// <para>Usually you can use directly <see cref="Mathf.LerpUnclamped"/> for floating point value types.</para>
        /// </summary>
        /// <param name="a">The lowest value in the range.</param>
        /// <param name="b">The highest value in the range.</param>
        /// <param name="interpolant">The normalized value to process.</param>
        /// <returns></returns>
        protected abstract TValueType SliderLerpUnclamped(TValueType a, TValueType b, float interpolant);

        /// <summary>
        /// Method to implement which returns the normalized value of a given value in a specific range.
        /// <para>Usually you can use directly an <see cref="Mathf.InverseLerp"/> for floating point value types.</para>
        /// </summary>
        /// <param name="currentValue">The value to normalize.</param>
        /// <param name="lowerValue">The lowest value in the range.</param>
        /// <param name="higherValue">The highest value in the range.</param>
        /// <returns></returns>
        protected abstract float SliderNormalizeValue(TValueType currentValue, TValueType lowerValue, TValueType higherValue);

        /// <summary>
        /// Method to implement which returns the decrement of a given value.
        /// </summary>
        /// <param name="val"> The value to decrement.</param>
        /// <returns> The decremented value.</returns>
        protected abstract TValueType Decrement(TValueType val);

        /// <summary>
        /// Method to implement which returns the increment of a given value.
        /// </summary>
        /// <param name="val"> The value to increment.</param>
        /// <returns> The incremented value.</returns>
        protected abstract TValueType Increment(TValueType val);
    }

    /// <summary>
    /// Base class for TouchSlider UI elements (<see cref="TouchSliderFloat"/>, <see cref="TouchSliderInt"/>).
    /// </summary>
    /// <typeparam name="TValueType">A comparable value type.</typeparam>
    public abstract class TouchSlider<TValueType> : BaseSlider<TValueType> where TValueType : IComparable, IEquatable<TValueType>
    {
        const int k_EventDelay = 16;

        /// <summary>
        /// The TouchSlider main styling class.
        /// </summary>
        public const string ussClassName = "appui-touchslider";

        /// <summary>
        /// The TouchSlider progress styling class.
        /// </summary>
        public static readonly string progressUssClassName = ussClassName + "__progress";

        /// <summary>
        /// The TouchSlider label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The TouchSlider value label styling class.
        /// </summary>
        public static readonly string valueUssClassName = ussClassName + "__valuelabel";

        /// <summary>
        /// The TouchSlider size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly VisualElement m_ProgressElement;

        readonly UIElements.TextField m_InputField;

        bool m_IsEditingTextField;

        readonly LocalizedTextElement m_LabelElement;

        Size m_Size;

        readonly LocalizedTextElement m_ValueLabelElement;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected TouchSlider()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;

            m_ProgressElement = new VisualElement
            {
                name = progressUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_ProgressElement.AddToClassList(progressUssClassName);
            hierarchy.Add(m_ProgressElement);

            m_LabelElement = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);
            hierarchy.Add(m_LabelElement);

            m_ValueLabelElement = new LocalizedTextElement { name = valueUssClassName, pickingMode = PickingMode.Ignore };
            m_ValueLabelElement.AddToClassList(valueUssClassName);
            hierarchy.Add(m_ValueLabelElement);

            m_InputField = new UIElements.TextField { name = valueUssClassName, pickingMode = PickingMode.Position };
            m_InputField.BlinkingCursor();
            m_InputField.AddToClassList(valueUssClassName);
            m_InputField.RegisterCallback<FocusEvent>(OnInputFocusedIn);
            m_InputField.RegisterCallback<FocusOutEvent>(OnInputFocusedOut);
            hierarchy.Add(m_InputField);

            HideInputField();

            m_DraggerManipulator = new Draggable(OnTrackClicked, OnTrackDragged, OnTrackUp, OnTrackDown);
            this.AddManipulator(m_DraggerManipulator);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            size = Size.M;
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        /// <summary>
        /// Specify the size of the slider.
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
        /// Specify a unit for the value encapsulated in this slider.
        /// <para>This unit will be displayed next to value into the slider.</para>
        /// </summary>
        public string label
        {
            get => m_LabelElement.text;
            set => m_LabelElement.text = value;
        }

        /// <summary>
        /// Set the value of the slider without notifying the value change.
        /// </summary>
        /// <param name="newValue"> The new value to set.</param>
        public override void SetValueWithoutNotify(TValueType newValue)
        {
            if (m_IsEditingTextField)
                return;

            newValue = GetClampedValue(newValue);
            var strValue = ParseValueToString(newValue);

            m_Value = newValue;
            m_InputField.SetValueWithoutNotify(strValue);
            m_ValueLabelElement.text = strValue;

            if (validateValue != null) invalid = !validateValue(m_Value);

            if (panel == null || !contentRect.IsValid())
                return;

            var norm = SliderNormalizeValue(newValue, lowValue, highValue);
            var width = resolvedStyle.width * Mathf.Clamp01(norm);
            m_ProgressElement.style.width = width;
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Callback when the interactive part of the slider is clicked.
        /// </summary>
        protected override void OnTrackClicked()
        {
            if (!m_DraggerManipulator.hasMoved)
                ShowInputField();
        }

        void OnInputFocusedIn(FocusEvent evt)
        {
            m_IsEditingTextField = true;
            AddToClassList(Styles.focusedUssClassName);
        }

        void OnInputFocusedOut(FocusOutEvent evt)
        {
            m_IsEditingTextField = false;
            RemoveFromClassList(Styles.focusedUssClassName);
            HideInputField();

            if (ParseStringToValue(m_InputField.value, out var val))
            {
                var newValue = GetClampedValue(val);

                if (EqualityComparer<TValueType>.Default.Equals(newValue, value))
                    return;
                value = newValue;
            }
            else
            {
                m_InputField.value = ParseValueToString(value);
            }
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target == this && focusController.focusedElement == this)
            {
                var handled = false;
                var previousValue = value;
                var newValue = previousValue;

                if (evt.keyCode == KeyCode.LeftArrow)
                {
                    newValue = Decrement(newValue);
                    handled = true;
                }
                else if (evt.keyCode == KeyCode.RightArrow)
                {
                    newValue = Increment(newValue);
                    handled = true;
                }

                if (handled)
                {
                    evt.StopPropagation();
                    evt.PreventDefault();

                    SetValueWithoutNotify(newValue);

                    using var changingEvt = ChangingEvent<TValueType>.GetPooled();
                    changingEvt.previousValue = previousValue;
                    changingEvt.newValue = newValue;
                    changingEvt.target = this;
                    SendEvent(changingEvt);
                }
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        void ShowInputField()
        {
            m_ValueLabelElement.style.display = DisplayStyle.None;
            m_InputField.style.display = DisplayStyle.Flex;
            m_InputField.schedule.Execute(() => m_InputField.Focus()).ExecuteLater(k_EventDelay);
        }

        void HideInputField()
        {
            m_ValueLabelElement.style.display = DisplayStyle.Flex;
            m_InputField.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="TouchSlider{TValueType}"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription { name = "label" };

            readonly UxmlStringAttributeDescription m_Format = new UxmlStringAttributeDescription { name = "format", defaultValue = null };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (TouchSlider<TValueType>)ve;
                element.label = m_Label.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);

                string formatStr = null;
                if (m_Format.TryGetValueFromBag(bag, cc, ref formatStr) && !string.IsNullOrEmpty(formatStr))
                    element.formatString = formatStr;

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }

    /// <summary>
    /// TouchSlider UI element for integer values.
    /// </summary>
    public class TouchSliderInt : TouchSlider<int>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TouchSliderInt()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
        }

        /// <summary>
        /// The increment factor for the slider.
        /// </summary>
        public int incrementFactor { get; set; } = 1;

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out int v)
        {
            var ret = int.TryParse(strValue, out var val);
            v = val;
            return ret;
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseValueToString"/>
        protected override string ParseValueToString(int val)
        {
            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderLerpUnclamped"/>
        protected override int SliderLerpUnclamped(int a, int b, float interpolant)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, interpolant));
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderNormalizeValue"/>
        protected override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Increment"/>
        protected override int Increment(int val)
        {
            return val + incrementFactor;
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Decrement"/>
        protected override int Decrement(int val)
        {
            return val - incrementFactor;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="TouchSliderInt"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<TouchSliderInt, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="TouchSliderInt"/>.
        /// </summary>
        public new class UxmlTraits : TouchSlider<int>.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_HighValue = new UxmlStringAttributeDescription { name = "high-value", defaultValue = "1" };

            readonly UxmlStringAttributeDescription m_LowValue = new UxmlStringAttributeDescription { name = "low-value", defaultValue = "0" };

            readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value", defaultValue = "0" };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var elem = (TouchSliderInt)ve;

                var val = elem.ParseStringToValue(m_Value.GetValueFromBag(bag, cc), out var convertedVal) ? convertedVal : 0;
                var highVal = elem.ParseStringToValue(m_HighValue.GetValueFromBag(bag, cc), out var convertedHighVal) ? convertedHighVal : 1;
                var lowVal = elem.ParseStringToValue(m_LowValue.GetValueFromBag(bag, cc), out var convertedLowVal) ? convertedLowVal : 0;

                elem.highValue = highVal;
                elem.lowValue = lowVal;
                elem.SetValueWithoutNotify(val);
            }
        }
    }

    /// <summary>
    /// TouchSlider UI element for floating point values.
    /// </summary>
    public class TouchSliderFloat : TouchSlider<float>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TouchSliderFloat()
        {
            formatString = UINumericFieldsUtils.k_FloatFieldFormatString;
        }

        /// <summary>
        /// The increment factor for the slider.
        /// </summary>
        public float incrementFactor { get; set; } = 0.1f;

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out float v)
        {
            var ret = float.TryParse(strValue, out var val);
            v = val;
            return ret;
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseValueToString"/>
        protected override string ParseValueToString(float val)
        {
            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderLerpUnclamped"/>
        protected override float SliderLerpUnclamped(float a, float b, float interpolant)
        {
            return Mathf.LerpUnclamped(a, b, interpolant);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderNormalizeValue"/>
        protected override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Increment"/>
        protected override float Increment(float val)
        {
            return val + incrementFactor;
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Decrement"/>
        protected override float Decrement(float val)
        {
            return val - incrementFactor;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="TouchSliderFloat"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<TouchSliderFloat, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="TouchSliderFloat"/>.
        /// </summary>
        public new class UxmlTraits : TouchSlider<float>.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_HighValue = new UxmlStringAttributeDescription { name = "high-value", defaultValue = "1" };

            readonly UxmlStringAttributeDescription m_LowValue = new UxmlStringAttributeDescription { name = "low-value", defaultValue = "0" };

            readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value", defaultValue = "0" };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var elem = (TouchSliderFloat)ve;

                var val = elem.ParseStringToValue(m_Value.GetValueFromBag(bag, cc), out var convertedVal) ? convertedVal : 0;
                var highVal = elem.ParseStringToValue(m_HighValue.GetValueFromBag(bag, cc), out var convertedHighVal) ? convertedHighVal : 1f;
                var lowVal = elem.ParseStringToValue(m_LowValue.GetValueFromBag(bag, cc), out var convertedLowVal) ? convertedLowVal : 0;

                elem.highValue = highVal;
                elem.lowValue = lowVal;
                elem.SetValueWithoutNotify(val);
            }
        }
    }
}
