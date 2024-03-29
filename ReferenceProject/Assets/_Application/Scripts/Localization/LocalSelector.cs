﻿using System;
using System.Linq;
using Unity.ReferenceProject.Settings;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class LocalSelector : MonoBehaviour
    {
        [SerializeField]
        uint m_Order;

        IAppLocalization m_AppLocalization;
        DropdownSetting m_DropdownSetting;

        IGlobalSettings m_GlobalSettings;

        [Inject]
        void Setup(IGlobalSettings settings, IAppLocalization localization)
        {
            m_GlobalSettings = settings;
            m_AppLocalization = localization;
        }

        void OnEnable()
        {
            m_AppLocalization.LocalizationLoaded += AddSetting;
        }

        void OnDisable()
        {
            m_GlobalSettings.RemoveSetting(m_DropdownSetting);
        }

        void AddSetting()
        {
            m_AppLocalization.LocalizationLoaded -= AddSetting;
            var locales = m_AppLocalization.Locales;
            var languages = locales.Select(l => l.LocaleName).ToArray();
            m_DropdownSetting = new DropdownSetting("@ReferenceProject:Settings_Language", languages, OnSettingChanged, SelectedValue);
            m_GlobalSettings.AddSetting(m_DropdownSetting, m_Order);
        }

        int SelectedValue()
        {
            var currentLocal = m_AppLocalization.SelectedLocale;
            return m_AppLocalization.Locales.IndexOf(currentLocal);
        }

        void OnSettingChanged(int index)
        {
            var selectedLocale = m_AppLocalization.Locales[index];
            m_AppLocalization.SelectedLocale = selectedLocale;
        }
    }
}
