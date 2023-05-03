using System;
using Unity.ReferenceProject.VRManager;
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

        public override void InstallBindings()
        {
            Container.Bind<IRigUIController>().FromInstance(m_RigUIController).AsSingle();
            Container.Bind<IPanelManager>().FromInstance(m_PanelManager).AsSingle();
            Container.Bind<XROrigin>().FromInstance(m_XROrigin).AsSingle();
        }
    }
}
