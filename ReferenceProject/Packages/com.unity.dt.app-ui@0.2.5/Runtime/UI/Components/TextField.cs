using System;
using System.Reflection;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Text Field UI element.
    /// </summary>
    public class TextField : ExVisualElement, IValidatableElement<string>
    {
        /// <summary>
        /// The TextField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-textfield";

        /// <summary>
        /// The TextField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The TextField leading container styling class.
        /// </summary>
        public static readonly string leadingContainerUssClassName = ussClassName + "__leadingcontainer";

        /// <summary>
        /// The TextField leading icon styling class.
        /// </summary>
        public static readonly string leadingIconUssClassName = ussClassName + "__leadingicon";

        /// <summary>
        /// The TextField input container styling class.
        /// </summary>
        public static readonly string inputContainerUssClassName = ussClassName + "__inputcontainer";

        /// <summary>
        /// The TextField input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The TextField placeholder styling class.
        /// </summary>
        public static readonly string placeholderUssClassName = ussClassName + "__placeholder";

        /// <summary>
        /// The TextField trailing container styling class.
        /// </summary>
        public static readonly string trailingContainerUssClassName = ussClassName + "__trailingcontainer";

        /// <summary>
        /// The TextField trailing icon styling class.
        /// </summary>
        public static readonly string trailingIconUssClassName = ussClassName + "__trailingicon";

        readonly UIElements.TextField m_InputField;

        readonly VisualElement m_LeadingContainer;

        readonly LocalizedTextElement m_Placeholder;

        Size m_Size;

        readonly VisualElement m_TrailingContainer;

        string m_Value;

        int m_VisualInputTabIndex;

        internal static readonly PropertyInfo isCompositeRootProp = typeof(VisualElement).GetProperty("isCompositeRoot", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static readonly PropertyInfo excludeFromFocusRingProp = typeof(Focusable).GetProperty("excludeFromFocusRing", BindingFlags.Instance | BindingFlags.NonPublic);

        VisualElement m_LeadingElement;

        VisualElement m_TrailingElement;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextField()
            : this(null) { }

        /// <summary>
        /// Construct a TextField with a predefined text value.
        /// <remarks>
        /// No event will be triggered when setting the text value during construction.
        /// </remarks>
        /// </summary>
        /// <param name="value">A default text value.</param>
        public TextField(string value)
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;
            isCompositeRootProp!.SetValue(this, true);
            excludeFromFocusRingProp!.SetValue(this, true);
            delegatesFocus = true;

            m_LeadingContainer = new VisualElement { name = leadingContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_LeadingContainer.AddToClassList(leadingContainerUssClassName);
            hierarchy.Add(m_LeadingContainer);

            var leadingIcon = new Icon { name = leadingIconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            leadingIcon.AddToClassList(leadingIconUssClassName);
            m_LeadingContainer.hierarchy.Add(leadingIcon);

            var inputContainer = new VisualElement { name = inputContainerUssClassName, pickingMode = PickingMode.Ignore };
            inputContainer.AddToClassList(inputContainerUssClassName);
            hierarchy.Add(inputContainer);

            m_Placeholder = new LocalizedTextElement { name = placeholderUssClassName, pickingMode = PickingMode.Ignore, focusable = false };
            m_Placeholder.AddToClassList(placeholderUssClassName);
            inputContainer.hierarchy.Add(m_Placeholder);

            m_InputField = new UIElements.TextField { name = inputUssClassName };
            m_InputField.AddToClassList(inputUssClassName);
            m_InputField.BlinkingCursor();
            inputContainer.hierarchy.Add(m_InputField);

            m_TrailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_TrailingContainer.AddToClassList(trailingContainerUssClassName);
            hierarchy.Add(m_TrailingContainer);

            var trailingIcon = new Icon { name = trailingIconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            trailingIcon.AddToClassList(trailingIconUssClassName);
            m_TrailingContainer.hierarchy.Add(trailingIcon);

            SetValueWithoutNotify(value);
            leadingElement = leadingIcon;
            trailingElement = trailingIcon;
            leadingIconName = null;
            trailingIconName = null;
            size = Size.M;

            m_InputField.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
            m_Placeholder.RegisterValueChangedCallback(OnPlaceholderChanged);
        }

        void OnPlaceholderChanged(ChangeEvent<string> evt)
        {
            evt.PreventDefault();
            evt.StopPropagation();
        }

        /// <summary>
        /// The content container of the TextField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The TextField leading element.
        /// </summary>
        public VisualElement leadingElement
        {
            get => m_LeadingElement;
            set
            {
                if (m_LeadingElement == value)
                    return;

                if (m_LeadingElement != null)
                    m_LeadingContainer.Remove(m_LeadingElement);

                m_LeadingElement = value;

                if (m_LeadingElement != null)
                    m_LeadingContainer.Add(m_LeadingElement);
            }
        }

        /// <summary>
        /// The TextField trailing element.
        /// </summary>
        public VisualElement trailingElement
        {
            get => m_TrailingElement;
            set
            {
                if (m_TrailingElement == value)
                    return;

                if (m_TrailingElement != null)
                    m_TrailingContainer.Remove(m_TrailingElement);

                m_TrailingElement = value;

                if (m_TrailingElement != null)
                    m_TrailingContainer.Add(m_TrailingElement);
            }
        }

        /// <summary>
        /// The TextField placeholder text.
        /// </summary>
        public string placeholder
        {
            get => m_Placeholder.text;
            set => m_Placeholder.text = value;
        }

        /// <summary>
        /// The trailing icon name.
        /// </summary>
        public string trailingIconName
        {
            get => (trailingElement as Icon)?.iconName;
            set
            {
                if (trailingElement is not Icon icon)
                    return;

                icon.iconName = value;
                m_TrailingContainer.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(icon.iconName));
            }
        }

        /// <summary>
        /// The leading icon name.
        /// </summary>
        public string leadingIconName
        {
            get => (leadingElement as Icon)?.iconName;
            set
            {
                if (leadingElement is not Icon icon)
                    return;

                icon.iconName = value;
                m_LeadingContainer.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(icon.iconName));
            }
        }

        /// <summary>
        /// The TextField size.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());

                switch (leadingElement)
                {
                    case ISizeableElement leadingSizeable:
                        leadingSizeable.size = m_Size;
                        break;
                    case Icon leadingIcon:
                        leadingIcon.size = m_Size switch
                        {
                            Size.S => IconSize.S,
                            Size.M => IconSize.S,
                            Size.L => IconSize.M,
                            _ => IconSize.S
                        };
                        break;
                }

                switch (trailingElement)
                {
                    case ISizeableElement trailingSizeable:
                        trailingSizeable.size = m_Size;
                        break;
                    case Icon trailingIcon:
                        trailingIcon.size = m_Size switch
                        {
                            Size.S => IconSize.S,
                            Size.M => IconSize.S,
                            Size.L => IconSize.M,
                            _ => IconSize.S
                        };
                        break;
                }
            }
        }

        /// <summary>
        /// The validation function for the TextField.
        /// </summary>
        public Func<string, bool> validateValue { get; set; }

        /// <summary>
        /// The invalid state of the TextField.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// Set the TextField value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the TextField. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue;
            m_InputField.SetValueWithoutNotify(m_Value);
            RefreshUI();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The TextField value.
        /// </summary>
        public string value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                {
                    RefreshUI();
                    return;
                }

                using var evt = ChangeEvent<string>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        void OnFocusedOut(FocusOutEvent evt)
        {
            RemoveFromClassList(Styles.focusedUssClassName);
            RemoveFromClassList(Styles.keyboardFocusUssClassName);
            value = m_InputField.value;
        }

        void OnFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            m_Placeholder.AddToClassList(Styles.hiddenUssClassName);
            passMask = 0;
        }

        void OnKeyboardFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            AddToClassList(Styles.keyboardFocusUssClassName);
            m_Placeholder.AddToClassList(Styles.hiddenUssClassName);
            passMask = Passes.Clear | Passes.Outline;
        }

        void RefreshUI()
        {
            m_Placeholder.EnableInClassList(Styles.hiddenUssClassName, !string.IsNullOrEmpty(m_Value));
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="TextField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<TextField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="TextField"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_LeadingIconName = new UxmlStringAttributeDescription
            {
                name = "leading-icon-name",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Placeholder = new UxmlStringAttributeDescription
            {
                name = "placeholder",
                defaultValue = null
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlStringAttributeDescription m_TrailingIconName = new UxmlStringAttributeDescription
            {
                name = "trailing-icon-name",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription
            {
                name = "value",
                defaultValue = null
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

                var el = (TextField)ve;

                var size = m_Size.GetValueFromBag(bag, cc);
                if (m_Size.TryGetValueFromBag(bag, cc, ref size))
                    el.size = size;
                
                var placeholder = m_Placeholder.GetValueFromBag(bag, cc);
                if (m_Placeholder.TryGetValueFromBag(bag, cc, ref placeholder))
                    el.placeholder = placeholder;
                
                var value = m_Value.GetValueFromBag(bag, cc);
                if (m_Value.TryGetValueFromBag(bag, cc, ref value))
                    el.value = value;
                
                var leadingIconName = m_LeadingIconName.GetValueFromBag(bag, cc);
                if (m_LeadingIconName.TryGetValueFromBag(bag, cc, ref leadingIconName))
                    el.leadingIconName = leadingIconName;
                
                var trailingIconName = m_TrailingIconName.GetValueFromBag(bag, cc);
                if (m_TrailingIconName.TryGetValueFromBag(bag, cc, ref trailingIconName))
                    el.trailingIconName = trailingIconName;
                
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
