using System;
using Unity.ReferenceProject.Settings;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Instructions
{
    [RequireComponent(typeof(InstructionsUIController))]
    public class InstructionsToggleSetting : MonoBehaviour
    {
        [SerializeField]
        string m_LabelText;
        InstructionsUIController m_Controller;

        IGlobalSettings m_GlobalSettings;

        ToggleSetting m_Settings;

        [Inject]
        void Setup(IGlobalSettings globalSettings)
        {
            m_GlobalSettings = globalSettings;
        }

        void Awake()
        {
            m_Controller = GetComponent<InstructionsUIController>();
            m_Controller.InstructionPanelEnabled += OnInstructionPanelEnabled;
            m_Controller.InstructionsAvailable += (isSupport) => enabled = isSupport;
        }

        void OnEnable()
        {
            if (m_GlobalSettings != null && m_Settings == null)
            {
                m_Settings = new ToggleSetting(m_LabelText, OnSettingChanged, IsEnabled);
                m_GlobalSettings.AddSetting(m_Settings);
                OnInstructionPanelEnabled(IsEnabled());
            }
        }

        void OnDisable()
        {
            if (m_Settings != null && m_GlobalSettings != null)
            {
                m_GlobalSettings.RemoveSetting(m_Settings);
                m_Settings = null;
            }
        }

        bool IsEnabled() => m_Controller.IsInstructionsEnabled;

        void OnSettingChanged(bool isTrue) => m_Controller.SetVisiblePanel(isTrue);

        void OnInstructionPanelEnabled(bool isInstructionPanelEnabled) => m_Settings?.SetValueWithoutNotify(isInstructionPanelEnabled);
    }
}
