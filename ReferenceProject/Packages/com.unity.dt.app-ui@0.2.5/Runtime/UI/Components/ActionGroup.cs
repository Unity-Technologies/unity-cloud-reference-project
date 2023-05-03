using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// ActionGroup UI element.
    /// </summary>
    public class ActionGroup : VisualElement
    {
        /// <summary>
        /// The ActionGroup main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-actiongroup";

        /// <summary>
        /// The ActionGroup quiet mode styling class.
        /// </summary>
        public static readonly string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The ActionGroup compact mode styling class.
        /// </summary>
        public static readonly string compactUssClassName = ussClassName + "--compact";

        /// <summary>
        /// The ActionGroup vertical mode styling class.
        /// </summary>
        public static readonly string verticalUssClassName = ussClassName + "--vertical";

        /// <summary>
        /// The ActionGroup justified mode styling class.
        /// </summary>
        public static readonly string justifiedUssClassName = ussClassName + "--justified";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionGroup()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// The content container of the ActionGroup.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The quiet state of the ActionGroup.
        /// </summary>
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set => EnableInClassList(quietUssClassName, value);
        }

        /// <summary>
        /// The compact state of the ActionGroup.
        /// </summary>
        public bool compact
        {
            get => ClassListContains(compactUssClassName);
            set => EnableInClassList(compactUssClassName, value);
        }

        /// <summary>
        /// The vertical state of the ActionGroup.
        /// </summary>
        public bool vertical
        {
            get => ClassListContains(verticalUssClassName);
            set => EnableInClassList(verticalUssClassName, value);
        }

        /// <summary>
        /// The justified state of the ActionGroup.
        /// </summary>
        public bool justified
        {
            get => ClassListContains(justifiedUssClassName);
            set => EnableInClassList(justifiedUssClassName, value);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            for (var i = 0; i < childCount; i++)
            {
                var child = ElementAt(i);
                child.EnableInClassList("unity-first-child", i == 0);
                child.EnableInClassList(ussClassName + "__inbetween-item", i != 0 && i != childCount - 1);
                child.EnableInClassList("unity-last-child", i == childCount - 1);
                child.AddToClassList(ussClassName + "__item");
            }
        }

        /// <summary>
        /// The UXML factory for the ActionGroup.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ActionGroup, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ActionGroup"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Compact = new UxmlBoolAttributeDescription
            {
                name = "compact",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Justified = new UxmlBoolAttributeDescription
            {
                name = "justified",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Quiet = new UxmlBoolAttributeDescription
            {
                name = "quiet",
                defaultValue = false
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
                var el = (ActionGroup)ve;
                el.quiet = m_Quiet.GetValueFromBag(bag, cc);
                el.compact = m_Compact.GetValueFromBag(bag, cc);
                el.vertical = m_Vertical.GetValueFromBag(bag, cc);
                el.justified = m_Justified.GetValueFromBag(bag, cc);
            }
        }
    }
}
