using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.InputSystem
{
    internal struct DelayActionInputCheck
    {
        public bool isCancelRequested;
        public InputActionWrapper inputActionWrapper;
        public InputAction.CallbackContext inputContext;
        public Action<InputAction.CallbackContext> propagationCallback;
    }

    /// <summary>
    /// The InputScheme is close to an InputActionAsset as it host wrappers of InputAction and
    /// is responsible to check if delayed InputActionWrapper should be triggered.
    /// </summary>
    public sealed class InputScheme : IDisposable
    {
        bool m_IsEnabled;
        readonly InputSchemeType m_SchemeType;
        readonly InputSchemeCategory m_SchemeCategory;
        readonly InputManager m_InputManager;
        readonly HashSet<string> m_BindingPaths = new();
        readonly Dictionary<string, InputActionWrapper> m_InputActions = new();
        readonly Queue<DelayActionInputCheck> m_PendingAcionInputChecks = new();
        readonly InputActionAsset m_InputActionAsset;

        /// <summary>
        /// The InputScheme type
        /// </summary>
        public InputSchemeType SchemeType => m_SchemeType;

        /// <summary>
        /// The InputScheme category
        /// </summary>
        public InputSchemeCategory SchemeCategory => m_SchemeCategory;

        /// <summary>
        /// Return the enable state of this scheme
        /// </summary>
        public bool IsEnabled => m_IsEnabled;

        /// <summary>
        /// References all uniques bindings used by InputActions in this scheme
        /// </summary>
        public IReadOnlyCollection<string> UniqueBindingPaths => m_BindingPaths;

        /// <summary>
        /// List of all wrappers of InputAction hosted by this scheme
        /// </summary>
        public IReadOnlyCollection<InputActionWrapper> InputActionWrappers => m_InputActions.Values;

        internal InputScheme(IInputManager inputManager, InputSchemeType schemeType, InputSchemeCategory schemeCategory, InputActionAsset inputActionAsset)
        {
            m_SchemeType = schemeType;
            m_SchemeCategory = schemeCategory;
            m_InputManager = inputManager as InputManager;
            BuildActionWrappers(inputActionAsset);
            m_InputActionAsset = inputActionAsset;
        }

        internal InputScheme(IInputManager inputManager, InputSchemeType schemeType, InputSchemeCategory schemeCategory, InputAction[] actions)
        {
            m_SchemeType = schemeType;
            m_SchemeCategory = schemeCategory;
            m_InputManager = inputManager as InputManager;
            BuildActionWrappers(actions);
        }

        /// <summary>
        /// Enables or disable all InputAction in this scheme to trigger
        /// </summary>
        public void SetEnable(bool state)
        {
            m_IsEnabled = state;
            if (state)
                m_InputActionAsset?.Enable();
            else
                m_InputActionAsset?.Disable();
        }

        public InputActionWrapper this[string actionName]
        {
            get => m_InputActions[actionName];
        }

        internal void Update()
        {
            while (m_PendingAcionInputChecks.TryDequeue(out DelayActionInputCheck action))
            {
                if (IsActionEligibleForTrigger(action.inputActionWrapper, action.isCancelRequested))
                {
                    action.propagationCallback.Invoke(action.inputContext);
                }
            }

            m_PendingAcionInputChecks.Clear();
        }

        public bool IsActionEligibleForTrigger(InputActionWrapper inputActionWrapper, bool isCancelRequested = false)
        {
            if (!inputActionWrapper.IsEnabled)
                return false;

            //For cancel we don't check UI. And no need to check for priority because cancel will check if event was started.
            if (isCancelRequested)
                return true;

            if (inputActionWrapper.IsUIPointerCheckEnabled && EventSystem.current.IsPointerOverGameObject(-1))
                return false;

            if (inputActionWrapper.IsUISelectionCheckEnabled && m_InputManager.IsUIFocused)
                return false;

            return CheckPriorityInput(inputActionWrapper.InputAction);
        }

        public bool IsSchemeEligibleForInputs()
        {
            return !m_InputManager.GetDisableSchemeCategories().Contains(m_SchemeCategory) && m_IsEnabled;
        }

        internal bool CheckPriorityInput(InputAction refAction)
        {
            InputScheme priorityScheme = m_InputManager.GetPriorityInputScheme();
            if (priorityScheme != this && priorityScheme != null)
            {
                foreach (InputBinding binding in refAction.bindings)
                {
                    if (priorityScheme.UniqueBindingPaths.Contains(binding.path))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        void RegisterDelayInputCheck(DelayActionInputCheck delayInputCheckElement)
        {
            m_PendingAcionInputChecks.Enqueue(delayInputCheckElement);
        }

        void BuildActionWrappers(IEnumerable<InputAction> actions)
        {
            foreach (InputAction inputAction in actions)
            {
                m_InputActions.Add(inputAction.name, BuildInputActionWrapper(inputAction));
            }
        }

        InputActionWrapper BuildInputActionWrapper(InputAction inputAction)
        {
            m_BindingPaths.UnionWith(InputUnifier.GetBindingTypes(inputAction.bindings, out bool doubleClickBinding, out bool singleClickBinding, out bool doubleTouchBinding, out bool singleTouchBinding));

            InputActionWrapper actionWrapper = new InputActionWrapper(inputAction, this, RegisterDelayInputCheck, doubleClickBinding || singleClickBinding || doubleTouchBinding || singleTouchBinding);

            if (singleClickBinding || doubleClickBinding)
            {
                m_InputManager.RegisterOverridenClickAction(actionWrapper, doubleClickBinding);
            }
            if (singleTouchBinding || doubleTouchBinding)
            {
                m_InputManager.RegisterOverridenTouchAction(actionWrapper, doubleTouchBinding);
            }                

            return actionWrapper;
        }

        /// <summary>
        /// Should be used when this scheme is no longer needed.
        /// All wrappers will be unbinded from InputActions and the Scheme will unregister from the InputManager
        /// </summary>
        public void Dispose()
        {
            m_InputManager.RemoveInputScheme(this);
            foreach (KeyValuePair<string, InputActionWrapper> pair in m_InputActions)
            {
                pair.Value.Reset();
                pair.Value.Unbind();
                pair.Value.InputAction.Dispose();
            }
        }
    }
}