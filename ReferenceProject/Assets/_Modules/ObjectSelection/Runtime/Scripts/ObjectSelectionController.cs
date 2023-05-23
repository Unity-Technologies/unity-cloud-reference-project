using System;
using System.Threading.Tasks;
using Unity.ReferenceProject.UIInputBlocker;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using Zenject;

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
                DispatchSelection(null);
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
                var selected = await m_Picker.PickFromRayAsync(ray);

                // if at the middle of the DataStreamingPicking process user exit Select Mode then stop
                // or selection is empty
                if (!m_IsActive || selected == null || selected.Count == 0)
                {
                    DispatchSelection(null);
                }
                else
                {
                    var target = selected[0].Item1;

                    // TODO: add HLOD detection and empty metadata check

                    DispatchSelection(target);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void DispatchSelection(GameObject selectedGameObject)
        {
            var data = m_SelectedGameObjectInfo.GetValue();
            data.SelectedGameObject = selectedGameObject;
            m_SelectedGameObjectInfo.SetValue(data);
        }
    }
}
