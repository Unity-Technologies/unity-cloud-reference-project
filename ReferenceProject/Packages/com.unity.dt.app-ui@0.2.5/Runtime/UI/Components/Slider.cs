using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Inline mode for the Slider value element.
    /// </summary>
    public enum InlineValue
    {
        /// <summary>
        /// Not inline.
        /// </summary>
        None,
        /// <summary>
        /// Inline on the left.
        /// </summary>
        Start,
        /// <summary>
        /// Inline on the right.
        /// </summary>
        End,
    }

    /// <summary>
    /// Base class for Sliders (<see cref="SliderFloat"/>, <see cref="SliderInt"/>).
    /// </summary>
    /// <typeparam name="TValueType">A comparable value type.</typeparam>
    public abstract class SliderBase<TValueType> : BaseSlider<TValueType> where TValueType : IComparable, IEquatable<TValueType>
    {
        /// <summary>
        /// The Slider main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-slider";

        /// <summary>
        /// The Slider size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Slider with tick labels variant styling class.
        /// </summary>
        public static readonly string tickLabelVariantUssClassName = ussClassName + "--tick-labels";

        /// <summary>
        /// The Slider no label variant styling class.
        /// </summary>
        public static readonly string noLabelUssClassName = ussClassName + "--no-label";

        /// <summary>
        /// The Slider tick styling class.
        /// </summary>
        public static readonly string tickUssClassName = ussClassName + "__tick";

        /// <summary>
        /// The Slider inline value styling class.
        /// </summary>
        public static readonly string inlineValueUssClassName = ussClassName + "--inline-value-";

        /// <summary>
        /// The Slider tick label styling class.
        /// </summary>
        public static readonly string tickLabelUssClassName = ussClassName + "__ticklabel";

        /// <summary>
        /// The Slider ticks container styling class.
        /// </summary>
        public static readonly string ticksUssClassName = ussClassName + "__ticks";

        /// <summary>
        /// The Slider track styling class.
        /// </summary>
        public static readonly string trackUssClassName = ussClassName + "__track";

        /// <summary>
        /// The Slider progress styling class.
        /// </summary>
        public static readonly string progressUssClassName = ussClassName + "__progress";

        /// <summary>
        /// The Slider handle styling class.
        /// </summary>
        public static readonly string handleUssClassName = ussClassName + "__handle";

        /// <summary>
        /// The Slider handle container styling class.
        /// </summary>
        public static readonly string handleContainerUssClassName = ussClassName + "__handle-container";

        /// <summary>
        /// The Slider label container styling class.
        /// </summary>
        public static readonly string labelContainerUssClassName = ussClassName + "__labelcontainer";

        /// <summary>
        /// The Slider label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The Slider value label styling class.
        /// </summary>
        public static readonly string valueLabelUssClassName = ussClassName + "__valuelabel";

        /// <summary>
        /// The Slider inline value label styling class.
        /// </summary>
        public static readonly string inlineValueLabelUssClassName = ussClassName + "__inline-valuelabel";

        /// <summary>
        /// The Slider controls styling class.
        /// </summary>
        public static readonly string controlsUssClassName = ussClassName + "__controls";

        /// <summary>
        /// The Slider control container styling class.
        /// </summary>
        public static readonly string controlContainerUssClassName = ussClassName + "__control-container";

        float m_FillOffset;

        readonly ExVisualElement m_Handle;

        readonly LocalizedTextElement m_Label;

        readonly VisualElement m_Progress;

        Size m_Size;

        int m_TickCount;

        bool m_TickLabel;

        readonly VisualElement m_Ticks;

        readonly LocalizedTextElement m_ValueLabel;

        readonly VisualElement m_HandleContainer;

        InlineValue m_InlineValue;

        readonly VisualElement m_Controls;

        readonly LocalizedTextElement m_InlineValueLabel;

        string m_LabelStr;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected SliderBase()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            var labelContainer = new VisualElement { name = labelContainerUssClassName, pickingMode = PickingMode.Ignore };
            labelContainer.AddToClassList(labelContainerUssClassName);
            hierarchy.Add(labelContainer);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            labelContainer.hierarchy.Add(m_Label);

            m_ValueLabel = new LocalizedTextElement { name = valueLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_ValueLabel.AddToClassList(valueLabelUssClassName);
            labelContainer.hierarchy.Add(m_ValueLabel);

            var controlContainer = new VisualElement { name = controlContainerUssClassName, pickingMode = PickingMode.Ignore };
            controlContainer.AddToClassList(controlContainerUssClassName);
            hierarchy.Add(controlContainer);

            m_Controls = new VisualElement { name = controlsUssClassName, pickingMode = PickingMode.Ignore };
            m_Controls.AddToClassList(controlsUssClassName);
            controlContainer.hierarchy.Add(m_Controls);

            m_InlineValueLabel = new LocalizedTextElement { name = inlineValueLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_InlineValueLabel.AddToClassList(valueLabelUssClassName);
            m_InlineValueLabel.AddToClassList(inlineValueLabelUssClassName);
            controlContainer.hierarchy.Add(m_InlineValueLabel);

            var track = new VisualElement { name = trackUssClassName, pickingMode = PickingMode.Ignore };
            track.AddToClassList(trackUssClassName);
            m_Controls.hierarchy.Add(track);

            m_Ticks = new VisualElement { name = ticksUssClassName, pickingMode = PickingMode.Ignore };
            m_Ticks.AddToClassList(ticksUssClassName);
            m_Controls.hierarchy.Add(m_Ticks);

            m_Progress = new VisualElement
            {
                name = progressUssClassName,
                usageHints = UsageHints.DynamicTransform,
                pickingMode = PickingMode.Ignore
            };
            m_Progress.AddToClassList(progressUssClassName);
            m_Controls.hierarchy.Add(m_Progress);

            m_HandleContainer = new VisualElement
            {
                name = handleContainerUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_HandleContainer.AddToClassList(handleContainerUssClassName);
            m_Controls.hierarchy.Add(m_HandleContainer);

            m_Handle = new ExVisualElement
            {
                name = handleUssClassName,
                pickingMode = PickingMode.Ignore,
                passMask = 0
            };
            m_Handle.AddToClassList(handleUssClassName);
            m_HandleContainer.hierarchy.Add(m_Handle);

            size = Size.M;
            tickCount = 0;
            label = null;
            filled = false;
            fillOffset = 0;
            inlineValue = InlineValue.None;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            m_DraggerManipulator = new Draggable(OnTrackClicked, OnTrackDragged, OnTrackUp, OnTrackDown);
            this.AddManipulator(m_DraggerManipulator);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Handle.passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Handle.passMask = Passes.Clear | Passes.Outline;
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

        /// <summary>
        /// If the slider progress is filled.
        /// </summary>
        public bool filled
        {
            get => m_Progress.visible;
            set
            {
                m_Progress.visible = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// The inline mode for the slider value element.
        /// </summary>
        public InlineValue inlineValue
        {
            get => m_InlineValue;
            set
            {
                RemoveFromClassList(inlineValueUssClassName + m_InlineValue.ToString().ToLower());
                m_InlineValue = value;
                if (m_InlineValue != InlineValue.None)
                    AddToClassList(inlineValueUssClassName + m_InlineValue.ToString().ToLower());
            }
        }

        /// <summary>
        /// Should be normalized.
        /// </summary>
        public float fillOffset
        {
            get => m_FillOffset;
            set
            {
                m_FillOffset = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Text which will be used for the Slider label.
        /// </summary>
        public string label
        {
            get => m_LabelStr;
            set
            {
                m_LabelStr = value;
                RefreshLabel();
            }
        }

        /// <summary>
        /// The number of ticks to display on the slider.
        /// </summary>
        public int tickCount
        {
            get => m_TickCount;
            set
            {
                m_TickCount = value;
                m_Ticks.EnableInClassList(Styles.hiddenUssClassName, m_TickCount <= 0);
                m_Ticks.Clear();
                for (var i = 0; i < m_TickCount; i++)
                {
                    var tickItem = new VisualElement { name = tickUssClassName, pickingMode = PickingMode.Ignore };
                    tickItem.AddToClassList(tickUssClassName);
                    m_Ticks.Add(tickItem);
                }

                RefreshTickLabels();
            }
        }

        /// <summary>
        /// Should the tick labels be displayed.
        /// </summary>
        public bool tickLabel
        {
            get => m_TickLabel;
            set
            {
                m_TickLabel = value;
                RefreshTickLabels();
            }
        }

        /// <summary>
        /// The size of the slider.
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
        /// Set the value of the slider without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the slider. </param>
        public override void SetValueWithoutNotify(TValueType newValue)
        {
            newValue = GetClampedValue(newValue);
            var strValue = ParseValueToString(newValue);

            m_Value = newValue;
            m_ValueLabel.text = strValue;
            m_InlineValueLabel.text = strValue;

            if (validateValue != null) invalid = !validateValue(m_Value);

            RefreshUI();
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.GetSliderRect"/>
        protected override Rect GetSliderRect() => this.WorldToLocal(m_Controls.LocalToWorld(m_Controls.contentRect));

        /// <inheritdoc cref="BaseSlider{TValueType}.OnSliderRangeChanged"/>
        protected override void OnSliderRangeChanged()
        {
            base.OnSliderRangeChanged();
            RefreshTickLabels();
        }

        void RefreshLabel()
        {
            m_Label.text = label;
            EnableInClassList(noLabelUssClassName, string.IsNullOrEmpty(label));
        }

        void RefreshUI()
        {
            if (panel == null || !contentRect.IsValid())
                return;

            // set the label
            RefreshLabel();

            // progress bar
            var val = Mathf.Clamp01(SliderNormalizeValue(m_Value, lowValue, highValue));
            var trackWidth = GetSliderRect().width;
            m_Progress.style.width = trackWidth * Mathf.Abs(val - fillOffset);
            m_Progress.style.left = trackWidth * Mathf.Min(fillOffset, val);

            // handle
            m_HandleContainer.style.left = trackWidth * val;

            MarkDirtyRepaint();
        }

        void RefreshTickLabels()
        {
            EnableInClassList(tickLabelVariantUssClassName, tickLabel);
            for (var i = 0; i < tickCount; i++)
            {
                var tick = m_Ticks.ElementAt(i);
                if (tickLabel)
                {
                    var tickLabelElement = tick.childCount == 0 ? new TextElement { pickingMode = PickingMode.Ignore } : (TextElement)tick.ElementAt(0);
                    var ratio = i / ((float)tickCount - 1);
                    var tickVal = SliderLerpUnclamped(lowValue, highValue, ratio);
                    tickLabelElement.text = ParseValueToString(tickVal);
                    tickLabelElement.AddToClassList(tickLabelUssClassName);
                    if (tickLabelElement.parent == null)
                        tick.Add(tickLabelElement);
                }
                else
                {
                    tick.Clear();
                }
            }
        }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SliderBase{TValueType}"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Filled = new UxmlBoolAttributeDescription
            {
                name = "filled",
                defaultValue = false
            };

            readonly UxmlFloatAttributeDescription m_FillOffset = new UxmlFloatAttributeDescription
            {
                name = "fill-offset",
                defaultValue = 0
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Format = new UxmlStringAttributeDescription
            {
                name = "format",
                defaultValue = null
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlIntAttributeDescription m_TickCount = new UxmlIntAttributeDescription
            {
                name = "tick-count",
                defaultValue = 0
            };

            readonly UxmlBoolAttributeDescription m_TickLabel = new UxmlBoolAttributeDescription
            {
                name = "tick-label",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<InlineValue> m_InlineValue = new UxmlEnumAttributeDescription<InlineValue>
            {
                name = "inline-value",
                defaultValue = InlineValue.None
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

                var el = (SliderBase<TValueType>)ve;
                el.label = m_Label.GetValueFromBag(bag, cc);
                el.size = m_Size.GetValueFromBag(bag, cc);
                el.tickCount = m_TickCount.GetValueFromBag(bag, cc);
                el.tickLabel = m_TickLabel.GetValueFromBag(bag, cc);
                el.filled = m_Filled.GetValueFromBag(bag, cc);
                el.fillOffset = m_FillOffset.GetValueFromBag(bag, cc);
                el.inlineValue = m_InlineValue.GetValueFromBag(bag, cc);

                string formatStr = null;
                if (m_Format.TryGetValueFromBag(bag, cc, ref formatStr) && !string.IsNullOrEmpty(formatStr))
                    el.formatString = formatStr;

                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }

    /// <summary>
    /// Slider UI element for floating point values.
    /// </summary>
    public class SliderFloat : SliderBase<float>
    {
        /// <summary>
        /// The increment factor used when the slider is interacted with using the keyboard.
        /// </summary>
        public float incrementFactor { get; set; } = 0.1f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SliderFloat()
        {
            formatString = UINumericFieldsUtils.k_FloatFieldFormatString;
        }

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
        /// Factory class to instantiate a <see cref="SliderFloat"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SliderFloat, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SliderFloat"/>.
        /// </summary>
        public new class UxmlTraits : SliderBase<float>.UxmlTraits
        {
            readonly UxmlFloatAttributeDescription m_HighValue = new UxmlFloatAttributeDescription
            {
                name = "high-value",
                defaultValue = 100
            };

            readonly UxmlFloatAttributeDescription m_LowValue = new UxmlFloatAttributeDescription
            {
                name = "low-value",
                defaultValue = 0
            };

            readonly UxmlFloatAttributeDescription m_Value = new UxmlFloatAttributeDescription
            {
                name = "value",
                defaultValue = 0
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

                var el = (SliderFloat)ve;
                el.lowValue = m_LowValue.GetValueFromBag(bag, cc);
                el.highValue = m_HighValue.GetValueFromBag(bag, cc);
                el.value = m_Value.GetValueFromBag(bag, cc);
            }
        }
    }

    /// <summary>
    /// Slider UI element for integer values.
    /// </summary>
    public class SliderInt : SliderBase<int>
    {
        /// <summary>
        /// The increment factor used when the slider is interacted with using the keyboard.
        /// </summary>
        public int incrementFactor { get; set; } = 1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SliderInt()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
        }

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
        /// Factory class to instantiate a <see cref="SliderInt"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SliderInt, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SliderInt"/>.
        /// </summary>
        public new class UxmlTraits : SliderBase<int>.UxmlTraits
        {
            readonly UxmlIntAttributeDescription m_HighValue = new UxmlIntAttributeDescription
            {
                name = "high-value",
                defaultValue = 100
            };

            readonly UxmlIntAttributeDescription m_LowValue = new UxmlIntAttributeDescription
            {
                name = "low-value",
                defaultValue = 0
            };

            readonly UxmlIntAttributeDescription m_Value = new UxmlIntAttributeDescription
            {
                name = "value",
                defaultValue = 0
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

                var el = (SliderInt)ve;
                el.lowValue = m_LowValue.GetValueFromBag(bag, cc);
                el.highValue = m_HighValue.GetValueFromBag(bag, cc);
                el.value = m_Value.GetValueFromBag(bag, cc);
            }
        }
    }
}
