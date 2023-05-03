using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Badge UI element.
    /// </summary>
    public class Badge : VisualElement
    {
        /// <summary>
        /// The Badge main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-badge";

        /// <summary>
        /// The Badge label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        readonly LocalizedTextElement m_LabelElement;

        /// <summary>
        /// The content container of the Badge. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The background color of the Badge.
        /// </summary>
        public Color backgroundColor
        {
            get => resolvedStyle.backgroundColor;
            set => style.backgroundColor = value;
        }

        /// <summary>
        /// The text color of the Badge.
        /// </summary>
        public Color textColor
        {
            get => m_LabelElement.resolvedStyle.color;
            set => m_LabelElement.style.color = value;
        }

        /// <summary>
        /// The text of the Badge.
        /// </summary>
        public string text
        {
            get => m_LabelElement.text;
            set
            {
                m_LabelElement.text = value;
                m_LabelElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Badge()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_LabelElement = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);
            hierarchy.Add(m_LabelElement);

            text = null;
            backgroundColor = new Color(1, 0.3f, 0.3f);
            textColor = Color.white;
        }

        /// <summary>
        /// Defines the UxmlFactory for the Badge.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Badge, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Badge"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
            {
                name = "text",
                defaultValue = null
            };

            readonly UxmlColorAttributeDescription m_TextColor = new UxmlColorAttributeDescription
            {
                name = "text-color",
                defaultValue = Color.white
            };

            readonly UxmlColorAttributeDescription m_BackgroundColor = new UxmlColorAttributeDescription
            {
                name = "background-color",
                defaultValue = new Color(1, 0.3f, 0.3f)
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

                var el = (Badge)ve;
                el.text = m_Text.GetValueFromBag(bag, cc);
                el.textColor = m_TextColor.GetValueFromBag(bag, cc);
                el.backgroundColor = m_BackgroundColor.GetValueFromBag(bag, cc);
            }
        }
    }
}
