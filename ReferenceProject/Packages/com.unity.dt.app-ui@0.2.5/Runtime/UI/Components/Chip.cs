using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Chip UI element.
    /// </summary>
    public class Chip : VisualElement
    {
        /// <summary>
        /// The possible variants for a <see cref="Chip"/>.
        /// </summary>
        public enum Variant
        {
            /// <summary>
            /// The <see cref="Chip"/> is displayed with a fill color.
            /// </summary>
            Filled,
            /// <summary>
            /// The <see cref="Chip"/> is displayed with an outline.
            /// </summary>
            Outlined,
        }

        const string k_DefaultDeleteIconName = "x";

        /// <summary>
        /// The Chip main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-chip";

        /// <summary>
        /// The Chip variant styling class.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The Chip Clickable variant styling class.
        /// </summary>
        public static readonly string clickableUssClassName = ussClassName + "--clickable";

        /// <summary>
        /// The Chip Deletable variant styling class.
        /// </summary>
        public static readonly string deletableUssClassName = ussClassName + "--deletable";

        /// <summary>
        /// The Chip label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The Chip ornament container styling class.
        /// </summary>
        public static readonly string ornamentContainerUssClassName = ussClassName + "__ornament-container";

        /// <summary>
        /// The Chip delete Button styling class.
        /// </summary>
        public static readonly string deleteButtonUssClassName = ussClassName + "__delete-button";

        /// <summary>
        /// The Chip delete Icon styling class.
        /// </summary>
        public static readonly string deleteIconUssClassName = ussClassName + "__delete-icon";

        static readonly CustomStyleProperty<Color> k_UssColor = new CustomStyleProperty<Color>("--chip-color");

        static readonly CustomStyleProperty<Color> k_UssBgColor = new CustomStyleProperty<Color>("--chip-background-color");

        readonly VisualElement m_DeleteButton;

        readonly Icon m_DeleteIcon;

        Clickable m_Clickable;

        readonly LocalizedTextElement m_Label;

        Variant m_Variant;

        EventHandler m_Clicked;

        EventHandler m_Deleted;

        VisualElement m_Ornament;

        readonly VisualElement m_OrnamentContainer;

        /// <summary>
        /// The content container of the Chip. This is the ornament container.
        /// </summary>
        public override VisualElement contentContainer => m_OrnamentContainer;

        /// <summary>
        /// The icon name for the delete button.
        /// </summary>
        public string deleteIcon
        {
            get => m_DeleteIcon.iconName;
            set => m_DeleteIcon.iconName = value;
        }

        /// <summary>
        /// The Chip variant.
        /// </summary>
        public Variant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(variantUssClassName + m_Variant.ToString().ToLower());
            }
        }

        /// <summary>
        /// The Chip ornament.
        /// </summary>
        public VisualElement ornament
        {
            get => m_Ornament;
            set
            {
                if (m_Ornament != null && m_Ornament.parent == m_OrnamentContainer)
                    m_OrnamentContainer.Remove(m_Ornament);
                m_Ornament = value;
                if (m_Ornament != null)
                    m_OrnamentContainer.Add(m_Ornament);
            }
        }

        /// <summary>
        /// The Chip label.
        /// </summary>
        public string label
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        /// <summary>
        /// Clickable Manipulator for this Chip.
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
        /// Event fired when the Chip is clicked.
        /// </summary>
        public event EventHandler clicked
        {
            add
            {
                if (m_Clicked == null && value != null)
                    AddToClassList(clickableUssClassName);
                m_Clicked += value;
            }
            remove
            {
                m_Clicked -= value;
                if (m_Clicked == null)
                    RemoveFromClassList(clickableUssClassName);
            }
        }

        /// <summary>
        /// Event fired when the Chip is deleted.
        /// </summary>
        public event EventHandler deleted
        {
            add
            {
                if (m_Deleted == null && value != null)
                    AddToClassList(deletableUssClassName);
                m_Deleted += value;
            }
            remove
            {
                m_Deleted -= value;
                if (m_Deleted == null)
                    RemoveFromClassList(deletableUssClassName);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Chip()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            clickable = new Submittable(OnClickedInternal);
            focusable = true;
            tabIndex = 0;

            m_OrnamentContainer = new VisualElement { name = ornamentContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_OrnamentContainer.AddToClassList(ornamentContainerUssClassName);
            hierarchy.Add(m_OrnamentContainer);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            m_DeleteButton = new VisualElement { name = deleteButtonUssClassName, pickingMode = PickingMode.Position, focusable = true };
            m_DeleteButton.AddToClassList(deleteButtonUssClassName);
            m_DeleteButton.AddManipulator(new Submittable(OnDeletedInternal));
            hierarchy.Add(m_DeleteButton);

            m_DeleteIcon = new Icon { name = deleteIconUssClassName, pickingMode = PickingMode.Ignore };
            m_DeleteIcon.AddToClassList(deleteIconUssClassName);
            m_DeleteButton.hierarchy.Add(m_DeleteIcon);

            deleteIcon = k_DefaultDeleteIconName;
            variant = Variant.Filled;
            ornament = null;
        }

        void OnDeletedInternal()
        {
            m_Deleted?.Invoke(this, EventArgs.Empty);
        }

        void OnClickedInternal()
        {
            m_Clicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Defines the UxmlFactory for the Chip.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Chip, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Chip"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<Variant> m_Variant = new UxmlEnumAttributeDescription<Variant>
            {
                name = "variant",
                defaultValue = Variant.Filled
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_DeleteIcon = new UxmlStringAttributeDescription
            {
                name = "delete-icon",
                defaultValue = k_DefaultDeleteIconName
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

                var element = (Chip)ve;
                element.variant = m_Variant.GetValueFromBag(bag, cc);
                element.label = m_Label.GetValueFromBag(bag, cc);
                element.deleteIcon = m_DeleteIcon.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
