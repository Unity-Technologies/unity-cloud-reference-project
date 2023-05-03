using System.Collections.Generic;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A slider that allows the user to select a color value.
    /// </summary>
    public sealed class ColorSlider : BaseSlider<float>
    {
        /// <summary>
        /// The ColorSlider main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-colorslider";

        /// <summary>
        /// The ColorSlider thumb container styling class.
        /// </summary>
        public static readonly string thumbContainerUssClassName = ussClassName + "__thumbcontainer";

        /// <summary>
        /// The ColorSlider thumb styling class.
        /// </summary>
        public static readonly string thumbUssClassName = ussClassName + "__thumb";

        /// <summary>
        /// The ColorSlider track styling class.
        /// </summary>
        public static readonly string trackUssClassName = ussClassName + "__track";

        /// <summary>
        /// The ColorSlider track swatch styling class.
        /// </summary>
        public static readonly string trackSwatchUssClassName = ussClassName + "__colorcontainer";

        /// <summary>
        /// The ColorSlider thumb content styling class.
        /// </summary>
        public static readonly string thumbContentUssClassName = ussClassName + "__thumb-content";

        /// <summary>
        /// The ColorSlider size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly ColorSwatch m_TrackSwatch;

        readonly VisualElement m_Track;

        readonly ExVisualElement m_Thumb;

        readonly VisualElement m_ThumbContainer;

        readonly VisualElement m_ThumbContent;

        Size m_Size;

        /// <summary>
        /// The currently selected color value.
        /// </summary>
        public Color colorValue => m_ThumbContent.resolvedStyle.backgroundColor;

        /// <summary>
        /// The current color range in the track.
        /// </summary>
        public List<ColorEntry> colorRange
        {
            get => m_TrackSwatch.value;
            set
            {
                m_TrackSwatch.value = value;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// The current size of the ActionButton.
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
        /// The delta value used when interacting with the slider with the keyboard.
        /// </summary>
        public float incrementFactor { get; set; } = 0.01f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorSlider()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            passMask = 0;

            m_Track = new VisualElement { name = trackUssClassName, pickingMode = PickingMode.Ignore };
            m_Track.AddToClassList(trackUssClassName);

            m_TrackSwatch = new ColorSwatch { name = trackSwatchUssClassName, pickingMode = PickingMode.Ignore };
            m_TrackSwatch.AddToClassList(trackSwatchUssClassName);
            m_TrackSwatch.SetValueWithoutNotify(new List<ColorEntry>
            {
                new ColorEntry(Color.clear, 0),
                new ColorEntry(Color.clear, 1),
            });

            m_ThumbContent = new VisualElement
            {
                name = thumbContentUssClassName,
                usageHints = UsageHints.DynamicColor,
                pickingMode = PickingMode.Ignore
            };
            m_ThumbContent.AddToClassList(thumbContentUssClassName);
            m_ThumbContent.style.backgroundColor = Color.clear;

            m_ThumbContainer = new VisualElement
            {
                name = thumbContainerUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_ThumbContainer.AddToClassList(thumbContainerUssClassName);

            m_Thumb = new ExVisualElement
            {
                name = thumbUssClassName,
                pickingMode = PickingMode.Ignore,
                passMask = Passes.Clear | Passes.Borders | Passes.BackgroundColor | Passes.OutsetShadows
            };
            m_Thumb.AddToClassList(thumbUssClassName);

            hierarchy.Add(m_Track);
            m_Track.hierarchy.Add(m_TrackSwatch);
            hierarchy.Add(m_ThumbContainer);
            m_ThumbContainer.hierarchy.Add(m_Thumb);
            m_Thumb.hierarchy.Add(m_ThumbContent);

            m_TrackSwatch.StretchToParentSize();

            size = Size.M;
            lowValue = 0;
            highValue = 1f;
            SetValueWithoutNotify(0);

            m_DraggerManipulator = new Draggable(OnTrackClicked, OnTrackDragged, OnTrackUp, OnTrackDown);
            this.AddManipulator(m_DraggerManipulator);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Thumb.passMask = Passes.Clear | Passes.Borders | Passes.BackgroundColor | Passes.OutsetShadows;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Thumb.passMask = Passes.Clear | Passes.Borders | Passes.BackgroundColor | Passes.OutsetShadows | Passes.Outline;
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

                    using var changingEvt = ChangingEvent<float>.GetPooled();
                    changingEvt.previousValue = previousValue;
                    changingEvt.newValue = newValue;
                    changingEvt.target = this;
                    SendEvent(changingEvt);
                }
            }
        }

        /// <summary>
        /// Sets the value of the slider without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value of the slider. </param>
        public override void SetValueWithoutNotify(float newValue)
        {
            m_Value = newValue;

            if (validateValue != null) invalid = !validateValue(m_Value);

            RefreshUI();
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out float v)
        {
            return float.TryParse(strValue, out v);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderLerpUnclamped"/>
        protected override float SliderLerpUnclamped(float a, float b, float t)
        {
            return Mathf.LerpUnclamped(a, b, t);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.SliderNormalizeValue"/>
        protected override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Decrement"/>
        protected override float Decrement(float val)
        {
            return val - incrementFactor;
        }

        /// <inheritdoc cref="BaseSlider{TValueType}.Increment"/>
        protected override float Increment(float val)
        {
            return val + incrementFactor;
        }

        void RefreshUI()
        {
            if (panel == null || !contentRect.IsValid())
                return;

            var firstIndex = colorRange.FindLastIndex(e => e.position <= m_Value);
            var secondIndex = Mathf.Min(firstIndex + 1, colorRange.Count - 1);
            var delta = (m_Value - colorRange[firstIndex].position) / Mathf.Max(0.001f, colorRange[secondIndex].position - colorRange[firstIndex].position);
            var color = Color.Lerp(colorRange[firstIndex].color, colorRange[secondIndex].color, delta);
            m_ThumbContent.style.backgroundColor = color;
            m_ThumbContainer.style.left = new StyleLength(new Length(m_Value * 100f, LengthUnit.Percent));
        }

        /// <summary>
        /// Instantiates an <see cref="ColorSlider"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ColorSlider, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ColorSlider"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlFloatAttributeDescription m_Value = new UxmlFloatAttributeDescription
            {
                name = "value",
                defaultValue = 0
            };

            readonly UxmlColorAttributeDescription m_From = new UxmlColorAttributeDescription
            {
                name = "from",
                defaultValue = new Color(1, 0, 0, 0)
            };

            readonly UxmlColorAttributeDescription m_To = new UxmlColorAttributeDescription
            {
                name = "to",
                defaultValue = new Color(1, 0, 0, 1)
            };

            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
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

                var el = (ColorSlider)ve;
                el.size = m_Size.GetValueFromBag(bag, cc);
                Color from = Color.black, to = Color.black;
                if (m_From.TryGetValueFromBag(bag, cc, ref from) && m_To.TryGetValueFromBag(bag, cc, ref to))
                {
                    var range = new List<ColorEntry>()
                    {
                        new ColorEntry(from, 0),
                        new ColorEntry(to, 1)
                    };
                    el.colorRange = range;
                }
                el.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
