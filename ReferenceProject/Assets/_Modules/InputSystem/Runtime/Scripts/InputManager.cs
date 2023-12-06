using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.InputSystem
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        [SerializeField]
        InputUnifier m_InputUnifier = new();

        readonly List<InputScheme> m_Schemes = new ();
        readonly Dictionary<InputSchemeType, InputScheme> m_SchemesByType = new();
        readonly HashSet<InputSchemeCategory> m_DisabledSchemeCategories = new();
        InputScheme m_PriorityScheme = null;

        public IReadOnlyCollection<InputSchemeCategory> GetDisableSchemeCategories() => m_DisabledSchemeCategories;
        public void SetPriorityInputScheme(InputScheme scheme) => m_PriorityScheme = scheme;
        public void UnsetPriorityInputScheme() => m_PriorityScheme = null;
        public InputScheme GetPriorityInputScheme() => m_PriorityScheme;
        public bool IsUIFocused { get; set; }

        public void SetSchemeCategoryState(InputSchemeCategory category, bool state)
        {
            if (state && m_DisabledSchemeCategories.Contains(category))
                m_DisabledSchemeCategories.Remove(category);
            else if (!state && !m_DisabledSchemeCategories.Contains(category))
                m_DisabledSchemeCategories.Add(category);
        }

        public InputScheme GetOrCreateInputScheme(InputSchemeType schemeType, InputSchemeCategory schemeCategory, UnityEngine.InputSystem.InputActionAsset inputActionAsset)
        {
            if (schemeType != InputSchemeType.Other && m_SchemesByType.TryGetValue(schemeType, out InputScheme scheme))
                return scheme;

            scheme = new InputScheme(this, schemeType, schemeCategory, inputActionAsset);
            m_Schemes.Add(scheme);

            if (schemeType != InputSchemeType.Other)
            {
                m_SchemesByType.Add(schemeType, scheme);
            }

            return scheme;
        }

        public InputScheme GetOrCreateInputScheme(InputSchemeType schemeType, InputSchemeCategory schemeCategory, UnityEngine.InputSystem.InputAction[] actions)
        {
            if (schemeType != InputSchemeType.Other && m_SchemesByType.TryGetValue(schemeType, out InputScheme scheme))
                return scheme;

            scheme = new InputScheme(this, schemeType, schemeCategory, actions);
            m_Schemes.Add(scheme);

            if (schemeType != InputSchemeType.Other)
            {
                m_SchemesByType.Add(schemeType, scheme);
            }

            return scheme;
        }

        internal void RemoveInputScheme(InputScheme scheme)
        {
            if (m_SchemesByType.ContainsKey(scheme.SchemeType))
            {
                m_SchemesByType.Remove(scheme.SchemeType);
            }
            if (m_PriorityScheme == scheme)
            {
                m_PriorityScheme = null;
            }
            m_Schemes.Remove(scheme);

            foreach (InputActionWrapper wrapper in scheme.InputActionWrappers)
            {
                m_InputUnifier.Unregister(wrapper);
            }
        }

        void Awake()
        {
            m_InputUnifier.Initialize();
            UnityEngine.InputSystem.InputSystem.onAfterUpdate += CustomUpdate;
        }

        void OnDestroy()
        {
            UnityEngine.InputSystem.InputSystem.onAfterUpdate -= CustomUpdate;
        }

        internal void RegisterOverridenClickAction(InputActionWrapper actionWrapper, bool isDoubleClick = false)
        {
            m_InputUnifier.RegisterOverridenClickAction(actionWrapper, isDoubleClick);
        }

        internal void RegisterOverridenTouchAction(InputActionWrapper actionWrapper, bool isDoubleTouch = false)
        {
            m_InputUnifier.RegisterOverridenTouchAction(actionWrapper, isDoubleTouch);
        }

        void CustomUpdate()
        {
            foreach (InputScheme scheme in m_Schemes)
            {
                if (m_DisabledSchemeCategories.Contains(scheme.SchemeCategory) || !scheme.IsEnabled)
                    continue;

                scheme.Update();
            }
        }
    }
}