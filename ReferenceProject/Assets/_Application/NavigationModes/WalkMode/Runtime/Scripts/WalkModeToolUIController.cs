using System;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.WalkController
{
    public class WalkModeToolUIController : ToolUIController
    {
        [SerializeField]
        WalkModeMoveController m_WalkModeMoveController;

        [Header("Settings")]
        
        [SerializeField]
        Vector2 m_HeightRange = new (0.5f, 2.0f);
        
        [SerializeField]
        Vector2 m_WalkSpeedRange = new (1.0f, 3.0f);
        
        [SerializeField]
        Vector2 m_SprintSpeedRange = new (3.0f, 10.0f);
        
        [SerializeField]
        Vector2 m_JumpForceRange = new (0.5f, 4.0f);

        INavigationManager m_NavigationManager;
        const string k_WalkModeString = "WalkMode";

        const string k_HeightString = "ControllerHeightSlider";
        const string k_WalkSpeedString = "WalkSpeedSlider";
        const string k_SprintSpeedString = "SprintSpeedSlider";
        const string k_JumpForceString = "JumpForceSlider";
        
        SliderFloat m_ControllerHeightSlider;
        SliderFloat m_WalkSpeedSlider;
        SliderFloat m_SprintSpeedSlider;
        SliderFloat m_JumpForceSlider;

        [Inject]
        void Setup(INavigationManager navigationManager)
        {
            m_NavigationManager = navigationManager;
        }

        protected override void Awake()
        {
            base.Awake();
            m_NavigationManager.NavigationModeChanged -= OnNavigationModeChanged;
            m_NavigationManager.NavigationModeChanged += OnNavigationModeChanged;
        }

        protected override void Start()
        {
            base.Start();
            RefreshToolState();
        }

        protected override void OnDestroy()
        {
            m_NavigationManager.NavigationModeChanged -= OnNavigationModeChanged;
        }

        void RefreshToolState()
        {
            var isWalkMode = m_NavigationManager.CurrentNavigationModeData != null 
                && m_NavigationManager.CurrentNavigationModeData.name == k_WalkModeString;
            
            if (isWalkMode && m_WalkModeMoveController != null)
            {
                SetToolState(ToolState.Active);
            }
            else
            {
                SetToolState(ToolState.Hidden);
            }
        }
        
        protected override void RegisterCallbacks(VisualElement visualElement)
        {
            m_ControllerHeightSlider = visualElement.Q<SliderFloat>(k_HeightString);
            m_ControllerHeightSlider.RegisterValueChangedCallback(OnHeightSliderChanged);
            
            m_WalkSpeedSlider = visualElement.Q<SliderFloat>(k_WalkSpeedString);
            m_WalkSpeedSlider.RegisterValueChangedCallback(OnWalkSpeedSliderChanged);
            
            m_SprintSpeedSlider = visualElement.Q<SliderFloat>(k_SprintSpeedString);
            m_SprintSpeedSlider.RegisterValueChangedCallback(OnSprintSpeedSliderChanged);
            
            m_JumpForceSlider = visualElement.Q<SliderFloat>(k_JumpForceString);
            m_JumpForceSlider.RegisterValueChangedCallback(OnJumpForceSliderChanged);
            
            InitializeValues();
        }

        protected override void UnregisterCallbacks(VisualElement visualElement)
        {
            m_ControllerHeightSlider.UnregisterValueChangedCallback(OnHeightSliderChanged);
            m_WalkSpeedSlider.UnregisterValueChangedCallback(OnWalkSpeedSliderChanged);
            m_SprintSpeedSlider.UnregisterValueChangedCallback(OnSprintSpeedSliderChanged);
            m_JumpForceSlider.UnregisterValueChangedCallback(OnJumpForceSliderChanged);
        }

        void InitializeValues()
        {
            if (m_WalkModeMoveController != null)
            {
                m_ControllerHeightSlider.value = RoundValue(m_WalkModeMoveController.CharacterHeight);
                m_WalkSpeedSlider.value = RoundValue(m_WalkModeMoveController.WalkSpeed);
                m_SprintSpeedSlider.value = RoundValue(m_WalkModeMoveController.SprintSpeed);
                m_JumpForceSlider.value = RoundValue(m_WalkModeMoveController.JumpForce);
            }
            
            SetSliderMinMax(m_ControllerHeightSlider, m_HeightRange.x, m_HeightRange.y);
            SetSliderMinMax(m_WalkSpeedSlider, m_WalkSpeedRange.x, m_WalkSpeedRange.y);
            SetSliderMinMax(m_SprintSpeedSlider, m_SprintSpeedRange.x, m_SprintSpeedRange.y);
            SetSliderMinMax(m_JumpForceSlider, m_JumpForceRange.x, m_JumpForceRange.y);
        }

        static void SetSliderMinMax(SliderFloat slider, float min, float max)
        {
            slider.lowValue = min;
            slider.highValue = max;
        }

        static float RoundValue(float val)
        {
            return Mathf.Round(val * 100f) / 100f;
        }
        
        void OnNavigationModeChanged()
        {
            if (m_NavigationManager.CurrentNavigationModeData.name == k_WalkModeString)
            {
                m_WalkModeMoveController = FindObjectOfType<WalkModeMoveController>();
            }
            RefreshToolState();
        }
        
        void OnHeightSliderChanged(ChangeEvent<float> evt)
        {
            m_WalkModeMoveController.CharacterHeight = evt.newValue;
            m_ControllerHeightSlider.value = RoundValue(evt.newValue);
        }
        
        void OnWalkSpeedSliderChanged(ChangeEvent<float> evt)
        {
            m_WalkModeMoveController.WalkSpeed = evt.newValue;
            m_WalkSpeedSlider.value = RoundValue(evt.newValue);
        }
        
        void OnSprintSpeedSliderChanged(ChangeEvent<float> evt)
        {
            m_WalkModeMoveController.SprintSpeed = evt.newValue;
            m_SprintSpeedSlider.value = RoundValue(evt.newValue);
        }
        
        void OnJumpForceSliderChanged(ChangeEvent<float> evt)
        {
            m_WalkModeMoveController.JumpForce = evt.newValue;
            m_JumpForceSlider.value = RoundValue(evt.newValue);
        }
    }
}
