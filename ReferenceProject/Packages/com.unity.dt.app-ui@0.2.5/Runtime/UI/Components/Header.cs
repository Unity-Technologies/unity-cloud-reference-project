using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Header sizing.
    /// </summary>
    public enum HeaderSize
    {
        /// <summary>
        /// Double Extra-small
        /// </summary>
        XXS,

        /// <summary>
        /// Extra-small
        /// </summary>
        XS,

        /// <summary>
        /// Small
        /// </summary>
        S,

        /// <summary>
        /// Medium
        /// </summary>
        M,

        /// <summary>
        /// Large
        /// </summary>
        L,

        /// <summary>
        /// Extra-large
        /// </summary>
        XL,

        /// <summary>
        /// Double Extra-large
        /// </summary>
        XXL,
    }

    /// <summary>
    /// Header UI element.
    /// </summary>
    public sealed class Header : LocalizedTextElement
    {
        /// <summary>
        /// The Header main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-header";

        /// <summary>
        /// The Header primary variant styling class.
        /// </summary>
        public static readonly string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The Header size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        HeaderSize m_Size = HeaderSize.M;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Header()
            : this(string.Empty) { }

        /// <summary>
        /// Construct a Header UI element with a provided text to display.
        /// </summary>
        /// <param name="text">The text that will be displayed.</param>
        public Header(string text)
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Position; // in case we want a tooltip

            this.text = text;
            primary = true;
            size = HeaderSize.M;
        }

        /// <summary>
        /// The primary variant of the Header.
        /// </summary>
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set => EnableInClassList(primaryUssClassName, value);
        }

        /// <summary>
        /// The size of the Header.
        /// </summary>
        public HeaderSize size
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
        /// Factory class to instantiate a <see cref="Header"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Header, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Header"/>.
        /// </summary>
        public new class UxmlTraits : LocalizedTextElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Primary = new UxmlBoolAttributeDescription
            {
                name = "primary",
                defaultValue = true,
            };

            readonly UxmlEnumAttributeDescription<HeaderSize> m_Size = new UxmlEnumAttributeDescription<HeaderSize>
            {
                name = "size",
                defaultValue = HeaderSize.M,
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

                var element = (Header)ve;
                element.primary = m_Primary.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
