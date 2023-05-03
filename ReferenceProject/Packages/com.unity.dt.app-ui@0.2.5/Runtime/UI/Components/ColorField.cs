using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Color Field UI element.
    /// </summary>
    public class ColorField : ExVisualElement, IValidatableElement<Color>, INotifyValueChanging<Color>, ISizeableElement
    {
        /// <summary>
        /// The ColorField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-colorfield";

        /// <summary>
        /// The ColorField color swatch styling class.
        /// </summary>
        public static readonly string colorSwatchUssClassName = ussClassName + "__color-swatch";

        /// <summary>
        /// The ColorField label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The ColorField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ColorField swatch only styling class.
        /// </summary>
        public static readonly string swatchOnlyUssClassName = ussClassName + "--swatch-only";

        readonly ColorSwatch m_SwatchElement;

        readonly LocalizedTextElement m_LabelElement;

        Color m_Value;

        Size m_Size;

        Type m_Type;

        Clickable m_Clickable;

        Color m_PreviousValue;

        ColorPicker m_Picker;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;
            clickable = new Submittable(OnClick);

            m_SwatchElement = new ColorSwatch
            {
                name = colorSwatchUssClassName,
                pickingMode = PickingMode.Ignore,
                round = true,
            };
            m_SwatchElement.AddToClassList(colorSwatchUssClassName);

            m_LabelElement = new LocalizedTextElement
            {
                name = labelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_LabelElement.AddToClassList(labelUssClassName);

            hierarchy.Add(m_SwatchElement);
            hierarchy.Add(m_LabelElement);

            size = Size.M;
            SetValueWithoutNotify(Color.clear);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnClick()
        {
            var wasInline = m_Picker != null && m_Picker.parent == parent;
            m_Picker?.parent?.Remove(m_Picker);

            if (inlinePicker && wasInline)
            {
                RemoveFromClassList(Styles.focusedUssClassName);
                m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
                using var evt = ChangeEvent<Color>.GetPooled(m_PreviousValue, m_Picker.value);
                SetValueWithoutNotify(m_Picker.value);
                evt.target = this;
                SendEvent(evt);
                return;
            }

            m_PreviousValue = value;
            m_Picker = m_Picker ?? new ColorPicker
            {
                showAlpha = true,
                showHex = true,
                showToolbar = true,
            };
            m_Picker.previousValue = m_PreviousValue;
            m_Picker.SetValueWithoutNotify(m_PreviousValue);
            m_Picker.RegisterValueChangedCallback(OnPickerValueChanged);
            if (inlinePicker)
            {
                var idx = parent.IndexOf(this) + 1;
                parent.Insert(idx, m_Picker);
            }
            else
            {
                var popover = Popover.Build(this, m_Picker);
                popover.dismissed += (_, _) =>
                {
                    RemoveFromClassList(Styles.focusedUssClassName);
                    m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
                    using var evt = ChangeEvent<Color>.GetPooled(m_PreviousValue, m_Picker.value);
                    SetValueWithoutNotify(m_Picker.value);
                    evt.target = this;
                    SendEvent(evt);
                    Focus();
                };
                popover.Show();
            }
            AddToClassList(Styles.focusedUssClassName);
        }

        void OnPickerValueChanged(ChangeEvent<Color> e)
        {
            if (e.newValue != value)
            {
                SetValueWithoutNotify(e.newValue);
                using var evt = ChangingEvent<Color>.GetPooled();
                evt.previousValue = m_PreviousValue;
                evt.newValue = e.newValue;
                evt.target = this;
                SendEvent(evt);
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

        /// <summary>
        /// The content container of this ColorField. This is null for ColorField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Clickable Manipulator for this AssetTargetField.
        /// </summary>
        public Clickable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// The ColorField size.
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
        /// The ColorField type. When this is true, the ColorField will only show the swatch.
        /// </summary>
        public bool swatchOnly
        {
            get => ClassListContains(swatchOnlyUssClassName);
            set => EnableInClassList(swatchOnlyUssClassName, value);
        }

        /// <summary>
        /// The ColorPicker position relative to the ColorField. When this is true, the ColorPicker will be inlined
        /// instead of being displayed in a Popover.
        /// </summary>
        public bool inlinePicker { get; set; }

        /// <summary>
        /// The ColorField invalid state.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// The ColorField validation function.
        /// </summary>
        public Func<Color, bool> validateValue { get; set; }

        /// <summary>
        /// Sets the ColorField value without notifying the ColorField.
        /// </summary>
        /// <param name="newValue"> The new ColorField value. </param>
        public void SetValueWithoutNotify(Color newValue)
        {
            m_Value = newValue;
            m_LabelElement.text = $"#{ColorExtensions.ColorToRgbaHex(m_Value)}";
            m_SwatchElement.color = m_Value;
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The ColorField value.
        /// </summary>
        public Color value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<Color>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Class to instantiate a <see cref="ColorField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ColorField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ColorField"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Invalid = new UxmlBoolAttributeDescription
            {
                name = "invalid",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_SwatchOnly = new UxmlBoolAttributeDescription
            {
                name = "swatch-only",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_InlinePicker = new UxmlBoolAttributeDescription
            {
                name = "inline-picker",
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

                var element = (ColorField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.invalid = m_Invalid.GetValueFromBag(bag, cc);
                element.swatchOnly = m_SwatchOnly.GetValueFromBag(bag, cc);
                element.inlinePicker = m_InlinePicker.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
