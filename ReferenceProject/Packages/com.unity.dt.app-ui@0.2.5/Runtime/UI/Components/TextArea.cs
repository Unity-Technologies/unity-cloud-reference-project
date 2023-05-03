using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Text Area UI element.
    /// </summary>
    public class TextArea : ExVisualElement, IValidatableElement<string>
    {
        /// <summary>
        /// The TextArea main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-textarea";

        /// <summary>
        /// The TextArea input container styling class.
        /// </summary>
        public static readonly string scrollViewUssClassName = ussClassName + "__scrollview";

        /// <summary>
        /// The TextArea resize handle styling class.
        /// </summary>
        public static readonly string resizeHandleUssClassName = ussClassName + "__resize-handle";

        /// <summary>
        /// The TextArea input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The TextArea placeholder styling class.
        /// </summary>
        public static readonly string placeholderUssClassName = ussClassName + "__placeholder";

        readonly UIElements.TextField m_InputField;

        readonly LocalizedTextElement m_Placeholder;

        readonly ScrollView m_ScrollView;

        Size m_Size;

        string m_Value;

        int m_VisualInputTabIndex;

        readonly VisualElement m_ResizeHandle;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextArea()
            : this(null) { }

        /// <summary>
        /// Construct a TextArea with a predefined text value.
        /// <remarks>
        /// No event will be triggered when setting the text value during construction.
        /// </remarks>
        /// </summary>
        /// <param name="value">A default text value.</param>
        public TextArea(string value)
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            passMask = 0;

            m_ScrollView = new ScrollView
            {
                name = scrollViewUssClassName,
                elasticity = 0,
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
#if (UNITY_2021_3 && UNITY_2021_3_NIK) || (UNITY_2022_1 && UNITY_2022_1_NIK) || (UNITY_2022_2 && UNITY_2022_2_NIK) || UNITY_2022_3 || (UNITY_2023_1 && UNITY_2023_1_NIK) || UNITY_2023_2_OR_NEWER
                nestedInteractionKind = ScrollView.NestedInteractionKind.StopScrolling,
#endif
            };
            m_ScrollView.AddToClassList(scrollViewUssClassName);
            hierarchy.Add(m_ScrollView);

            m_Placeholder = new LocalizedTextElement
            {
                name = placeholderUssClassName,
                pickingMode = PickingMode.Ignore,
                focusable = false
            };
            m_Placeholder.AddToClassList(placeholderUssClassName);
            hierarchy.Add(m_Placeholder);

            m_InputField = new UIElements.TextField { name = inputUssClassName, multiline = true };
            m_InputField.AddToClassList(inputUssClassName);
            m_InputField.BlinkingCursor();
            m_ScrollView.Add(m_InputField);

            m_ResizeHandle = new VisualElement
            {
                name = resizeHandleUssClassName,
                pickingMode = PickingMode.Position,
            };
            m_ResizeHandle.AddToClassList(resizeHandleUssClassName);
            hierarchy.Add(m_ResizeHandle);
            var dragManipulator = new Draggable(null, OnDrag, null);
            m_ResizeHandle.AddManipulator(dragManipulator);

            SetValueWithoutNotify(value);
            m_InputField.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
            m_Placeholder.RegisterValueChangedCallback(OnPlaceholderValueChanged);
        }

        void OnPlaceholderValueChanged(ChangeEvent<string> evt)
        {
            evt.PreventDefault();
            evt.StopPropagation();
        }

        void OnDrag(Draggable draggable)
        {
            style.height = Mathf.Max(resolvedStyle.minHeight.value, resolvedStyle.height + draggable.deltaPos.y);
        }

        /// <summary>
        /// The content container of the TextArea.
        /// </summary>
        public override VisualElement contentContainer => m_InputField.contentContainer;

        /// <summary>
        /// The TextArea placeholder text.
        /// </summary>
        public string placeholder
        {
            get => m_Placeholder.text;
            set => m_Placeholder.text = value;
        }

        /// <summary>
        /// The validation function for the TextArea.
        /// </summary>
        public Func<string, bool> validateValue { get; set; }

        /// <summary>
        /// The invalid state of the TextArea.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// Set the TextArea value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the TextArea. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue;
            m_InputField.SetValueWithoutNotify(m_Value);
            RefreshUI();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The TextArea value.
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
            Debug.Log("OnFocusedIn");
            AddToClassList(Styles.focusedUssClassName);
            m_Placeholder.AddToClassList(Styles.hiddenUssClassName);
            passMask = 0;
        }

        void OnKeyboardFocusedIn(FocusInEvent evt)
        {
            Debug.Log("OnKeyboardFocusedIn");
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
        /// Factory class to instantiate a <see cref="TextArea"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<TextArea, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="TextArea"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new()
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_Placeholder = new()
            {
                name = "placeholder",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Value = new()
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

                var el = (TextArea)ve;

                el.placeholder = m_Placeholder.GetValueFromBag(bag, cc);
                el.value = m_Value.GetValueFromBag(bag, cc);
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
