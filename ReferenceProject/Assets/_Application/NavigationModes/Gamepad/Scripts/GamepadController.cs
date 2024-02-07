using System;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Application
{
    public class GamepadController : MonoBehaviour
    {
        [SerializeField]
        string m_NavigationModeName = "WalkMode";
        
        static readonly string k_RightGPBack = "RightGamepadBackground";
        static readonly string k_RightGPHndl = "RightGamepadHandle";
        static readonly string k_LeftGPBack = "LeftGamepadBackground";
        static readonly string k_LeftGPHndl = "LeftGamepadHandle";

        // GamepadSticks
        GamepadStick m_RightGamepadStick;
        GamepadStick m_LeftGamepadStick;

        // InputAsset
        Gamepad m_GamepadInput;
        
        INavigationManager m_NavigationManager;
        VisualElement m_RootVisualElement;

        [Inject]
        void Setup(Gamepad gamepad, INavigationManager navigationManager)
        {
            if (!UnityEngine.Application.isMobilePlatform)
                return;
            
            m_GamepadInput = gamepad;
            m_NavigationManager = navigationManager;
        }

        void Awake()
        {
            if (!UnityEngine.Application.isMobilePlatform)
            {
                gameObject.SetActive(false);
                return;
            }

            m_NavigationManager.NavigationModeChanged += OnNavigationModeChanged;
            m_RootVisualElement = GetComponent<UIDocument>().rootVisualElement;
            m_RightGamepadStick = new GamepadStick(m_RootVisualElement, k_RightGPBack, k_RightGPHndl, m_GamepadInput, true);
            m_LeftGamepadStick = new GamepadStick(m_RootVisualElement, k_LeftGPBack, k_LeftGPHndl, m_GamepadInput, false);
        }

        void OnNavigationModeChanged()
        {
            if (m_NavigationManager.CurrentNavigationModeData.name == m_NavigationModeName)
            {
                Common.Utils.SetVisible(m_RootVisualElement, true);
            }
            else if (m_NavigationManager.CurrentNavigationModeData.name != m_NavigationModeName)
            {
                Common.Utils.SetVisible(m_RootVisualElement, false);
            }
        }

        void Update()
        {
            m_RightGamepadStick?.Update();
            m_LeftGamepadStick?.Update();
        }
        
        void OnDestroy()
        {
            if (!UnityEngine.Application.isMobilePlatform)
                return;
            
            m_NavigationManager.NavigationModeChanged -= OnNavigationModeChanged;
        }
    }
}
