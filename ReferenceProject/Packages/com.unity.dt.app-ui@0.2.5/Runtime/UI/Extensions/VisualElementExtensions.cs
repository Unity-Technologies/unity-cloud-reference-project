using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Extensions for <see cref="VisualElement"/> class.
    /// </summary>
    public static class VisualElementExtensions
    {
        static readonly ConditionalWeakTable<VisualElement, AdditionalData> k_AdditionalDataCache =
            new ConditionalWeakTable<VisualElement, AdditionalData>();

        /// <summary>
        /// Get the current application context associated with the current <see cref="VisualElement"/> object.
        /// </summary>
        /// <param name="ve">The <see cref="VisualElement"/> object.</param>
        /// <returns>The application context for this element.</returns>
        /// <exception cref="ArgumentNullException">The provided <see cref="VisualElement"/> object must be not null.</exception>
        public static ApplicationContext GetContext(this VisualElement ve)
        {
            if (ve == null)
                throw new ArgumentNullException(nameof(ve));

            var p = ve;
            // ReSharper disable once UseNegatedPatternInIsExpression
            while (p != null && !(p is ContextProvider)) p = p.parent;

            if (p is ContextProvider contextProvider)
                return contextProvider.context;

            return default;
        }

        /// <summary>
        /// Get the <see cref="PanelSettings"/> instance associated to this <see cref="IPanel"/>, if any.
        /// </summary>
        /// <param name="panel">The <see cref="IPanel"/> object.</param>
        /// <returns>The <see cref="PanelSettings"/> instance if it exists, null otherwise.</returns>
        /// <exception cref="ArgumentNullException">The <see cref="IPanel"/> object must not be null.</exception>
        public static PanelSettings GetPanelSettings(this IPanel panel)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel));

            var prop = panel.GetType()
                .GetProperty("panelSettings", BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
                return null;

            return prop.GetValue(panel) as PanelSettings;
        }

        /// <summary>
        /// Get child elements of a given type.
        /// </summary>
        /// <param name="element">The parent element.</param>
        /// <param name="recursive">If true, the search will be recursive.</param>
        /// <typeparam name="T">The type of the child elements to search for.</typeparam>
        /// <returns> A list of child elements of the given type.</returns>
        /// <exception cref="ArgumentNullException"> The <see cref="VisualElement"/> object can't be null.</exception>
        public static IEnumerable<T> GetChildren<T>(this VisualElement element, bool recursive)
            where T : VisualElement
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var res = new List<T>();

            foreach (var child in element.Children())
            {
                if (child is T c)
                {
                    res.Add(c);
                    if (recursive)
                        res.AddRange(c.GetChildren<T>(true));
                }
            }

            return res;
        }

        /// <summary>
        /// Get the preferred placement for a <see cref="VisualElement"/>'s <see cref="Tooltip"/>.
        /// </summary>
        /// <param name="element">The <see cref="VisualElement"/> which contains a tooltip.</param>
        /// <returns>The preferred placement, previously set using <see cref="SetPreferredTooltipPlacement"/>
        /// or the closest value set on a parent <see cref="ContextProvider"/> element.</returns>
        /// <exception cref="ArgumentNullException">The <see cref="VisualElement"/> object can't be null.</exception>
        public static PopoverPlacement GetPreferredTooltipPlacement(this VisualElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element is ContextProvider provider && provider.preferredTooltipPlacement.HasValue)
                return provider.preferredTooltipPlacement.Value;

            if (k_AdditionalDataCache.TryGetValue(element, out var data))
                return data.preferredTooltipPlacement;

            var context = element.GetContext();
            return context.preferredTooltipPlacement.GetValueOrDefault(Tooltip.defaultPlacement);
        }

        /// <summary>
        /// Set a preferred <see cref="Tooltip"/> placement.
        /// </summary>
        /// <param name="element">The target visual element.</param>
        /// <param name="placement">The placement value.</param>
        /// <exception cref="ArgumentNullException">The <see cref="VisualElement"/> object can't be null.</exception>
        public static void SetPreferredTooltipPlacement(this VisualElement element, PopoverPlacement placement)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element is ContextProvider provider)
            {
                provider.preferredTooltipPlacement = placement;
                return;
            }

            var data = k_AdditionalDataCache.GetOrCreateValue(element);
            data.preferredTooltipPlacement = placement;
        }

        /// <summary>
        /// Additional Data that should be stored on any <see cref="VisualElement"/> object.
        /// </summary>
        class AdditionalData
        {
            /// <summary>
            /// The preferred placement for a tooltip.
            /// </summary>
            public PopoverPlacement preferredTooltipPlacement { get; set; } = Tooltip.defaultPlacement;
        }
    }
}
