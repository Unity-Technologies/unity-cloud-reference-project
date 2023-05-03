using System;
using Unity.ReferenceProject.Tools;
using UnityEngine;

namespace Unity.ReferenceProject.VRManager
{
    [RequireComponent(typeof(ToolUISubMenuVR))]
    public class SubMenuToolUIController : ToolUIController
    {
        ToolUISubMenuVR m_ToolUISubMenuVR;

        protected override void Awake()
        {
            base.Awake();
            m_ToolUISubMenuVR = GetComponent<ToolUISubMenuVR>();
        }

        public override void OnToolOpened()
        {
            m_ToolUISubMenuVR.Activate(true);
        }

        public override void OnToolClosed()
        {
            m_ToolUISubMenuVR.Activate(false);
        }
    }
}
