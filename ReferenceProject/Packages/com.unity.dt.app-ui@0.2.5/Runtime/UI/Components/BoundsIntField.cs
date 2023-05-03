using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// BoundsInt Field UI element.
    /// </summary>
    public class BoundsIntField : VisualElement, IValidatableElement<BoundsInt>, ISizeableElement
    {
        /// <summary>
        /// The BoundsIntField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-boundsfield";

        /// <summary>
        /// The BoundsIntField row styling class.
        /// </summary>
        public static readonly string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The BoundsIntField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The BoundsIntField X NumericalField styling class.
        /// </summary>
        public static readonly string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The BoundsIntField Y NumericalField styling class.
        /// </summary>
        public static readonly string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The BoundsIntField Z NumericalField styling class.
        /// </summary>
        public static readonly string zFieldUssClassName = ussClassName + "__z-field";

        /// <summary>
        /// The BoundsIntField X NumericalField styling class.
        /// </summary>
        public static readonly string sxFieldUssClassName = ussClassName + "__sx-field";

        /// <summary>
        /// The BoundsIntField Y NumericalField styling class.
        /// </summary>
        public static readonly string syFieldUssClassName = ussClassName + "__sy-field";

        /// <summary>
        /// The BoundsIntField Z NumericalField styling class.
        /// </summary>
        public static readonly string szFieldUssClassName = ussClassName + "__sz-field";

        /// <summary>
        /// The BoundsIntField Label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        BoundsInt m_Value;

        readonly IntField m_CXField;

        readonly IntField m_CYField;

        readonly IntField m_CZField;

        readonly IntField m_SXField;

        readonly IntField m_SYField;

        readonly IntField m_SZField;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BoundsIntField()
        {
            AddToClassList(ussClassName);

            m_CXField = new IntField { name = xFieldUssClassName, unit = "X" };
            m_CXField.AddToClassList(xFieldUssClassName);

            m_CYField = new IntField { name = yFieldUssClassName, unit = "Y" };
            m_CYField.AddToClassList(yFieldUssClassName);

            m_CZField = new IntField { name = zFieldUssClassName, unit = "Z" };
            m_CZField.AddToClassList(zFieldUssClassName);

            m_SXField = new IntField { name = sxFieldUssClassName, unit = "X" };
            m_SXField.AddToClassList(sxFieldUssClassName);

            m_SYField = new IntField { name = syFieldUssClassName, unit = "Y" };
            m_SYField.AddToClassList(syFieldUssClassName);

            m_SZField = new IntField { name = szFieldUssClassName, unit = "Z" };
            m_SZField.AddToClassList(szFieldUssClassName);

            var centerLabel = new Text("Position") { size = TextSize.S, pickingMode = PickingMode.Ignore };
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
            SetValueWithoutNotify(new BoundsInt());

            m_CXField.RegisterValueChangedCallback(OnCXFieldChanged);
            m_CYField.RegisterValueChangedCallback(OnCYFieldChanged);
            m_CZField.RegisterValueChangedCallback(OnCZFieldChanged);
            m_SXField.RegisterValueChangedCallback(OnSXFieldChanged);
            m_SYField.RegisterValueChangedCallback(OnSYFieldChanged);
            m_SZField.RegisterValueChangedCallback(OnSZFieldChanged);
        }

        /// <summary>
        /// The content container of the BoundsIntField. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The BoundsIntField size.
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
        /// Sets the BoundsIntField value without notifying any change event listeners.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(BoundsInt newValue)
        {
            m_Value = newValue;
            m_CXField.SetValueWithoutNotify(m_Value.position.x);
            m_CYField.SetValueWithoutNotify(m_Value.position.y);
            m_CZField.SetValueWithoutNotify(m_Value.position.z);
            m_SXField.SetValueWithoutNotify(m_Value.size.x);
            m_SYField.SetValueWithoutNotify(m_Value.size.y);
            m_SZField.SetValueWithoutNotify(m_Value.size.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The BoundsIntField value.
        /// </summary>
        public BoundsInt value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<BoundsInt>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// Set the validation state of the BoundsIntField.
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
        /// The validation function to use on the BoundsIntField value.
        /// </summary>
        public Func<BoundsInt, bool> validateValue { get; set; }

        void OnCZFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(new Vector3Int(value.position.x, value.position.y, evt.newValue), value.size);
        }

        void OnCYFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(new Vector3Int(value.position.x, evt.newValue, value.position.z), value.size);
        }

        void OnCXFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(new Vector3Int(evt.newValue, value.position.y, value.position.z), value.size);
        }

        void OnSXFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(value.position, new Vector3Int(evt.newValue, value.size.y, value.size.z));
        }

        void OnSYFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(value.position, new Vector3Int(value.size.x, evt.newValue, value.size.z));
        }

        void OnSZFieldChanged(ChangeEvent<int> evt)
        {
            value = new BoundsInt(value.position, new Vector3Int(value.size.x, value.size.y, evt.newValue));
        }

        /// <summary>
        /// Class to instantiate a <see cref="BoundsIntField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<BoundsIntField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="BoundsIntField"/>.
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

                var element = (BoundsIntField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
