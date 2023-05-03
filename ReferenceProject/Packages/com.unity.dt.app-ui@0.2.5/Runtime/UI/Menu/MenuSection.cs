using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A section inside a menu, with a heading.
    /// </summary>
    public class MenuSection : VisualElement
    {
        /// <summary>
        /// The MenuSection main styling class.
        /// </summary>
        public const string ussClassName = "appui-menusection";

        /// <summary>
        /// The MenuSection title styling class.
        /// </summary>
        public static readonly string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The MenuSection container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        readonly VisualElement m_Container;

        readonly LocalizedTextElement m_Title;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuSection()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);

            hierarchy.Add(m_Title);
            hierarchy.Add(m_Container);
        }

        /// <summary>
        /// The MenuSection container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The text to display in the section heading.
        /// </summary>
        public string title
        {
            get => m_Title.text;
            set
            {
                m_Title.text = value;
                m_Title.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// The MenuSection UXML factory.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<MenuSection, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="MenuSection"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Title = new UxmlStringAttributeDescription
            {
                name = "title",
                defaultValue = null
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

                var element = (MenuSection)ve;
                element.title = m_Title.GetValueFromBag(bag, cc);
            }
        }
    }
}
