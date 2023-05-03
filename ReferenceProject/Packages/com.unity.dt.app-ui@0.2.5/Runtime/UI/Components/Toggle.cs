using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Toggle UI element.
    /// </summary>
    public class Toggle : VisualElement, IValidatableElement<bool>
    {
        /// <summary>
        /// The Toggle main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-toggle";

        /// <summary>
        /// The Toggle size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Toggle emphasized mode styling class.
        /// </summary>
        public static readonly string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Toggle box styling class.
        /// </summary>
        public static readonly string boxUssClassName = ussClassName + "__box";

        /// <summary>
        /// The Toggle box padded styling class.
        /// </summary>
        public static readonly string paddedBoxUssClassName = ussClassName + "__boxpadded";

        /// <summary>
        /// The Toggle checkmark container styling class.
        /// </summary>
        public static readonly string checkmarkContainerUssClassName = ussClassName + "__checkmarkcontainer";

        /// <summary>
        /// The Toggle checkmark styling class.
        /// </summary>
        public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Toggle label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        readonly LocalizedTextElement m_Label;

        Size m_Size;

        bool m_Value;

        Clickable m_Clickable;

        readonly ExVisualElement m_Box;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Toggle()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            clickable = new Submittable(OnClick);

            var checkmark = new Icon
            {
                name = checkmarkUssClassName,
                iconName = "radio-bullet",
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            checkmark.AddToClassList(checkmarkUssClassName);
            var checkmarkContainer = new VisualElement { name = checkmarkContainerUssClassName, pickingMode = PickingMode.Ignore };
            checkmarkContainer.AddToClassList(checkmarkContainerUssClassName);
            var boxPadded = new VisualElement { name = paddedBoxUssClassName, pickingMode = PickingMode.Ignore };
            boxPadded.AddToClassList(paddedBoxUssClassName);
            m_Box = new ExVisualElement { name = boxUssClassName, pickingMode = PickingMode.Ignore, passMask = 0 };
            m_Box.AddToClassList(boxUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            checkmarkContainer.hierarchy.Add(checkmark);
            boxPadded.hierarchy.Add(checkmarkContainer);
            m_Box.hierarchy.Add(boxPadded);
            hierarchy.Add(m_Box);
            hierarchy.Add(m_Label);

            size = Size.M;
            emphasized = false;
            invalid = false;
            SetValueWithoutNotify(false);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <summary>
        /// Clickable Manipulator for this Toggle.
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
        /// The Toggle size.
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
        /// The Toggle emphasized mode.
        /// </summary>
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set => EnableInClassList(emphasizedUssClassName, value);
        }

        /// <summary>
        /// The Toggle label.
        /// </summary>
        public string label
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        /// <summary>
        /// The invalid state of the Toggle.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// The validation function for the Toggle.
        /// </summary>
        public Func<bool, bool> validateValue { get; set; }

        /// <summary>
        /// Set the Toggle value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(bool newValue)
        {
            m_Value = newValue;
            EnableInClassList(Styles.checkedUssClassName, m_Value);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The Toggle value.
        /// </summary>
        public bool value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<bool>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

        void OnClick()
        {
            value = !value;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Toggle"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Toggle, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Toggle"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Emphasized = new UxmlBoolAttributeDescription
            {
                name = "emphasized",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlBoolAttributeDescription m_Value = new UxmlBoolAttributeDescription
            {
                name = "value",
                defaultValue = false
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

                var element = (Toggle)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.emphasized = m_Emphasized.GetValueFromBag(bag, cc);
                element.value = m_Value.GetValueFromBag(bag, cc);
                element.label = m_Label.GetValueFromBag(bag, cc);

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
