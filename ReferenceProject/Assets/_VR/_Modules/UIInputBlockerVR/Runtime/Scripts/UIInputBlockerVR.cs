using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.UIInputBlocker;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;

namespace Unity.ReferenceProject.VR.UIInputBlockerVR
{
    public class UIInputBlockerVR : MonoBehaviour
    {
        [SerializeField]
        LayerMask m_LayerMask;

        IUIInputBlockerEventsDispatcher m_Dispatcher;
        IControllerList m_ControllerList;

        [Inject]
        public void Setup(IUIInputBlockerEventsDispatcher dispatcher, IControllerList controllerList)
        {
            m_Dispatcher = dispatcher;
            m_ControllerList = controllerList;
        }

        void Awake()
        {
            foreach (var controller in m_ControllerList.Controllers)
            {
                var xrController = controller.GetComponent<ActionBasedController>();
                var xrRayInteractor = controller.GetComponent<XRRayInteractor>();

                if (xrController)
                {
                    xrController.selectAction.action.performed += _ =>
                    {
                        OnClickPerformed(xrRayInteractor);
                    };
                }
            }
        }

        void OnClickPerformed(XRRayInteractor xrRayInteractor)
        {
            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit)
                && !Utils.IsInLayerMask(m_LayerMask, raycastHit.collider.gameObject.layer))
            {
                var controllerTransform = xrRayInteractor.gameObject.transform;
                var ray = new Ray(controllerTransform.position, controllerTransform.forward);
                m_Dispatcher?.DispatchRay(ray);
            }
        }
    }
}
