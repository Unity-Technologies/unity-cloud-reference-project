using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Settings;
using Unity.ReferenceProject.UIPanel;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class ThemeSetting : MonoBehaviour
    {
        [SerializeField]
        protected List<string> m_ThemeIds;

        readonly string k_ThemePrefKey = "ReferenceProject-Theme";

        protected string m_CurrentTheme;

        DropdownSettings m_DropdownSettings;
        IGlobalSettings m_GlobalSettings;

        [Inject]
        void Setup(IGlobalSettings settings)
        {
            m_GlobalSettings = settings;
        }

        void Awake()
        {
            m_DropdownSettings = new DropdownSettings("@ReferenceProject:Settings_Theme", m_ThemeIds.ToArray(), OnSettingChanged, SelectedValue);
            m_CurrentTheme = PlayerPrefs.GetString(k_ThemePrefKey, m_ThemeIds[0]);
        }

        void Start()
        {
            UpdateTheme();
        }

        void OnEnable()
        {
            if (m_GlobalSettings != null)
            {
                m_GlobalSettings.AddSetting(m_DropdownSettings);
            }
        }

        void OnDisable()
        {
            m_GlobalSettings.RemoveSetting(m_DropdownSettings);
            PlayerPrefs.SetString(k_ThemePrefKey, m_CurrentTheme);
        }

        protected virtual void UpdateTheme()
        {
            MainUIPanel.Instance.Theme = m_CurrentTheme;
        }

        void OnSettingChanged(int index)
        {
            m_CurrentTheme = m_ThemeIds[index];
            UpdateTheme();
        }

        int SelectedValue()
        {
            var theme = m_CurrentTheme;
            return m_ThemeIds.IndexOf(theme);
        }
    }
}
