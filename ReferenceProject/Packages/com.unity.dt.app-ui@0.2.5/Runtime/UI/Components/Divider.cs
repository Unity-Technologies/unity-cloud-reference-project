using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Divider UI element.
    /// </summary>
    public class Divider : VisualElement
    {
        /// <summary>
        /// The Divider main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-divider";

        /// <summary>
        /// The Divider size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Divider spacing styling class.
        /// </summary>
        public static readonly string spacingUssClassName = ussClassName + "--spacing-";

        /// <summary>
        /// The Divider vertical mode styling class.
        /// </summary>
        public static readonly string verticalUssClassName = ussClassName + "--vertical";

        /// <summary>
        /// The Divider content styling class.
        /// </summary>
        public static readonly string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The content container of the Divider. This is always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        Size m_Size;

        Spacing m_Spacing;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Divider()
        {
            AddToClassList(ussClassName);

            var content = new VisualElement { name = contentUssClassName, pickingMode = PickingMode.Ignore };
            content.AddToClassList(contentUssClassName);
            hierarchy.Add(content);

            pickingMode = PickingMode.Ignore;

            size = Size.M;
            spacing = Spacing.M;
            vertical = false;
        }

        /// <summary>
        /// The orientation of the Divider.
        /// </summary>
        public bool vertical
        {
            get => ClassListContains(verticalUssClassName);
            set => EnableInClassList(verticalUssClassName, value);
        }

        /// <summary>
        /// The size of the Divider.
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
        /// The spacing of the Divider.
        /// </summary>
        public Spacing spacing
        {
            get => m_Spacing;
            set
            {
                RemoveFromClassList(spacingUssClassName + m_Spacing.ToString().ToLower());
                m_Spacing = value;
                AddToClassList(spacingUssClassName + m_Spacing.ToString().ToLower());
            }
        }

        /// <summary>
        /// The UXML factory for the Divider.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Divider, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Divider"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlEnumAttributeDescription<Spacing> m_Spacing = new UxmlEnumAttributeDescription<Spacing>
            {
                name = "spacing",
                defaultValue = Spacing.M,
            };

            readonly UxmlBoolAttributeDescription m_Vertical = new UxmlBoolAttributeDescription
            {
                name = "vertical",
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
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);

                var element = (Divider)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.vertical = m_Vertical.GetValueFromBag(bag, cc);
                element.spacing = m_Spacing.GetValueFromBag(bag, cc);
            }
        }
    }
}
