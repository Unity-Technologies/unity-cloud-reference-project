using Unity.ReferenceProject.DataStores;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Unity.ReferenceProject.WorldSpaceUIDocumentExtensions
{
    public class WorldSpaceUIDocumentController : MonoBehaviour
    {
        [SerializeField]
        InputActionProperty m_SelectInputAction;

        Transform m_ControllerTransform;
        int k_UIMask;

        PropertyValue<IControllerInfo> m_ControllerInfo;

        [Inject]
        void Setup(PropertyValue<IControllerInfo> controllerInfo)
        {
            m_ControllerInfo = controllerInfo;
        }

        void Awake()
        {
            k_UIMask = LayerMask.GetMask("UI");
            m_ControllerTransform = transform;
        }

        void OnEnable()
        {
            if (m_SelectInputAction.action != null)
            {
                m_SelectInputAction.action.performed += OnSelect;
            }
        }

        void OnDisable()
        {
            if (m_SelectInputAction.action != null)
            {
                m_SelectInputAction.action.performed -= OnSelect;
            }
        }

        void OnSelect(InputAction.CallbackContext obj)
        {
            if(m_ControllerInfo != null &&
               (m_ControllerInfo.GetValue() == null ||
                   m_ControllerInfo.GetValue().ControllerTransform != m_ControllerTransform) &&
               CheckHit())
            {
                m_ControllerInfo.SetValue(new ControllerInfo{ControllerTransform = m_ControllerTransform});
            }
        }

        bool CheckHit()
        {
            var ray = new Ray(m_ControllerTransform.position, m_ControllerTransform.forward);
            return Physics.Raycast(ray, out _, 100, k_UIMask);
        }
    }
}
