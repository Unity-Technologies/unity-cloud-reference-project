using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The text overflow mode.
    /// </summary>
    public enum TextOverflow
    {
        /// <summary>
        /// The text will be truncated with an ellipsis.
        /// </summary>
        Ellipsis,
        /// <summary>
        /// The text won't be truncated.
        /// </summary>
        Normal,
    }

    /// <summary>
    /// InputLabel UI element.
    /// </summary>
    public class InputLabel : VisualElement
    {
        /// <summary>
        /// The InputLabel main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-inputlabel";

        /// <summary>
        /// The InputLabel size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The InputLabel direction styling class.
        /// </summary>
        public static readonly string orientationUssClassName = ussClassName + "--";

        /// <summary>
        /// The InputLabel container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The InputLabel label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The InputLabel label overflow styling class.
        /// </summary>
        public static readonly string labelOverflowUssClassName = ussClassName + "--label-overflow-";

        /// <summary>
        /// The InputLabel input alignment styling class.
        /// </summary>
        public static readonly string inputAlignmentUssClassName = ussClassName + "--input-alignment-";

        TextSize m_Size = TextSize.M;

        readonly Text m_LabelElement;

        readonly VisualElement m_Container;

        Direction m_Direction = Direction.Horizontal;

        TextOverflow m_LabelOverflow = TextOverflow.Ellipsis;

        Align m_InputAlignment = Align.Stretch;

        /// <summary>
        /// The content container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The label value.
        /// </summary>
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                m_LabelElement.text = value;
                m_LabelElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// The size of the label.
        /// </summary>
        public TextSize size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                m_LabelElement.size = m_Size;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
            }
        }

        /// <summary>
        /// The orientation of the label.
        /// </summary>
        public Direction direction
        {
            get => m_Direction;
            set
            {
                RemoveFromClassList(orientationUssClassName + m_Direction.ToString().ToLower());
                m_Direction = value;
                AddToClassList(orientationUssClassName + m_Direction.ToString().ToLower());
            }
        }

        /// <summary>
        /// The text overflow mode.
        /// </summary>
        public TextOverflow labelOverflow
        {
            get => m_LabelOverflow;
            set
            {
                RemoveFromClassList(labelOverflowUssClassName + m_LabelOverflow.ToString().ToLower());
                m_LabelOverflow = value;
                AddToClassList(labelOverflowUssClassName + m_LabelOverflow.ToString().ToLower());
            }
        }

        /// <summary>
        /// The alignment of the input.
        /// </summary>
        public Align inputAlignment
        {
            get => m_InputAlignment;
            set
            {
                RemoveFromClassList(inputAlignmentUssClassName + m_InputAlignment.ToString().ToLower());
                m_InputAlignment = value;
                AddToClassList(inputAlignmentUssClassName + m_InputAlignment.ToString().ToLower());
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InputLabel()
            : this(null)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label"> The label value. </param>
        public InputLabel(string label)
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_LabelElement = new Text { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);
            hierarchy.Add(m_LabelElement);

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            this.label = label;
            size = TextSize.S;
            direction = Direction.Horizontal;
            inputAlignment = Align.Stretch;
            labelOverflow = TextOverflow.Ellipsis;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="InputLabel"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<InputLabel, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="InputLabel"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlEnumAttributeDescription<TextSize> m_Size = new UxmlEnumAttributeDescription<TextSize>
            {
                name = "size",
                defaultValue = TextSize.S,
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null,
            };

            readonly UxmlEnumAttributeDescription<Direction> m_Orientation = new UxmlEnumAttributeDescription<Direction>
            {
                name = "direction",
                defaultValue = Direction.Horizontal,
            };

            readonly UxmlEnumAttributeDescription<TextOverflow> m_LabelOverflow = new UxmlEnumAttributeDescription<TextOverflow>
            {
                name = "label-overflow",
                defaultValue = TextOverflow.Ellipsis,
            };

            readonly UxmlEnumAttributeDescription<Align> m_InputAlignment = new UxmlEnumAttributeDescription<Align>
            {
                name = "input-alignment",
                defaultValue = Align.Stretch,
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

                var element = (InputLabel)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.direction = m_Orientation.GetValueFromBag(bag, cc);
                element.inputAlignment = m_InputAlignment.GetValueFromBag(bag, cc);
                element.labelOverflow = m_LabelOverflow.GetValueFromBag(bag, cc);
                element.label = m_Label.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
