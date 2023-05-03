using System;
using Unity.ReferenceProject.VRManager;
using UnityEngine;

namespace Unity.ReferenceProject.VR
{
    public class AppStateRigUIToggler : AppStateListener
    {
        [SerializeField]
        ToolUIMenuVR m_Menu;

        [SerializeField]
        bool m_DesactivateOnExit = true;

        [SerializeField]
        bool m_ClearPanelOnEnter = true;

        protected override void StateEntered()
        {
            if (m_Menu != null)
            {
                m_Menu.Activate(true, m_ClearPanelOnEnter);
            }
        }

        protected override void StateExited()
        {
            if (m_Menu != null && m_DesactivateOnExit)
            {
                m_Menu.Activate(false);
            }
        }
    }
}
