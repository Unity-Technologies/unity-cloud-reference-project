using System;
using Unity.AppUI.Core;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.WorldSpaceUIDocumentExtensions
{
    [RequireComponent(typeof(WorldSpaceUIDocument))]
    public class WorldSpaceUIDocumentCustomFunction : MonoBehaviour
    {
        [Tooltip("If set, this value will override the injected value.")]
        [SerializeField]
        Transform m_ControllerTransform;

        WorldSpaceUIDocument m_WorldSpaceUIDocument;

        PropertyValue<IControllerInfo> m_ControllerInfo;

        [Inject]
        void Setup(PropertyValue<IControllerInfo> controllerInfo)
        {
            m_ControllerInfo = controllerInfo;
        }

        void Awake()
        {
            m_WorldSpaceUIDocument = GetComponent<WorldSpaceUIDocument>();
            if (m_ControllerTransform != null)
            {
                SetCustomFunction(m_ControllerTransform);
            }
            else if(m_ControllerInfo != null)
            {
                var data = m_ControllerInfo.GetValue();
                if(data != null && data.ControllerTransform != null)
                {
                    SetCustomFunction(data.ControllerTransform);
                }

                m_ControllerInfo.ValueChanged += OnControllerInfoChanged;
            }
        }

        void OnDestroy()
        {
            m_ControllerInfo.ValueChanged -= OnControllerInfoChanged;
        }

        void OnControllerInfoChanged(IControllerInfo controllerInfo)
        {
            if(controllerInfo != null)
            {
                SetCustomFunction(controllerInfo.ControllerTransform);
            }
        }

        void SetCustomFunction(Transform controllerTransfrom)
        {
            m_WorldSpaceUIDocument.customRayFunc = () =>
            {
                return new Ray(controllerTransfrom.position, controllerTransfrom.forward);
            };
        }
    }
}
