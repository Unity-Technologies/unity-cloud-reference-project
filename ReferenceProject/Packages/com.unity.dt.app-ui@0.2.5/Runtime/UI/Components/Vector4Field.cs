using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Vector4 Field UI element.
    /// </summary>
    public class Vector4Field : VisualElement, IValidatableElement<Vector4>, ISizeableElement
    {
        /// <summary>
        /// The Vector4Field main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-vector4field";

        /// <summary>
        /// The Vector4Field row styling class.
        /// </summary>
        public static readonly string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The Vector4Field size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Vector4Field X NumericalField styling class.
        /// </summary>
        public static readonly string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The Vector4Field Y NumericalField styling class.
        /// </summary>
        public static readonly string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The Vector4Field Z NumericalField styling class.
        /// </summary>
        public static readonly string zFieldUssClassName = ussClassName + "__z-field";

        /// <summary>
        /// The Vector4Field W NumericalField styling class.
        /// </summary>
        public static readonly string wFieldUssClassName = ussClassName + "__w-field";

        Size m_Size;

        Vector4 m_Value;

        readonly FloatField m_WField;

        readonly FloatField m_XField;

        readonly FloatField m_YField;

        readonly FloatField m_ZField;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector4Field()
        {
            AddToClassList(ussClassName);

            m_XField = new FloatField { name = xFieldUssClassName, unit = "X" };
            m_XField.AddToClassList(xFieldUssClassName);

            m_YField = new FloatField { name = yFieldUssClassName, unit = "Y" };
            m_YField.AddToClassList(yFieldUssClassName);

            m_ZField = new FloatField { name = zFieldUssClassName, unit = "Z" };
            m_ZField.AddToClassList(zFieldUssClassName);

            m_WField = new FloatField { name = wFieldUssClassName, unit = "W" };
            m_WField.AddToClassList(wFieldUssClassName);

            var xyRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            xyRow.AddToClassList(rowUssClassName);
            xyRow.Add(m_XField);
            xyRow.Add(m_YField);

            var zwRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            zwRow.AddToClassList(rowUssClassName);
            zwRow.Add(m_ZField);
            zwRow.Add(m_WField);

            hierarchy.Add(xyRow);
            hierarchy.Add(zwRow);

            size = Size.M;
            SetValueWithoutNotify(Vector4.zero);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_ZField.RegisterValueChangedCallback(OnZFieldChanged);
            m_WField.RegisterValueChangedCallback(OnWFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector4Field.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector4Field.
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
                m_ZField.size = m_Size;
                m_WField.size = m_Size;
            }
        }

        /// <summary>
        /// Set the value of the Vector4Field without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the Vector4Field. </param>
        public void SetValueWithoutNotify(Vector4 newValue)
        {
            m_Value = newValue;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_ZField.SetValueWithoutNotify(m_Value.z);
            m_WField.SetValueWithoutNotify(m_Value.w);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector4Field.
        /// </summary>
        public Vector4 value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<Vector4>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector4Field.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_XField.EnableInClassList(Styles.invalidUssClassName, value);
                m_YField.EnableInClassList(Styles.invalidUssClassName, value);
                m_ZField.EnableInClassList(Styles.invalidUssClassName, value);
                m_WField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The validation function of the Vector4Field.
        /// </summary>
        public Func<Vector4, bool> validateValue { get; set; }

        void OnWFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(value.x, value.y, value.z, evt.newValue);
        }

        void OnZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(value.x, value.y, evt.newValue, value.w);
        }

        void OnYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(value.x, evt.newValue, value.z, value.w);
        }

        void OnXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(evt.newValue, value.y, value.z, value.w);
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Vector4Field"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Vector4Field, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Vector4Field"/>.
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

                var element = (Vector4Field)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);

                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
