using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A container for a set of <see cref="Radio"/> UI elements.
    /// </summary>
    public class RadioGroup : VisualElement, INotifyValueChanged<int>
    {
        /// <summary>
        /// The RadioGroup main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-radiogroup";

        Action<Radio, int> m_BindItem;

        IList m_Items;

        int m_Value = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RadioGroup()
        {
            AddToClassList(ussClassName);
            RegisterCallback<ChangeEvent<bool>>(OnItemChosen);
        }

        /// <summary>
        /// Construct a RadioGroup UI element using a provided collection of items.
        /// </summary>
        /// <param name="items">A collection of items that will be displayed as Radio component.</param>
        /// <param name="bindItem">A function invoked to bind display data per item.</param>
        public RadioGroup(IList items, Action<Radio, int> bindItem = null)
            : this()
        {
            sourceItems = items;
            this.bindItem = bindItem;
        }

        /// <summary>
        /// The function invoked to bind display data per item.
        /// </summary>
        public Action<Radio, int> bindItem
        {
            get => m_BindItem;
            set
            {
                m_BindItem = value;
                if (sourceItems != null)
                    Refresh();
            }
        }

        /// <summary>
        /// The RadioGroup content container.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The collection of items that will be displayed as Radio component.
        /// </summary>
        public IList sourceItems
        {
            get => m_Items;
            set
            {
                m_Items = value;
                Refresh();
            }
        }

        /// <summary>
        /// The selected item index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> if the value is out of range.</exception>
        public int value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;
                if (value < -1 || value >= childCount)
                    throw new ArgumentOutOfRangeException(nameof(value));
                using var evt = ChangeEvent<int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Set the value without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value.</param>
        public void SetValueWithoutNotify(int newValue)
        {
            for (var i = 0; i < childCount; i++)
            {
                if (ElementAt(i) is Radio r)
                    r.SetValueWithoutNotify(i == newValue);
            }

            m_Value = newValue;
        }

        void Refresh()
        {
            Clear();
            var newValue = -1;
            if (m_Items is { Count: > 0 })
            {
                for (var i = 0; i < m_Items.Count; i++)
                {
                    var item = new Radio();
                    if (bindItem != null)
                        bindItem.Invoke(item, i);
                    else
                        item.label = m_Items[i].ToString();
                    Add(item);
                }

                newValue = 0;
            }

            value = newValue;
            // if the value is the same as before, there won't be any refresh so we call SetValueWithoutNotify explicitly
            SetValueWithoutNotify(newValue);
        }

        void OnItemChosen(ChangeEvent<bool> evt)
        {
            if (evt.target is Radio radio && radio.parent == this && evt.newValue)
            {
                var newIndex = IndexOf(radio);
                value = newIndex;
            }
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="RadioGroup"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<RadioGroup, UxmlTraits> { }
    }

    /// <summary>
    /// Radio UI element.
    /// </summary>
    public class Radio : VisualElement, IValidatableElement<bool>
    {
        /// <summary>
        /// The Radio main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-radio";

        /// <summary>
        /// The Radio size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Radio emphasized mode styling class.
        /// </summary>
        public static readonly string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Radio button styling class.
        /// </summary>
        public static readonly string boxUssClassName = ussClassName + "__button";

        /// <summary>
        /// The Radio checkmark styling class.
        /// </summary>
        public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Radio label styling class.
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
        public Radio()
        {
            AddToClassList(ussClassName);

            clickable = new Submittable(OnClick);
            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;

            var radioIcon = new Icon { name = checkmarkUssClassName, iconName = "radio-bullet", pickingMode = PickingMode.Ignore };
            radioIcon.AddToClassList(checkmarkUssClassName);
            m_Box = new ExVisualElement { name = boxUssClassName, pickingMode = PickingMode.Ignore, passMask = 0 };
            m_Box.AddToClassList(boxUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_Box.hierarchy.Add(radioIcon);
            hierarchy.Add(m_Box);
            hierarchy.Add(m_Label);

            size = Size.M;
            emphasized = false;
            invalid = false;
            SetValueWithoutNotify(false);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <summary>
        /// Clickable Manipulator for this Radio.
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
        /// The Radio size.
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
        /// The Radio emphasized mode.
        /// </summary>
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set => EnableInClassList(emphasizedUssClassName, value);
        }

        /// <summary>
        /// The Radio label.
        /// </summary>
        public string label
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        /// <summary>
        /// The Radio invalid state.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        /// <summary>
        /// The Radio validation function.
        /// </summary>
        public Func<bool, bool> validateValue { get; set; }

        /// <summary>
        /// Sets the Radio value without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(bool newValue)
        {
            m_Value = newValue;
            EnableInClassList(Styles.checkedUssClassName, m_Value);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The Radio value.
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

        void OnClick()
        {
            value = true;
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Radio"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Radio, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Radio"/>.
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

                var element = (Radio)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.emphasized = m_Emphasized.GetValueFromBag(bag, cc);
                element.value = m_Value.GetValueFromBag(bag, cc);
                element.label = m_Label.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
