using System.Collections.Generic;
using Unity.ReferenceProject.Annotation;
using Unity.ReferenceProject.InputSystem;
using Unity.ReferenceProject.InputSystem.VR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class AnnotationToolUIControllerVR : AnnotationToolUIController
    {
        InputScheme[] m_InputSchemeVR;
        Dictionary<InputAction, XRRayInteractor> m_ActionToVRControllers;

        IInputManager m_InputManager;

        [Inject]
        void Setup(IInputManager inputManager, IVRControllerList vrControllers)
        {
            m_InputManager = inputManager;
            SetupInputs(vrControllers);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_InputSchemeVR != null)
            {
                foreach (InputScheme inputScheme in m_InputSchemeVR)
                {
                    inputScheme.Dispose();
                }
            }
        }

        public override void OnToolOpened()
        {
            base.OnToolOpened();
            if (m_InputSchemeVR != null)
            {
                foreach (InputScheme inputScheme in m_InputSchemeVR)
                {
                    inputScheme.SetEnable(true);
                }
            }
        }

        public override void OnToolClosed()
        {
            base.OnToolClosed();
            if (m_InputSchemeVR != null)
            {
                foreach (InputScheme inputScheme in m_InputSchemeVR)
                {
                    inputScheme.SetEnable(false);
                }
            }
        }

        void SetupInputs(IVRControllerList vrControllers)
        {
            if (vrControllers == null)
            {
                SetupInputs();
            }
            else
            {
                m_ActionToVRControllers = new Dictionary<InputAction, XRRayInteractor>();
                vrControllers.GetSelectActionFromControllers(m_ActionToVRControllers);
                SetupVRInputs(vrControllers);
            }
        }

        void SetupVRInputs(IVRControllerList vrControllers)
        {
            List<InputScheme> inputSchemes = new List<InputScheme>();

            foreach (GameObject controller in vrControllers.Controllers)
            {
                ActionBasedController xrController = controller.GetComponent<ActionBasedController>();
                XRRayInteractor xrRayInteractor = controller.GetComponent<XRRayInteractor>();

                if (xrController == null || xrRayInteractor == null)
                    continue;

                InputScheme inputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.Other, InputSchemeCategory.Tools, new InputAction[] { xrController.selectAction.action });
                inputScheme[xrController.selectAction.action.name].OnStarted += AnnotationSelectionStartedVR;
                inputScheme[xrController.selectAction.action.name].OnCanceled += AnnotationSelectionCanceled;
                inputScheme[xrController.selectAction.action.name].OnPerformed += PerformSelection;
                inputScheme[xrController.selectAction.action.name].IsUIPointerCheckEnabled = true;
                inputSchemes.Add(inputScheme);
            }

            m_InputSchemeVR = inputSchemes.ToArray();
        }

        void AnnotationSelectionStartedVR(InputAction.CallbackContext context)
        {
            m_SelectionStarted = true;

            if (m_ActionToVRControllers.TryGetValue(context.action, out XRRayInteractor controller))
            {
                m_SelectionRay = new Ray(controller.transform.position, controller.transform.forward);
            }
        }
    }
}
