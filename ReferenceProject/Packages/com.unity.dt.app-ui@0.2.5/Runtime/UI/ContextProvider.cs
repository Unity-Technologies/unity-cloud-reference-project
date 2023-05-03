using System;
using UnityEngine.Assertions;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// This element can be used in the visual tree to wrap a part of the user-interface where the context
    /// of the application needs to be overriden.
    /// </summary>
    public class ContextProvider : VisualElement
    {
        /// <summary>
        /// Main Uss Class Name.
        /// </summary>
        public static readonly string ussClassName = "appui";

        /// <summary>
        /// Prefix used in Context USS classes.
        /// </summary>
        public static readonly string contextPrefix = "appui--";

        string m_Scale;

        string m_Theme;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContextProvider()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
        }

        /// <summary>
        /// The current context, computed using the root context of the <see cref="Panel"/> and cascading context
        /// providers overrides until this <see cref="ContextProvider"/> instance.
        /// </summary>
        public virtual ApplicationContext context
        {
            get
            {
                var parentContextProvider = GetFirstAncestorOfType<ContextProvider>();
                Assert.IsNotNull(parentContextProvider, "You should have at least the main Application element as parent context provider.");
                return new ApplicationContext(parentContextProvider.context, this);
            }
        }

        /// <summary>
        /// The current language override.
        /// <remarks> This property is useful only if you use the Unity Localization package inside your project.</remarks>
        /// </summary>
        public string lang { get; set; }

        /// <summary>
        /// The preferred <see cref="Tooltip"/> placement in this context.
        /// </summary>
        public PopoverPlacement? preferredTooltipPlacement { get; set; }

        /// <summary>
        /// The current UI scale override.
        /// </summary>
        public string scale
        {
            get => m_Scale;
            set
            {
                if (!string.IsNullOrEmpty(m_Scale))
                    RemoveFromClassList($"{contextPrefix}{m_Scale}");
                m_Scale = value;
                if (!string.IsNullOrEmpty(m_Scale))
                    AddToClassList($"{contextPrefix}{m_Scale}");
            }
        }

        /// <summary>
        /// The current UI theme override.
        /// </summary>
        public string theme
        {
            get => m_Theme;
            set
            {
                if (!string.IsNullOrEmpty(m_Theme))
                    RemoveFromClassList($"{contextPrefix}{m_Theme}");
                m_Theme = value;
                if (!string.IsNullOrEmpty(m_Theme))
                    AddToClassList($"{contextPrefix}{m_Theme}");
            }
        }

        /// <summary>
        /// A class responsible for creating a <see cref="ContextProvider"/> from UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ContextProvider, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ContextProvider"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Lang = new UxmlStringAttributeDescription
            {
                defaultValue = null,
                name = "lang",
                use = UxmlAttributeDescription.Use.Optional
            };

            readonly UxmlStringAttributeDescription m_Scale = new UxmlStringAttributeDescription
            {
                defaultValue = "medium",
                name = "scale",
                use = UxmlAttributeDescription.Use.Optional
            };

            readonly UxmlStringAttributeDescription m_Theme = new UxmlStringAttributeDescription
            {
                defaultValue = "dark",
                name = "theme",
                use = UxmlAttributeDescription.Use.Optional
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
                var el = (ContextProvider)ve;
                string val = null;
                if (m_Lang.TryGetValueFromBag(bag, cc, ref val))
                    el.lang = val;
                if (m_Theme.TryGetValueFromBag(bag, cc, ref val))
                    el.theme = val;
                if (m_Scale.TryGetValueFromBag(bag, cc, ref val))
                    el.scale = val;
            }
        }
    }
}
