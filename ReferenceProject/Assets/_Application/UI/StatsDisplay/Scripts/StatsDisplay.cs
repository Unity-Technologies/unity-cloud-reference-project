using System;
using Unity.ReferenceProject.Settings;
using UnityEngine;
using Unity.ReferenceProject.Stats;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject
{
    public class StatsDisplay : MonoBehaviour
    {
        [SerializeField]
        GlobalStatsUIController m_GlobalStatsUIController;

        [Header("Localization")]
        [SerializeField]
        string m_DisplayStatsString = "@ReferenceProject:DisplayStats";

        public event Action<bool> OnShowPanel;
        public bool IsEnabled { get; private set; }

        IGlobalSettings m_GlobalSettings;

        ToggleSetting m_ToggleSetting;

        static readonly string k_StatsPrefKey = "ReferenceProject-StatsProject-Enabled";

        [Inject]
        void Setup(IGlobalSettings settings)
        {
            m_GlobalSettings = settings;
        }

        void Awake()
        {
            m_ToggleSetting = new ToggleSetting(m_DisplayStatsString, OnSettingChanged, ToggledValue);
        }

        void OnEnable()
        {
            m_GlobalSettings.AddSetting(m_ToggleSetting);
            IsEnabled = PlayerPrefs.GetInt(k_StatsPrefKey, 0) != 0;
            ShowStats(IsEnabled);
        }

        void OnDisable()
        {
            m_GlobalSettings.RemoveSetting(m_ToggleSetting);
            PlayerPrefs.SetInt(k_StatsPrefKey, IsEnabled ? 1 : 0);
        }

        public void ClosePanel()
        {
            m_ToggleSetting.SetValueWithoutNotify(false);
            IsEnabled = false;
            ShowStats(false);
        }

        public bool ToggledValue()
        {
            return IsEnabled;
        }

        void OnSettingChanged(bool value)
        {
            IsEnabled = value;
            ShowStats(value);
        }

        void ShowStats(bool value)
        {
            m_GlobalStatsUIController.IsDisplayed = value;
            OnShowPanel?.Invoke(value);
        }

        public void InitWithUIDocument(UIDocument document)
        {
            m_GlobalStatsUIController.CreateVisualTree(document);
        }
    }
}
