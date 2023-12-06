using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.InputSystem
{
    [System.Serializable]
    class InputUnifier
    {
        const string k_ClickPath = "<Mouse>/leftButton";
        const string k_TouchPath = "<Touchscreen>/primaryTouch/tap";

        readonly Dictionary<InputAction, List<InputActionWrapper>> m_OverridenTapAction = new();
        readonly Dictionary<InputAction, List<InputActionWrapper>> m_OverridenDoubleTapAction = new();

        InputAction m_ClickAction = null;
        InputAction m_TouchAction = null;

        [SerializeField]
        float m_MultiTapDelay = 0.3f;

        [SerializeField]
        bool m_UseDefaultMultiTapDelay = false;

        static internal HashSet<string> GetBindingTypes(IEnumerable<InputBinding> bindings, out bool doubleClickBinding, out bool singleClickBinding, out bool doubleTouchBinding, out bool singleTouchBinding)
        {
            HashSet<string> types = new HashSet<string>();
            doubleClickBinding = false;
            singleClickBinding = false;
            doubleTouchBinding = false;
            singleTouchBinding = false;

            foreach (InputBinding inputBinding in bindings)
            {
                types.Add(inputBinding.path);
                if (inputBinding.path == k_ClickPath && !inputBinding.isPartOfComposite)
                {
                    GetClickBindingType(inputBinding, ref doubleClickBinding, ref singleClickBinding);
                }
                else if (inputBinding.path == k_TouchPath && !inputBinding.isPartOfComposite)
                {
                    GetTouchBindingType(inputBinding, ref doubleTouchBinding, ref singleTouchBinding);
                }
            }
            return types;
        }

        static void GetClickBindingType(in InputBinding inputBinding, ref bool doubleClickBinding, ref bool singleClickBinding)
        {
            if (!doubleClickBinding)
            {
                doubleClickBinding = inputBinding.interactions.Contains("MultiTap");
            }

            if (!singleClickBinding)
            {
                singleClickBinding = !inputBinding.interactions.Contains("MultiTap");
            }
        }

        static void GetTouchBindingType(in InputBinding inputBinding, ref bool doubleTouchBinding, ref bool singleTouchBinding)
        {
            if (!doubleTouchBinding)
            {
                doubleTouchBinding = inputBinding.interactions.Contains("MultiTap");
            }

            if (!singleTouchBinding)
            {
                singleTouchBinding = !inputBinding.interactions.Contains("MultiTap");
            }
        }

        internal void Initialize()
        {
            float doubleClickActualDelay = m_UseDefaultMultiTapDelay ? UnityEngine.InputSystem.InputSystem.settings.multiTapDelayTime : m_MultiTapDelay;

            m_ClickAction = new InputAction("ClickAction", InputActionType.Button, k_ClickPath, $"MultiTap(tapDelay={doubleClickActualDelay})");
            m_ClickAction.started += DoubleTapStarted;
            m_ClickAction.performed += DoubleTapPerformed;
            m_ClickAction.canceled += DoubleTapCanceled;
            m_ClickAction.Enable();

            m_OverridenTapAction.Add(m_ClickAction, new List<InputActionWrapper>());
            m_OverridenDoubleTapAction.Add(m_ClickAction, new List<InputActionWrapper>());

            m_TouchAction = new InputAction("TouchAction", InputActionType.Button, k_TouchPath, $"MultiTap(tapDelay={doubleClickActualDelay})");
            m_TouchAction.started += DoubleTapStarted;
            m_TouchAction.performed += DoubleTapPerformed;
            m_TouchAction.canceled += DoubleTapCanceled;
            m_TouchAction.Enable();

            m_OverridenTapAction.Add(m_TouchAction, new List<InputActionWrapper>());
            m_OverridenDoubleTapAction.Add(m_TouchAction, new List<InputActionWrapper>());
        }

        internal void RegisterOverridenClickAction(InputActionWrapper actionWrapper, bool isDoubleClick = false)
        {
            if (isDoubleClick)
            {
                m_OverridenDoubleTapAction[m_ClickAction].Add(actionWrapper);
            }
            else
            {
                m_OverridenTapAction[m_ClickAction].Add(actionWrapper);
            }
        }

        internal void Unregister(InputActionWrapper actionWrapper)
        {
            m_OverridenDoubleTapAction[m_ClickAction].Remove(actionWrapper);
            m_OverridenTapAction[m_ClickAction].Remove(actionWrapper);
            m_OverridenDoubleTapAction[m_TouchAction].Remove(actionWrapper);
            m_OverridenTapAction[m_TouchAction].Remove(actionWrapper);
        }

        internal void RegisterOverridenTouchAction(InputActionWrapper actionWrapper, bool isDoubleTouch = false)
        {
            if (isDoubleTouch)
            {
                m_OverridenDoubleTapAction[m_TouchAction].Add(actionWrapper);
            }
            else
            {
                m_OverridenTapAction[m_TouchAction].Add(actionWrapper);
            }
        }

        void DoubleTapStarted(InputAction.CallbackContext context)
        {
            foreach (InputActionWrapper action in m_OverridenDoubleTapAction[context.action])
            {
                action.Started(context);
            }

            foreach (InputActionWrapper action in m_OverridenTapAction[context.action])
            {
                action.Started(context);
            }
        }

        void DoubleTapPerformed(InputAction.CallbackContext context)
        {
            foreach (InputActionWrapper action in m_OverridenDoubleTapAction[context.action])
            {
                action.Performed(context);
            }

            foreach (InputActionWrapper action in m_OverridenTapAction[context.action])
            {
                action.Cancel(context);
            }
        }

        void DoubleTapCanceled(InputAction.CallbackContext context)
        {
            foreach (InputActionWrapper action in m_OverridenDoubleTapAction[context.action])
            {
                action.Cancel(context);
            }

            bool singleClickPerformed = true;

            //DoubleClick canceled because button is still pressed after double click delay is passed
            if (Pointer.current.press.isPressed)
            {
                singleClickPerformed = false;
            }

            foreach (InputActionWrapper action in m_OverridenTapAction[context.action])
            {
                if (singleClickPerformed)
                {
                    action.Performed(context);
                }
                action.Cancel(context);
            }
        }
    }
}
