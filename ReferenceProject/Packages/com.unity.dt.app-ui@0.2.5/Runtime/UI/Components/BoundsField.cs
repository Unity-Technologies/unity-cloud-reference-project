using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Bounds Field UI element.
    /// </summary>
    public class BoundsField : VisualElement, IValidatableElement<Bounds>, ISizeableElement
    {
        /// <summary>
        /// The BoundsField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-boundsfield";

        /// <summary>
        /// The BoundsField row styling class.
        /// </summary>
        public static readonly string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The BoundsField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The BoundsField X NumericalField styling class.
        /// </summary>
        public static readonly string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The BoundsField Y NumericalField styling class.
        /// </summary>
        public static readonly string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The BoundsField Z NumericalField styling class.
        /// </summary>
        public static readonly string zFieldUssClassName = ussClassName + "__z-field";

        /// <summary>
        /// The BoundsField X NumericalField styling class.
        /// </summary>
        public static readonly string sxFieldUssClassName = ussClassName + "__sx-field";

        /// <summary>
        /// The BoundsField Y NumericalField styling class.
        /// </summary>
        public static readonly string syFieldUssClassName = ussClassName + "__sy-field";

        /// <summary>
        /// The BoundsField Z NumericalField styling class.
        /// </summary>
        public static readonly string szFieldUssClassName = ussClassName + "__sz-field";

        /// <summary>
        /// The BoundsField Label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        Bounds m_Value;

        readonly FloatField m_CXField;

        readonly FloatField m_CYField;

        readonly FloatField m_CZField;

        readonly FloatField m_SXField;

        readonly FloatField m_SYField;

        readonly FloatField m_SZField;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BoundsField()
        {
            AddToClassList(ussClassName);

            m_CXField = new FloatField { name = xFieldUssClassName, unit = "X" };
            m_CXField.AddToClassList(xFieldUssClassName);

            m_CYField = new FloatField { name = yFieldUssClassName, unit = "Y" };
            m_CYField.AddToClassList(yFieldUssClassName);

            m_CZField = new FloatField { name = zFieldUssClassName, unit = "Z" };
            m_CZField.AddToClassList(zFieldUssClassName);

            m_SXField = new FloatField { name = sxFieldUssClassName, unit = "X" };
            m_SXField.AddToClassList(sxFieldUssClassName);

            m_SYField = new FloatField { name = syFieldUssClassName, unit = "Y" };
            m_SYField.AddToClassList(syFieldUssClassName);

            m_SZField = new FloatField { name = szFieldUssClassName, unit = "Z" };
            m_SZField.AddToClassList(szFieldUssClassName);

            var centerLabel = new Text("Center") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            centerLabel.AddToClassList(labelUssClassName);
            var sizeLabel = new Text("Size") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            sizeLabel.AddToClassList(labelUssClassName);

            var centerRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            centerRow.AddToClassList(rowUssClassName);
            centerRow.Add(centerLabel);
            centerRow.Add(m_CXField);
            centerRow.Add(m_CYField);
            centerRow.Add(m_CZField);

            var sizeRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            sizeRow.AddToClassList(rowUssClassName);
            sizeRow.Add(sizeLabel);
            sizeRow.Add(m_SXField);
            sizeRow.Add(m_SYField);
            sizeRow.Add(m_SZField);

            hierarchy.Add(centerRow);
            hierarchy.Add(sizeRow);

            size = Size.M;
            SetValueWithoutNotify(new Bounds());

            m_CXField.RegisterValueChangedCallback(OnCXFieldChanged);
            m_CYField.RegisterValueChangedCallback(OnCYFieldChanged);
            m_CZField.RegisterValueChangedCallback(OnCZFieldChanged);
            m_SXField.RegisterValueChangedCallback(OnSXFieldChanged);
            m_SYField.RegisterValueChangedCallback(OnSYFieldChanged);
            m_SZField.RegisterValueChangedCallback(OnSZFieldChanged);
        }

        /// <summary>
        /// The content container of the BoundsField. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The BoundsField size.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_CXField.size = m_Size;
                m_CYField.size = m_Size;
                m_CZField.size = m_Size;
                m_SXField.size = m_Size;
                m_SYField.size = m_Size;
                m_SZField.size = m_Size;
            }
        }

        /// <summary>
        /// Sets the value of the BoundsField without notifying the value changed callback.
        /// </summary>
        /// <param name="newValue"> The new value of the BoundsField. </param>
        public void SetValueWithoutNotify(Bounds newValue)
        {
            m_Value = newValue;
            m_CXField.SetValueWithoutNotify(m_Value.center.x);
            m_CYField.SetValueWithoutNotify(m_Value.center.y);
            m_CZField.SetValueWithoutNotify(m_Value.center.z);
            m_SXField.SetValueWithoutNotify(m_Value.size.x);
            m_SYField.SetValueWithoutNotify(m_Value.size.y);
            m_SZField.SetValueWithoutNotify(m_Value.size.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the BoundsField.
        /// </summary>
        public Bounds value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<Bounds>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Set the validation state of the BoundsField.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_CXField.EnableInClassList(Styles.invalidUssClassName, value);
                m_CYField.EnableInClassList(Styles.invalidUssClassName, value);
                m_CZField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SXField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SYField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SZField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The validation function of the BoundsField.
        /// </summary>
        public Func<Bounds, bool> validateValue { get; set; }

        void OnCZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(new Vector3(value.center.x, value.center.y, evt.newValue), value.size);
        }

        void OnCYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(new Vector3(value.center.x, evt.newValue, value.center.z), value.size);
        }

        void OnCXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(new Vector3(evt.newValue, value.center.y, value.center.z), value.size);
        }

        void OnSXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(value.center, new Vector3(evt.newValue, value.size.y, value.size.z));
        }

        void OnSYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(value.center, new Vector3(value.size.x, evt.newValue, value.size.z));
        }

        void OnSZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(value.center, new Vector3(value.size.x, value.size.y, evt.newValue));
        }

        /// <summary>
        /// Class to instantiate a <see cref="BoundsField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<BoundsField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="BoundsField"/>.
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

                var element = (BoundsField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
