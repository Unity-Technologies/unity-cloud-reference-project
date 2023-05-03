using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// ActionButton UI element.
    /// </summary>
    public class ActionButton : VisualElement, ISizeableElement, ISelectableElement, IClickable
    {
        /// <summary>
        /// The ActionButton main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-actionbutton";

        /// <summary>
        /// The ActionButton icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The ActionButton label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The ActionButton icon and label variant styling class.
        /// </summary>
        public static readonly string iconAndLabelUssClassName = ussClassName + "--icon-and-label";

        /// <summary>
        /// The ActionButton icon only variant styling class.
        /// </summary>
        public static readonly string iconOnlyUssClassName = ussClassName + "--icon-only";

        /// <summary>
        /// The ActionButton quiet variant styling class.
        /// </summary>
        public static readonly string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The ActionButton size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ActionButton accent styling class.
        /// </summary>
        public static readonly string accentUssClassName = ussClassName + "--accent";

        readonly Icon m_IconElement;

        readonly LocalizedTextElement m_LabelElement;

        Size m_Size;

        Clickable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionButton() : this(null) { }

        /// <summary>
        /// Construct a <see cref="ActionButton"/> with a given click event callback.
        /// </summary>
        /// <param name="clickEvent">THe given click event callback.</param>
        public ActionButton(Action clickEvent)
        {
            AddToClassList(ussClassName);

            clickable = new Submittable(clickEvent);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            m_IconElement = new Icon { name = iconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            m_IconElement.AddToClassList(iconUssClassName);
            m_LabelElement = new LocalizedTextElement { name = labelUssClassName, text = null, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);

            hierarchy.Add(m_IconElement);
            hierarchy.Add(m_LabelElement);

            size = Size.M;
            accent = false;
            quiet = false;

            Refresh();
        }

        /// <summary>
        /// Clickable Manipulator for this ActionButton.
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
        /// The ActionButton label.
        /// </summary>
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                m_LabelElement.text = value;
                Refresh();
            }
        }

        /// <summary>
        /// The ActionButton icon.
        /// </summary>
        public string icon
        {
            get => m_IconElement.iconName;
            set
            {
                m_IconElement.iconName = value;
                Refresh();
            }
        }

        /// <summary>
        /// The selected state of the ActionButton.
        /// </summary>
        public bool selected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set => SetSelectedWithoutNotify(value);
        }

        /// <summary>
        /// The quiet state of the ActionButton.
        /// </summary>
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set => EnableInClassList(quietUssClassName, value);
        }

        /// <summary>
        /// The accent variant of the ActionButton.
        /// </summary>
        public bool accent
        {
            get => ClassListContains(accentUssClassName);
            set => EnableInClassList(accentUssClassName, value);
        }

        /// <summary>
        /// The content container of the ActionButton.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The current size of the ActionButton.
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
        /// Set the selected state of the ActionButton without notifying the click event.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.selectedUssClassName, newValue);
        }

        void Refresh()
        {
            EnableInClassList(iconAndLabelUssClassName, !string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(label));
            EnableInClassList(iconOnlyUssClassName, !string.IsNullOrEmpty(icon) && string.IsNullOrEmpty(label));
            m_LabelElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(label));
            m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(icon));
        }

        /// <summary>
        /// The ActionButton UXML factory.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ActionButton, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ActionButton"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_Icon = new UxmlStringAttributeDescription
            {
                name = "icon",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null
            };

            readonly UxmlBoolAttributeDescription m_Quiet = new UxmlBoolAttributeDescription
            {
                name = "quiet",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Selected = new UxmlBoolAttributeDescription
            {
                name = "selected",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Accent = new UxmlBoolAttributeDescription
            {
                name = "accent",
                defaultValue = false,
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

                var el = (ActionButton)ve;
                el.label = m_Label.GetValueFromBag(bag, cc);
                el.icon = m_Icon.GetValueFromBag(bag, cc);
                el.size = m_Size.GetValueFromBag(bag, cc);
                el.accent = m_Accent.GetValueFromBag(bag, cc);
                el.selected = m_Selected.GetValueFromBag(bag, cc);
                el.quiet = m_Quiet.GetValueFromBag(bag, cc);
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
