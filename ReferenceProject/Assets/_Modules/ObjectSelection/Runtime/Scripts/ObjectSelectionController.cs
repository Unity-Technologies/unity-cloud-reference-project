using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.InputSystem;
using Unity.ReferenceProject.InputSystem.VR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;
using RaycastResult = Unity.Cloud.DataStreaming.Runtime.RaycastResult;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class ObjectSelectionController : MonoBehaviour
    {
        const string k_MouseSelectActionKey = "<Mouse>/leftButton";
        const string k_TouchSelectActionKey = "<Touchscreen>/primaryTouch/tap";
        const string k_MouseSelectAction = "ClickAction";
        const string k_TouchSelectAction = "TouchAction";

        ObjectSelectionActivator m_ObjectSelectionActivator;
        IObjectPicker m_Picker;
        IInputManager m_InputManager;

        Dictionary<InputAction, XRRayInteractor> m_ActionToVRControllers = null;

        Task m_PickTask;
        PropertyValue<IObjectSelectionInfo> m_SelectedGameObjectInfo;

        InputScheme m_InputScheme;
        InputScheme[] m_InputSchemeVR;
        ICameraProvider m_CameraProvider;

        bool m_IsValidPicking = false;
        Vector2 m_PointerPosition;

        [Inject]
        void Setup(PropertyValue<IObjectSelectionInfo> selectionInfo, ObjectSelectionActivator objectSelectionActivator, IObjectPicker picker, IInputManager inputManager, IVRControllerList controllers, ICameraProvider cameraProvider)
        {
            m_SelectedGameObjectInfo = selectionInfo;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_CameraProvider = cameraProvider;
            m_InputManager = inputManager;

            m_ObjectSelectionActivator.SetActivated += OnSetActivatedTool;

            m_Picker = picker;
            SetupInputs(controllers);
        }

        void SetupInputs(IVRControllerList vrControllers)
        {
            if(vrControllers != null)
            {
                m_ActionToVRControllers = new Dictionary<InputAction, XRRayInteractor>();
                vrControllers.GetSelectActionFromControllers(m_ActionToVRControllers);
                SetupVRInputs(vrControllers);
            }
            else
            {
                SetupRegularInputs();
            }
        }

        void SetupRegularInputs()
        {
            List<InputAction> actions = new List<InputAction>();

            InputAction clickAction = new InputAction(k_MouseSelectAction, InputActionType.Button, k_MouseSelectActionKey, "");
            InputAction touchAction = new InputAction(k_TouchSelectAction, InputActionType.Button, k_TouchSelectActionKey, "");
            actions.Add(touchAction);

            m_InputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.ObjectSelection, InputSchemeCategory.Tools, new InputAction[] { clickAction, touchAction });

            m_InputScheme[k_MouseSelectAction].OnStarted += StartPicking;
            m_InputScheme[k_MouseSelectAction].OnCanceled += CancelPicking;
            m_InputScheme[k_MouseSelectAction].OnPerformed += PerformPick;
            m_InputScheme[k_MouseSelectAction].RegisterValidationPeformedFunc(IsPickingValid);
            m_InputScheme[k_MouseSelectAction].IsUIPointerCheckEnabled = true;

            m_InputScheme[k_TouchSelectAction].OnStarted += StartPicking;
            m_InputScheme[k_TouchSelectAction].OnCanceled += CancelPicking;
            m_InputScheme[k_TouchSelectAction].OnPerformed += PerformPick;
            m_InputScheme[k_TouchSelectAction].RegisterValidationPeformedFunc(IsPickingValid);
            m_InputScheme[k_TouchSelectAction].IsUIPointerCheckEnabled = true;
        }

        void StartPicking(InputAction.CallbackContext _)
        {
            m_IsValidPicking = true;
            m_PointerPosition = Pointer.current.position.ReadValue();
        }

        bool IsPickingValid(InputAction.CallbackContext _)
        {
            return m_IsValidPicking;
        }

        void CancelPicking(InputAction.CallbackContext _)
        {
            m_IsValidPicking = false;
        }

        void OnDestroy()
        {
            if (m_ObjectSelectionActivator != null)
                m_ObjectSelectionActivator.SetActivated -= OnSetActivatedTool;

            m_InputScheme?.Dispose();

            if(m_InputSchemeVR != null)
            {
                foreach(InputScheme inputScheme in m_InputSchemeVR)
                {
                    inputScheme.Dispose();
                }
            }
        }

        void SetupVRInputs(IVRControllerList controllers)
        {
            List<InputScheme> inputSchemes = new List<InputScheme>();

            foreach (GameObject controller in controllers.Controllers)
            {
                ActionBasedController xrController = controller.GetComponent<ActionBasedController>();
                XRRayInteractor xrRayInteractor = controller.GetComponent<XRRayInteractor>();

                if (xrController == null || xrRayInteractor == null)
                    continue;

                InputScheme inputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.Other, InputSchemeCategory.Tools, new InputAction[] { xrController.selectAction.action });
                inputScheme[xrController.selectAction.action.name].OnPerformed += PerformPickVR;
                inputScheme[xrController.selectAction.action.name].IsUIPointerCheckEnabled = true;
                inputSchemes.Add(inputScheme);
            }
            m_InputSchemeVR = inputSchemes.ToArray();
        }

        void OnSetActivatedTool(bool isEnable)
        {
            if (!isEnable)
                DispatchSelection(PickerResult.Invalid);
            m_InputScheme?.SetEnable(isEnable);
            if(m_InputSchemeVR != null)
            {
                foreach(InputScheme inputScheme in m_InputSchemeVR)
                {
                    inputScheme.SetEnable(isEnable);
                }
            }
        }

        void PerformPickVR(InputAction.CallbackContext context)
        {
            if (!m_ObjectSelectionActivator.IsActive)
                return;

            if (m_ActionToVRControllers.TryGetValue(context.action, out XRRayInteractor controller))
            {
                Ray ray = new Ray(controller.transform.position, controller.transform.forward);

                if (m_PickTask?.IsCompleted ?? true)
                    m_PickTask = PickFromRayAsync(ray);
            }
        }

        void PerformPick(InputAction.CallbackContext context)
        {
            var ray = m_CameraProvider.Camera.ScreenPointToRay(m_PointerPosition);

            if (!m_ObjectSelectionActivator.IsActive)
                return;

            if (m_PickTask?.IsCompleted ?? true)
                m_PickTask = PickFromRayAsync(ray);
        }

        async Task PickFromRayAsync(Ray ray)
        {
            try
            {
                var pickerResult = await m_Picker.PickAsync(ray);

                // Check if we hit something closer than the raycast result
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.distance < pickerResult.Distance)
                    return;

                DispatchSelection(pickerResult);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void DispatchSelection(IPickerResult pickResult)
        {
            var data = new ObjectSelectionInfo(pickResult.HasIntersected, pickResult.InstanceId, 
                pickResult.Point, pickResult.Normal);
            m_SelectedGameObjectInfo.SetValue(data);
        }
    }
}
