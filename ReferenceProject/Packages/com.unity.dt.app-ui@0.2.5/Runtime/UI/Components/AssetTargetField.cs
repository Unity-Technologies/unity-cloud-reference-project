using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// AssetTarget Field UI element.
    /// </summary>
    // todo This has to work with an AssetReferencePicker
    class AssetTargetField : VisualElement, IValidatableElement<AssetReference>, ISizeableElement
    {
        const string k_DefaultIconName = "scene";

        /// <summary>
        /// The AssetTargetField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-assettargetfield";

        /// <summary>
        /// The AssetTargetField icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The AssetTargetField label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The AssetTargetField type label styling class.
        /// </summary>
        public static readonly string typeLabelUssClassName = ussClassName + "__typelabel";

        /// <summary>
        /// The AssetTargetField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly Icon m_IconElement;

        readonly LocalizedTextElement m_LabelElement;

        readonly LocalizedTextElement m_TypeLabelElement;

        AssetReference m_AssetReference;

        Size m_Size;

        Type m_Type;

        Clickable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AssetTargetField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            clickable = new Submittable((Action)null);

            m_IconElement = new Icon
            {
                name = iconUssClassName,
                pickingMode = PickingMode.Ignore,
                iconName = k_DefaultIconName
            };
            m_IconElement.AddToClassList(iconUssClassName);

            m_LabelElement = new LocalizedTextElement
            {
                name = labelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_LabelElement.AddToClassList(labelUssClassName);

            m_TypeLabelElement = new LocalizedTextElement
            {
                name = typeLabelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_TypeLabelElement.AddToClassList(typeLabelUssClassName);

            hierarchy.Add(m_IconElement);
            hierarchy.Add(m_LabelElement);
            hierarchy.Add(m_TypeLabelElement);

            size = Size.M;
            type = typeof(GameObject);
            SetValueWithoutNotify(null);
        }

        public override VisualElement contentContainer => null;

        /// <summary>
        /// Clickable Manipulator for this AssetTargetField.
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

        public Type type
        {
            get => m_Type;
            set
            {
                m_Type = value;
                if (m_AssetReference != null && m_Type != null)
                {
                    var valueType = m_AssetReference.GetType();
                    if (!m_Type.IsAssignableFrom(valueType))
                        this.value = null;
                }

                m_IconElement.iconName = m_Type?.Name.ToLower();
                m_TypeLabelElement.text = m_Type?.Name.ToUpper();
            }
        }

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

        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }

        public Func<AssetReference, bool> validateValue { get; set; }

        public void SetValueWithoutNotify(AssetReference newValue)
        {
            m_AssetReference = newValue;
            m_LabelElement.text = m_AssetReference?.name ?? "<None>";
            if (validateValue != null) invalid = !validateValue(m_AssetReference);
        }

        public AssetReference value
        {
            get => m_AssetReference;
            set
            {
                if (m_AssetReference == value)
                    return;
                using var evt = ChangeEvent<AssetReference>.GetPooled(m_AssetReference, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }


        [Preserve]
        public new class UxmlFactory : UxmlFactory<AssetTargetField, UxmlTraits> { }


        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="AssetTargetField"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Invalid = new UxmlBoolAttributeDescription
            {
                name = "invalid",
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

                var element = (AssetTargetField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.invalid = m_Invalid.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
