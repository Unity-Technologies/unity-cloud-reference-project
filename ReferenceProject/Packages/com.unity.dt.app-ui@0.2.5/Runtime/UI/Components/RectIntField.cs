using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// RectInt Field UI element.
    /// </summary>
    public class RectIntField : VisualElement, IValidatableElement<RectInt>, ISizeableElement
    {
        /// <summary>
        /// The RectIntField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-rectfield";

        /// <summary>
        /// The RectIntField row styling class.
        /// </summary>
        public static readonly string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The RectIntField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The RectIntField X NumericalField styling class.
        /// </summary>
        public static readonly string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The RectIntField Y NumericalField styling class.
        /// </summary>
        public static readonly string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The RectIntField H NumericalField styling class.
        /// </summary>
        public static readonly string hFieldUssClassName = ussClassName + "__h-field";

        /// <summary>
        /// The RectIntField W NumericalField styling class.
        /// </summary>
        public static readonly string wFieldUssClassName = ussClassName + "__w-field";

        /// <summary>
        /// The RectIntField Label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        RectInt m_Value;

        readonly IntField m_WField;

        readonly IntField m_XField;

        readonly IntField m_YField;

        readonly IntField m_HField;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RectIntField()
        {
            AddToClassList(ussClassName);

            m_XField = new IntField { name = xFieldUssClassName, unit = "X" };
            m_XField.AddToClassList(xFieldUssClassName);

            m_YField = new IntField { name = yFieldUssClassName, unit = "Y" };
            m_YField.AddToClassList(yFieldUssClassName);

            m_WField = new IntField { name = wFieldUssClassName, unit = "W" };
            m_WField.AddToClassList(wFieldUssClassName);

            m_HField = new IntField { name = hFieldUssClassName, unit = "H" };
            m_HField.AddToClassList(hFieldUssClassName);

            var positionLabel = new Text("Position") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            positionLabel.AddToClassList(labelUssClassName);
            var sizeLabel = new Text("Size") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            sizeLabel.AddToClassList(labelUssClassName);

            var positionRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            positionRow.AddToClassList(rowUssClassName);
            positionRow.Add(positionLabel);
            positionRow.Add(m_XField);
            positionRow.Add(m_YField);

            var sizeRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            sizeRow.AddToClassList(rowUssClassName);
            sizeRow.Add(sizeLabel);
            sizeRow.Add(m_WField);
            sizeRow.Add(m_HField);

            hierarchy.Add(positionRow);
            hierarchy.Add(sizeRow);

            size = Size.M;
            SetValueWithoutNotify(new RectInt(0, 0, 0, 0));

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_HField.RegisterValueChangedCallback(OnHFieldChanged);
            m_WField.RegisterValueChangedCallback(OnWFieldChanged);
        }

        /// <summary>
        /// The content container of the RectIntField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the RectIntField.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_XField.size = m_Size;
                m_YField.size = m_Size;
                m_HField.size = m_Size;
                m_WField.size = m_Size;
            }
        }

        /// <summary>
        /// Set the value of the RectIntField without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the RectIntField. </param>
        public void SetValueWithoutNotify(RectInt newValue)
        {
            m_Value = newValue;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_HField.SetValueWithoutNotify(m_Value.height);
            m_WField.SetValueWithoutNotify(m_Value.width);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the RectIntField.
        /// </summary>
        public RectInt value
        {
            get => m_Value;
            set
            {
                if (m_Value.Equals(value))
                    return;
                using var evt = ChangeEvent<RectInt>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the RectIntField.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_XField.EnableInClassList(Styles.invalidUssClassName, value);
                m_YField.EnableInClassList(Styles.invalidUssClassName, value);
                m_HField.EnableInClassList(Styles.invalidUssClassName, value);
                m_WField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The callback to validate the value of the RectIntField.
        /// </summary>
        public Func<RectInt, bool> validateValue { get; set; }

        void OnHFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(value.x, value.y, value.width, evt.newValue);
        }

        void OnWFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(value.x, value.y, evt.newValue, value.height);
        }

        void OnYFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(value.x, evt.newValue, value.width, value.height);
        }

        void OnXFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(evt.newValue, value.y, value.width, value.height);
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="RectIntField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<RectIntField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="RectIntField"/>.
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

                var element = (RectIntField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
