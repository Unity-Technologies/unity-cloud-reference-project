using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Stepper UI element.
    /// </summary>
    public class Stepper : ExVisualElement, INotifyValueChanged<int>
    {
        /// <summary>
        /// The Stepper main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-stepper";

        /// <summary>
        /// The Stepper size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Stepper increment icon styling class.
        /// </summary>
        public static readonly string incIconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The Stepper decrement icon styling class.
        /// </summary>
        public static readonly string decIconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The Stepper increment icon container styling class.
        /// </summary>
        public static readonly string incIconContainerUssClassName = ussClassName + "__iconcontainer";

        /// <summary>
        /// The Stepper decrement icon container styling class.
        /// </summary>
        public static readonly string decIconContainerUssClassName = ussClassName + "__iconcontainer";

        /// <summary>
        /// The Stepper decrement button styling class.
        /// </summary>
        public static readonly string decButtonUssClassName = ussClassName + "__decbutton";

        /// <summary>
        /// The Stepper increment button styling class.
        /// </summary>
        public static readonly string incButtonUssClassName = ussClassName + "__incbutton";

        /// <summary>
        /// The Stepper general button styling class.
        /// </summary>
        public static readonly string buttonUssClassName = ussClassName + "__button";

        Size m_Size;

        int m_Value;

        readonly Clickable m_DecClickable;

        readonly Clickable m_IncClickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Stepper()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;

            var decIcon = new Icon { name = decIconUssClassName, iconName = "minus", pickingMode = PickingMode.Ignore };
            decIcon.AddToClassList(decIconUssClassName);
            var incIcon = new Icon { name = incIconUssClassName, iconName = "plus", pickingMode = PickingMode.Ignore };
            incIcon.AddToClassList(incIconUssClassName);

            var decIconContainer = new VisualElement { name = decIconContainerUssClassName, pickingMode = PickingMode.Ignore };
            decIconContainer.AddToClassList(decIconContainerUssClassName);
            var incIconContainer = new VisualElement { name = incIconContainerUssClassName, pickingMode = PickingMode.Ignore };
            incIconContainer.AddToClassList(incIconContainerUssClassName);

            var decButton = new VisualElement { name = decButtonUssClassName };
            decButton.AddToClassList(buttonUssClassName);
            decButton.AddToClassList(decButtonUssClassName);
            var incButton = new VisualElement { name = incButtonUssClassName };
            incButton.AddToClassList(Styles.lastChildUssClassName); // todo use :last-child state in the USS when available
            incButton.AddToClassList(buttonUssClassName);
            incButton.AddToClassList(incButtonUssClassName);

            decIconContainer.hierarchy.Add(decIcon);
            incIconContainer.hierarchy.Add(incIcon);

            decButton.hierarchy.Add(decIconContainer);
            incButton.hierarchy.Add(incIconContainer);

            hierarchy.Add(decButton);
            hierarchy.Add(incButton);

            m_DecClickable = new Clickable(OnDecrementClicked);
            decButton.AddManipulator(m_DecClickable);
            m_IncClickable = new Clickable(OnIncrementClicked);
            incButton.AddManipulator(m_IncClickable);

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            size = Size.M;
        }

        /// <summary>
        /// The content container of the Stepper. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Stepper.
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
        /// Set the value of the Stepper without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            m_Value = newValue;
        }

        /// <summary>
        /// The value of the Stepper. 1 means increment, -1 means decrement.
        /// <para /> It is not recommended to get or set this value directly.
        /// To track the changes of the value, use <see cref="INotifyValueChangedExtensions.RegisterValueChangedCallback{T}"/> instead.
        /// </summary>
        public int value
        {
            get => m_Value;
            set
            {
                using var evt = ChangeEvent<int>.GetPooled(0, value);
                evt.target = this;
                SetValueWithoutNotify(value);
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

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target == this)
            {
                var handled = false;

                if (evt.keyCode == KeyCode.Plus || evt.keyCode == KeyCode.KeypadPlus || evt.keyCode == KeyCode.RightArrow)
                {
                    m_IncClickable.SimulateSingleClickInternal(evt);
                    handled = true;
                }
                else if (evt.keyCode == KeyCode.Minus || evt.keyCode == KeyCode.KeypadMinus || evt.keyCode == KeyCode.LeftArrow)
                {
                    m_DecClickable.SimulateSingleClickInternal(evt);
                    handled = true;
                }

                if (handled)
                {
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
            }
        }

        void OnIncrementClicked()
        {
            value = 1;
        }

        void OnDecrementClicked()
        {
            value = -1;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Stepper"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Stepper, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Stepper"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
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

                var el = (Stepper)ve;
                el.size = m_Size.GetValueFromBag(bag, cc);
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
