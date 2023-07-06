using System;
using Unity.ReferenceProject.Settings;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject
{
    public class StatsDisplay : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        float m_TargetFrameRate = 60.0f;

        [SerializeField]
        Gradient m_ColorGradient;

        [SerializeField]
        FrameRateCalculator m_FrameRateCalculator = new();

        [Header("UXML")]
        [SerializeField]
        string m_FrameRateElement = "fps-header";

        [SerializeField]
        string m_MinFrameRateElement = "fps-min-header";

        [SerializeField]
        string m_MaxFrameRateElement = "fps-max-header";

        [Header("Localization")]
        [SerializeField]
        string m_FrameRateString = "@ReferenceProject:Settings_FrameRate";

        public event Action<bool> OnShowPanel;
        public bool IsEnabled => m_IsEnabled;

        Heading m_FrameRateHeader;
        IGlobalSettings m_GlobalSettings;
        bool m_IsEnabled;
        Heading m_MaxFrameRateHeader;
        Heading m_MinFrameRateHeader;

        VisualElement m_RootVisualElement;
        ToggleSetting m_ToggleSetting;

        static readonly string s_StatsPrefKey = "ReferenceProject-StatsProject-Enabled";

        [Inject]
        void Setup(IGlobalSettings settings)
        {
            m_GlobalSettings = settings;
        }

        void Awake()
        {
            if (m_UIDocument != null)
            {
                InitUIToolkit(m_UIDocument);
            }

            m_ToggleSetting = new ToggleSetting(m_FrameRateString, OnSettingChanged, ToggledValue);
        }

        void Update()
        {
            if (m_IsEnabled)
            {
                m_FrameRateCalculator.Update(Time.deltaTime);
            }
        }

        void OnEnable()
        {
            m_GlobalSettings.AddSetting(m_ToggleSetting);
            m_IsEnabled = PlayerPrefs.GetFloat(s_StatsPrefKey, 0.0f) != 0.0f;
            ShowPanel(m_IsEnabled);
        }

        void OnDisable()
        {
            m_GlobalSettings.RemoveSetting(m_ToggleSetting);
            PlayerPrefs.SetFloat(s_StatsPrefKey, m_IsEnabled ? 1.0f : 0.0f);
        }

        public void InitUIToolkit(UIDocument uiDocument)
        {
            var root = m_RootVisualElement = uiDocument.rootVisualElement;

            m_FrameRateHeader = root.Q<Heading>(m_FrameRateElement);
            m_MinFrameRateHeader = root.Q<Heading>(m_MinFrameRateElement);
            m_MaxFrameRateHeader = root.Q<Heading>(m_MaxFrameRateElement);

            m_FrameRateCalculator.FrameRateRefreshed += OnFrameRateRefreshed;
        }

        public void ClosePanel()
        {
            m_ToggleSetting.SetValueWithoutNotify(false);
            m_IsEnabled = false;
            ShowPanel(false);
        }

        public bool ToggledValue()
        {
            return m_IsEnabled;
        }

        void OnSettingChanged(bool value)
        {
            m_IsEnabled = value;
            ShowPanel(value);
        }

        void ShowPanel(bool value)
        {
            SetVisible(m_RootVisualElement, value);
            OnShowPanel?.Invoke(value);
        }

        void OnFrameRateRefreshed(int fps, int minFps, int maxFps)
        {
            UpdateHeader(m_FrameRateHeader, fps);
            UpdateHeader(m_MinFrameRateHeader, minFps);
            UpdateHeader(m_MaxFrameRateHeader, maxFps);
        }

        static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        void UpdateHeader(Heading header, int fps)
        {
            header.text = fps.ToString();
            header.style.color = m_ColorGradient.Evaluate(fps / m_TargetFrameRate);
        }
    }
}
