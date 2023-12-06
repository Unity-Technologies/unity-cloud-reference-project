using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.InputSystem
{
    /// <summary>
    /// Wrapper for InputAction to intercept inputs and validate the trigger
    /// </summary>
    public class InputActionWrapper
    {
        const string k_leftButtonClick = "Button:/Mouse/leftButton";
        const string k_touchPrimary = "Button:/Touchscreen/primaryTouch";

        readonly bool m_IsClickOverriden = false;
        readonly InputAction m_InputAction;
        readonly InputScheme m_AttachedScheme;
        readonly Action<DelayActionInputCheck> m_DelayCheckCallback;

        readonly HashSet<Func<InputAction.CallbackContext, bool>> m_ValidationStartedFuncs = new();
        readonly HashSet<Func<InputAction.CallbackContext, bool>> m_ValidationPerformedFuncs = new();
        readonly HashSet<Func<InputAction.CallbackContext, bool>> m_ValidationCancelFuncs = new();

        bool m_IsUIPointerCheckEnabled = false;
        bool m_IsUISelectionCheckEnabled = false;
        bool m_WasStartedOrPerformed = false;

        /// <summary>
        /// Should the InputAction trigger be delayed to the afterUpdate
        /// </summary>
        public bool IsDelayedInput { get; set; }

        /// <summary>
        /// Should the InputAction trigger when input event is raised
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Should a check of the pointer over the UI happen during validation.
        /// (If this flag is set to true, IsDelayedInput will be set to true)
        /// </summary>
        public bool IsUIPointerCheckEnabled
        {
            get => m_IsUIPointerCheckEnabled;
            set
            {
                if (value)
                {
                    IsDelayedInput = true;
                }
                m_IsUIPointerCheckEnabled = value;
            }
        }

        /// <summary>
        /// Should a check of UI selection state happen during validation. (Linked to InputManager.IsUIFocused)
        /// (If this flag is set to true, IsDelayedInput will be set to true)
        /// </summary>
        public bool IsUISelectionCheckEnabled
        {
            get => m_IsUISelectionCheckEnabled;
            set
            {
                if (value)
                    IsDelayedInput = true;
                m_IsUISelectionCheckEnabled = value;
            }
        }

        /// <summary>
        /// The source InputAction
        /// </summary>
        public InputAction InputAction => m_InputAction;

        /// <summary>
        /// The InputScheme that contains this wrapper
        /// </summary>
        public InputScheme AttachedScheme => m_AttachedScheme;

        /// <summary>
        /// Will trigger on InputAction.start if all requirements are met
        /// </summary>
        public event Action<InputAction.CallbackContext> OnStarted;

        /// <summary>
        /// Will trigger on InputAction.performed if all requirements are met
        /// </summary>
        public event Action<InputAction.CallbackContext> OnPerformed;

        /// <summary>
        /// Will trigger on InputAction.canceled if all requirements are met
        /// </summary>
        public event Action<InputAction.CallbackContext> OnCanceled;

        internal InputActionWrapper(InputAction inputAction, InputScheme attachedScheme, Action<DelayActionInputCheck> delayCheckCallback, bool overridenAction = false)
        {
            m_InputAction = inputAction;
            m_AttachedScheme = attachedScheme;
            m_DelayCheckCallback = delayCheckCallback;

            m_IsClickOverriden = overridenAction;

            if (!overridenAction)
            {
                m_InputAction.Enable();

                m_InputAction.started += Started;
                m_InputAction.performed += Performed;
                m_InputAction.canceled += Cancel;
            }
            else
            {
                m_InputAction.Disable();
            }

            IsEnabled = true;
        }

        internal void Unbind()
        {
            m_InputAction.started -= Started;
            m_InputAction.performed -= Performed;
            m_InputAction.canceled -= Cancel;
        }

        /// <summary>
        /// Register a validation function executed before a start event is propagated to validated or not the propagation
        /// </summary>
        /// <param name="func"></param>
        public void RegisterValidationStartedFunc(Func<InputAction.CallbackContext, bool> func)
        {
            m_ValidationStartedFuncs.Add(func);
        }

        /// <summary>
        /// Register a validation function executed before a performed event is propagated to validated or not the propagation
        /// </summary>
        /// <param name="func"></param>
        public void RegisterValidationPeformedFunc(Func<InputAction.CallbackContext, bool> func)
        {
            m_ValidationPerformedFuncs.Add(func);
        }

        /// <summary>
        /// Register a validation function executed before a canceled event is propagated to validated or not the propagation
        /// </summary>
        /// <param name="func"></param>
        public void RegisterValidationCancelFunc(Func<InputAction.CallbackContext, bool> func)
        {
            m_ValidationCancelFuncs.Add(func);
        }

        /// <summary>
        /// Unregister a validation function from the start event
        /// </summary>
        /// <param name="func"></param>
        public void UnRegisterValidationStartedFunc(Func<InputAction.CallbackContext, bool> func)
        {
            m_ValidationStartedFuncs.Remove(func);
        }

        /// <summary>
        /// Unregister a validation function from the performed event
        /// </summary>
        /// <param name="func"></param>
        public void UnRegisterValidationPeformedFunc(Func<InputAction.CallbackContext, bool> func)
        {
            m_ValidationPerformedFuncs.Remove(func);
        }

        /// <summary>
        /// Unregister a validation function from the canceled event
        /// </summary>
        /// <param name="func"></param>
        public void UnRegisterValidationCancelFunc(Func<InputAction.CallbackContext, bool> func)
        {
            m_ValidationCancelFuncs.Remove(func);
        }

        /// <summary>
        /// Calls the InputAction.Reset
        /// </summary>
        public void Reset()
        {
            m_InputAction.Reset();
        }

        bool CheckValidation(InputAction.CallbackContext context, HashSet<Func<InputAction.CallbackContext, bool>> funcList)
        {
            bool isValid = true;
            foreach (Func<InputAction.CallbackContext, bool> func in funcList)
            {
                isValid = func.Invoke(context);
                if (!isValid)
                    return false;
            }

            return true;
        }

        bool IsControlOverridden(InputControl control)
        {
            return control.ToString() == k_leftButtonClick || control.ToString() == k_touchPrimary;
        }

        internal void Started(InputAction.CallbackContext context)
        {
            if (IsControlOverridden(context.control) && m_IsClickOverriden && context.action == m_InputAction)
                return;

            if (IsDelayedInput)
            {
                m_DelayCheckCallback.Invoke(new DelayActionInputCheck() { inputActionWrapper = this, inputContext = context, propagationCallback = PropagateStarted });
            }
            else if (IsInputEligible() && IsEnabled)
            {
                PropagateStarted(context);
            }
        }

        void PropagateStarted(InputAction.CallbackContext context)
        {
            if (!CheckValidation(context, m_ValidationStartedFuncs))
                return;
            
            if (!IsEnabled)
                return;

            m_WasStartedOrPerformed = true;
            OnStarted?.Invoke(context);
        }

        internal void Performed(InputAction.CallbackContext context)
        {
            if (IsControlOverridden(context.control) && m_IsClickOverriden && context.action == m_InputAction)
                return;

            if (IsDelayedInput)
            {
                m_DelayCheckCallback.Invoke(new DelayActionInputCheck() { inputActionWrapper = this, inputContext = context, propagationCallback = PropagatePerformed });
            }
            else if (IsInputEligible() && IsEnabled)
            {
                PropagatePerformed(context);
            }
        }

        void PropagatePerformed(InputAction.CallbackContext context)
        {
            if (!CheckValidation(context, m_ValidationPerformedFuncs))
                return;

            if (!IsEnabled)
                return;

            m_WasStartedOrPerformed = true;
            OnPerformed?.Invoke(context);
        }

        internal void Cancel(InputAction.CallbackContext context)
        {
            if (IsControlOverridden(context.control) && m_IsClickOverriden && context.action == m_InputAction)
                return;

            if (IsDelayedInput)
            {
                m_DelayCheckCallback.Invoke(new DelayActionInputCheck() { inputActionWrapper = this, inputContext = context, propagationCallback = PropagateCancel, isCancelRequested = true });
            }
            else if (IsInputEligible() && IsEnabled)
            {
                PropagateCancel(context);
            }
        }

        void PropagateCancel(InputAction.CallbackContext context)
        {
            if (!CheckValidation(context, m_ValidationCancelFuncs))
                return;

            if (!IsEnabled || !m_WasStartedOrPerformed)
                return;

            m_WasStartedOrPerformed = false;
            OnCanceled?.Invoke(context);
        }

        bool IsInputEligible()
        {
            return m_AttachedScheme.CheckPriorityInput(m_InputAction) && m_AttachedScheme.IsSchemeEligibleForInputs();
        }
    }
}