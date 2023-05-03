using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Class containing the UXML traits for the VisualElement class.
    /// </summary>
    public class VisualElementExtendedUxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlEnumAttributeDescription<PopoverPlacement> m_PreferredTooltipPlacement =
            new UxmlEnumAttributeDescription<PopoverPlacement>
            {
                defaultValue = Tooltip.defaultPlacement,
                name = "preferred-tooltip-placement"
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

            var preferredTooltipPlacement = Tooltip.defaultPlacement;
            if (m_PreferredTooltipPlacement.TryGetValueFromBag(bag, cc, ref preferredTooltipPlacement))
                ve.SetPreferredTooltipPlacement(preferredTooltipPlacement);
        }
    }

    /// <summary>
    /// Class containing the UXML traits for the TextElement class.
    /// </summary>
    public class TextElementExtendedUxmlTraits : TextElement.UxmlTraits
    {
        readonly UxmlEnumAttributeDescription<PopoverPlacement> m_PreferredTooltipPlacement =
            new UxmlEnumAttributeDescription<PopoverPlacement>
            {
                defaultValue = Tooltip.defaultPlacement,
                name = "preferred-tooltip-placement"
            };

        /// <summary>
        /// Initializes the VisualElement from the UXML attributes.
        /// </summary>
        /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
        /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
        /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            var isFocusable = ve.focusable;
            base.Init(ve, bag, cc);
            
            if (isFocusable)
                ve.focusable = true;

            var preferredTooltipPlacement = Tooltip.defaultPlacement;
            if (m_PreferredTooltipPlacement.TryGetValueFromBag(bag, cc, ref preferredTooltipPlacement))
                ve.SetPreferredTooltipPlacement(preferredTooltipPlacement);
        }
    }
}
