using System;
using System.Threading.Tasks;
using Unity.ReferenceProject.UIInputBlocker;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using RaycastResult = Unity.Cloud.DataStreaming.Runtime.RaycastResult;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class ObjectSelectionController : MonoBehaviour
    {
        IUIInputBlockerEvents m_ClickEventDispatcher;

        bool m_IsActive;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        IObjectPicker m_Picker;

        Task m_PickTask;
        PropertyValue<IObjectSelectionInfo> m_SelectedGameObjectInfo;

        [Inject]
        void Setup(PropertyValue<IObjectSelectionInfo> selectionInfo, ObjectSelectionActivator objectSelectionActivator, IObjectPicker picker, IUIInputBlockerEvents clickEventDispatcher)
        {
            m_SelectedGameObjectInfo = selectionInfo;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_ClickEventDispatcher = clickEventDispatcher;
            m_ClickEventDispatcher.OnDispatchRay += PerformPick;

            m_ObjectSelectionActivator.OnActivate += SetEnableTool;

            m_Picker = picker;
        }

        void OnDestroy()
        {
            if (m_ObjectSelectionActivator != null)
                m_ObjectSelectionActivator.OnActivate -= SetEnableTool;

            if (m_ClickEventDispatcher != null)
                m_ClickEventDispatcher.OnDispatchRay -= PerformPick;
        }

        void SetEnableTool(bool isEnable)
        {
            m_IsActive = isEnable;

            if (!isEnable)
                DispatchSelection(RaycastResult.Invalid);
        }

        void PerformPick(Ray ray)
        {
            if (!m_ObjectSelectionActivator.IsActive)
                return;

            if (m_PickTask?.IsCompleted ?? true)
                m_PickTask = PickFromRayAsync(ray);
        }

        async Task PickFromRayAsync(Ray ray)
        {
            try
            {
                var raycastResult = await m_Picker.RaycastAsync(ray);

                // Check if we hit something closer than the raycast result
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.distance < raycastResult.Distance)
                    return;

                DispatchSelection(raycastResult);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void DispatchSelection(RaycastResult raycastResult)
        {
            var data = m_SelectedGameObjectInfo.GetValue();
            data.SelectedInstanceId = raycastResult.InstanceId;
            data.SelectedPosition = raycastResult.Point;
            m_SelectedGameObjectInfo.SetValue(data);
        }
    }
}
