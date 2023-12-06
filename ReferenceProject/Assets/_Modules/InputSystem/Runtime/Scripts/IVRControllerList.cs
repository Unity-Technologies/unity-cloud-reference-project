using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.InputSystem.VR
{
    /// <summary>
    /// Hosts controller list of VR devices
    /// </summary>
    public interface IVRControllerList
    {
        /// <summary>
        /// Controllers of VR Devices
        /// </summary>
        List<GameObject> Controllers { get; set; }

        /// <summary>
        /// Maps controllers action to XRayInteractors
        /// </summary>
        /// <param name="actionToVRControllers"></param>
        void GetSelectActionFromControllers(Dictionary<InputAction, XRRayInteractor> actionToVRControllers);
    }

    public class ControllerList : IVRControllerList
    {
        public List<GameObject> Controllers { get; set; }

        public void GetSelectActionFromControllers(Dictionary<InputAction, XRRayInteractor> actionToVRControllers)
        {
            foreach (GameObject controllerGo in Controllers)
            {
                ActionBasedController xrController = controllerGo.GetComponent<ActionBasedController>();
                XRRayInteractor xrRayInteractor = controllerGo.GetComponent<XRRayInteractor>();

                if (xrController != null && xrRayInteractor != null)
                {
                    actionToVRControllers.Add(xrController.selectAction.action, xrRayInteractor);
                }
            }
        }
    }
}