using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Item used inside an <see cref="Accordion"/> element.
    /// </summary>
    public class AccordionItem : VisualElement, INotifyValueChanged<bool>
    {
        /// <summary>
        /// The AccordionItem main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-accordionitem";

        /// <summary>
        /// The AccordionItem content styling class.
        /// </summary>
        public static readonly string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The AccordionItem header styling class.
        /// </summary>
        public static readonly string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The AccordionItem headertext styling class.
        /// </summary>
        public static readonly string headerTextUssClassName = ussClassName + "__headertext";

        /// <summary>
        /// The AccordionItem indicator styling class.
        /// </summary>
        public static readonly string indicatorUssClassName = ussClassName + "__indicator";

        /// <summary>
        /// The AccordionItem heading styling class.
        /// </summary>
        public static readonly string headingUssClassName = ussClassName + "__heading";

        readonly VisualElement m_ContentElement;

        readonly LocalizedTextElement m_HeaderTextElement;

        readonly Clickable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AccordionItem()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            m_HeaderTextElement = new LocalizedTextElement { name = headerTextUssClassName, pickingMode = PickingMode.Ignore };
            m_HeaderTextElement.AddToClassList(headerTextUssClassName);

            var headerIndicatorElement = new Icon { name = indicatorUssClassName, iconName = "caret-down", pickingMode = PickingMode.Ignore };
            headerIndicatorElement.AddToClassList(indicatorUssClassName);

            var headerElement = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Position, focusable = true };
            headerElement.AddToClassList(headerUssClassName);
            m_Clickable = new Submittable(OnClicked);
            headerElement.AddManipulator(m_Clickable);
            headerElement.AddManipulator(new KeyboardFocusController());
            headerElement.hierarchy.Add(m_HeaderTextElement);
            headerElement.hierarchy.Add(headerIndicatorElement);

            var headingElement = new VisualElement { pickingMode = PickingMode.Ignore };
            headingElement.AddToClassList(headingUssClassName);
            headingElement.hierarchy.Add(headerElement);

            m_ContentElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_ContentElement.AddToClassList(contentUssClassName);

            hierarchy.Add(headingElement);
            hierarchy.Add(m_ContentElement);

            SetValueWithoutNotify(false);
        }

        /// <summary>
        /// The content container of the AccordionItem.
        /// </summary>
        public override VisualElement contentContainer => m_ContentElement;

        /// <summary>
        /// The title of the AccordionItem.
        /// </summary>
        public string title
        {
            get => m_HeaderTextElement.text;
            set => m_HeaderTextElement.text = value;
        }

        /// <summary>
        /// The value of the item, which represents its open state.
        /// </summary>
        public bool value
        {
            get => ClassListContains(Styles.openUssClassName);
            set
            {
                var previousValue = ClassListContains(Styles.openUssClassName);
                if (previousValue == value)
                    return;
                using var evt = ChangeEvent<bool>.GetPooled(previousValue, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Set the open state of the item without triggering any event.
        /// </summary>
        /// <param name="newValue">The new open state of the item.</param>
        public void SetValueWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.openUssClassName, newValue);
        }

        void OnClicked()
        {
            value = !value;
        }

        /// <summary>
        /// Class to be able to use the <see cref="AccordionItem"/> in UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<AccordionItem, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="AccordionItem"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_Title = new UxmlStringAttributeDescription
            {
                name = "title",
                defaultValue = "Header",
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);

                var element = (AccordionItem)ve;
                element.title = m_Title.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }

    /// <summary>
    /// Accordion UI element.
    /// </summary>
    public class Accordion : VisualElement
    {
        /// <summary>
        /// The Accordion main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-accordion";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Accordion()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
        }

        /// <summary>
        /// The UXML factory for the Accordion.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Accordion, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Accordion"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);
            }
        }
    }
}
