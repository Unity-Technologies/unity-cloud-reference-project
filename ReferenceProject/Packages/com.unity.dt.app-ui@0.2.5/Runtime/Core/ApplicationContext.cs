using System;
using UnityEngine.Dt.App.UI;
#if UNITY_LOCALIZATION_PRESENT
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// The ApplicationContext provides the current context of the application.
    /// <remarks><see cref="UI.VisualElementExtensions.GetContext"/> to retrieve the context at a specific place in the visual tree.</remarks>
    /// </summary>
    public struct ApplicationContext
    {
        /// <summary>
        /// The application instance which wraps the whole UI.
        /// <remarks>
        /// You must have a single <see cref="Panel"/> instance inside your runtime app.
        /// </remarks>
        /// </summary>
        public Panel panel { get; }

        /// <summary>
        /// The current language.
        /// <remarks>
        /// This parameter is only used if the Unity Localization package is installed and set up correctly.
        /// </remarks>
        /// </summary>
        public string lang { get; }

        /// <summary>
        /// The preferred placement of the tooltip.
        /// </summary>
        public PopoverPlacement? preferredTooltipPlacement { get; }

        /// <summary>
        /// The current scale of the UI. Possible values are "medium" or "large".
        /// </summary>
        public string scale { get; }

        /// <summary>
        /// The current theme of the UI. Possible values are "dark" or "light".
        /// </summary>
        public string theme { get; }
        
        /// <summary>
        /// The closest <see cref="ContextProvider"/> instance which overrides the current context.
        /// </summary>
        public ContextProvider closestProvider { get; }
        
#if UNITY_LOCALIZATION_PRESENT
        /// <summary>
        /// The current locale.
        /// </summary>
        public Locale locale
        {
            get
            {
                var settings = LocalizationSettings.GetInstanceDontCreateDefault();
                if (!settings)
                    return null;
                
                var globalLocale = settings.GetSelectedLocaleAsync();
                if (!globalLocale.IsDone)
                    return null;

                if (string.IsNullOrEmpty(lang))
                    return globalLocale.Result;

                var availableLocales = settings.GetAvailableLocales();
                if (availableLocales is LocalesProvider localesProvider &&
                    (!localesProvider.PreloadOperation.IsValid() || !localesProvider.PreloadOperation.IsDone))
                    return null;

                var scopedLocale = availableLocales.GetLocale(lang);
                return scopedLocale ? scopedLocale : globalLocale.Result;
            }
        }
#endif

        /// <summary>
        /// Construct an <see cref="ApplicationContext"/> based on an other one, and overrides from a given <see cref="ContextProvider"/>.
        /// </summary>
        /// <param name="context">The reference application context.</param>
        /// <param name="contextProvider">The <see cref="ContextProvider"/> instance which overrides the reference application context.</param>
        public ApplicationContext(ApplicationContext context, ContextProvider contextProvider)
        {
            lang = contextProvider.lang ?? context.lang;
            scale = contextProvider.scale ?? context.scale;
            theme = contextProvider.theme ?? context.theme;
            preferredTooltipPlacement = contextProvider.preferredTooltipPlacement ?? context.preferredTooltipPlacement;
            panel = context.panel; // no override
            closestProvider = contextProvider;
        }

        /// <summary>
        /// Construct an <see cref="ApplicationContext"/> based on an <see cref="Panel"/> instance.
        /// </summary>
        /// <param name="app">The application instance.</param>
        public ApplicationContext(Panel app)
        {
            lang = app.lang;
            scale = app.scale;
            theme = app.theme;
            preferredTooltipPlacement = app.preferredTooltipPlacement;
            panel = app;
            closestProvider = app;
        }
    }
}
