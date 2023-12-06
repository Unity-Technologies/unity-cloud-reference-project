using System;
using System.Collections.Generic;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.InputSystem.VR;
using Unity.ReferenceProject.VR.RigUI;
using Unity.ReferenceProject.WorldSpaceUIDocumentExtensions;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using UnityInputSystem = UnityEngine.InputSystem.InputSystem;

namespace Unity.ReferenceProject.VR
{
    public class VRInstaller : MonoInstaller
    {
        [SerializeField]
        RigUIController m_RigUIController;

        [SerializeField]
        PanelManager m_PanelManager;

        [SerializeField]
        XROrigin m_XROrigin;

        [SerializeField]
        ControllerInfo m_StartingControllerInfo;

        [SerializeField]
        List<GameObject> m_Controllers = new ();

        Mouse m_Mouse;
        
        public override void InstallBindings()
        {
            Container.Bind<IRigUIController>().FromInstance(m_RigUIController).AsSingle();
            Container.Bind<IPanelManager>().FromInstance(m_PanelManager).AsSingle();
            Container.Bind<XROrigin>().FromInstance(m_XROrigin).AsSingle();

            var controllerList = new ControllerList();
            controllerList.Controllers = m_Controllers;
            Container.Bind<IVRControllerList>().FromInstance(controllerList).AsSingle();

            var controllerStore = gameObject.AddComponent<ControllerStore>();
            var controlProperty = controllerStore.GetProperty<IControllerInfo>(nameof(ControllerViewModel.ControllerInfo));
            controlProperty.SetValue(m_StartingControllerInfo != null ? m_StartingControllerInfo : new ControllerInfo());
            Container.Bind<PropertyValue<IControllerInfo>>().FromInstance(controlProperty);

            var menuTypeStore = gameObject.AddComponent<MenuTypeStore>();
            var menuTypeProperty = menuTypeStore.GetProperty<MenuType>(nameof(MenuTypeViewModel.MenuType));
            Container.Bind<PropertyValue<MenuType>>().FromInstance(menuTypeProperty);

            // Necessary for VR keyboard
            m_Mouse = UnityInputSystem.AddDevice<Mouse>();
            Container.Bind<Mouse>().FromInstance(m_Mouse).AsSingle();
        }

        void OnDestroy()
        {
            Container.Bind<IVRControllerList>().FromInstance(null);
            UnityInputSystem.RemoveDevice(m_Mouse);
        }
    }
}
