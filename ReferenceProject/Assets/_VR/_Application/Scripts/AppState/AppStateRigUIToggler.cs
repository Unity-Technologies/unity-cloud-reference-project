using System;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class AppStateRigUIToggler : AppStateListener
    {
        [SerializeField]
        MenuType m_MenuType;

        [SerializeField]
        bool m_DesactivateOnExit = true;

        PropertyValue<MenuType> m_ActiveMenuType;

        [Inject]
        void Setup(PropertyValue<MenuType> menuType)
        {
            m_ActiveMenuType = menuType;
        }

        protected override void StateEntered()
        {
            m_ActiveMenuType.SetValue(m_MenuType);
        }

        protected override void StateExited()
        {
            if (m_DesactivateOnExit)
            {
                MenuType menuType = null;
                m_ActiveMenuType.SetValue(menuType);
            }
        }
    }
}
