using System;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Unity.ReferenceProject
{
    public interface IAppLocalization
    {
        List<Locale> Locales { get; }
        Locale SelectedLocale { get; set; }
        event Action LocalizationLoaded;
    }

    // To support Localization on WeGL, we need to use the asynchronous LocalizationSettings API.
    public class AppLocalization : IAppLocalization
    {
        bool m_IsDone;

        public AppLocalization()
        {
            var localizationSettings = LocalizationSettings.GetInstanceDontCreateDefault();
            if (localizationSettings)
            {
                var operation = localizationSettings.GetInitializationOperation();
                operation.Completed += _ => InitializationCompleted();
            }
        }

        public event Action LocalizationLoaded
        {
            add
            {
                m_LocalizationLoaded += value;

                if (m_IsDone)
                {
                    value?.Invoke();
                }
            }
            remove => m_LocalizationLoaded -= value;
        }

        public List<Locale> Locales
        {
            get
            {
                if (m_IsDone)
                    return LocalizationSettings.Instance.GetAvailableLocales().Locales;

                throw new InvalidOperationException($"Enumerating {nameof(Locales)} before {nameof(LocalizationSettings)} has finished async initialization."
                    + $" Use {nameof(LocalizationLoaded)} to get notified when initialization is done.");
            }
        }

        public Locale SelectedLocale
        {
            get
            {
                if (m_IsDone)
                    return LocalizationSettings.Instance.GetSelectedLocale();

                throw new InvalidOperationException($"Getting {nameof(SelectedLocale)} before {nameof(LocalizationSettings)} has finished async initialization."
                    + $" Use {nameof(LocalizationLoaded)} to get notified when initialization is done.");
            }
            set
            {
                if (m_IsDone)
                {
                    LocalizationSettings.Instance.SetSelectedLocale(value);
                }
            }
        }

        event Action m_LocalizationLoaded;

        void InitializationCompleted()
        {
            m_IsDone = true;
            m_LocalizationLoaded?.Invoke();
        }
    }
}
