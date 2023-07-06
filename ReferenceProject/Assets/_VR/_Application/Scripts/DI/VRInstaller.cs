using System;
using System.Collections.Generic;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.VR.RigUI;
using Unity.ReferenceProject.VR.UIInputBlockerVR;
using Unity.ReferenceProject.WorldSpaceUIDocumentExtensions;
using Unity.XR.CoreUtils;
using UnityEngine;
using Zenject;

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

        public override void InstallBindings()
        {
            Container.Bind<IRigUIController>().FromInstance(m_RigUIController).AsSingle();
            Container.Bind<IPanelManager>().FromInstance(m_PanelManager).AsSingle();
            Container.Bind<XROrigin>().FromInstance(m_XROrigin).AsSingle();

            var controllerList = new ControllerList();
            controllerList.Controllers = m_Controllers;
            Container.Bind<IControllerList>().FromInstance(controllerList).AsSingle();

            var controllerStore = gameObject.AddComponent<ControllerStore>();
            var controlProperty = controllerStore.GetProperty<IControllerInfo>(nameof(ControllerViewModel.ControllerInfo));
            controlProperty.SetValue(m_StartingControllerInfo != null ? m_StartingControllerInfo : new ControllerInfo());
            Container.Bind<PropertyValue<IControllerInfo>>().FromInstance(controlProperty);

            var menuTypeStore = gameObject.AddComponent<MenuTypeStore>();
            var menuTypeProperty = menuTypeStore.GetProperty<MenuType>(nameof(MenuTypeViewModel.MenuType));
            Container.Bind<PropertyValue<MenuType>>().FromInstance(menuTypeProperty);
        }
    }
}
