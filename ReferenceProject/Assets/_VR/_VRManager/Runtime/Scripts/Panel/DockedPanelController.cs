using System;
using UnityEngine;

namespace Unity.ReferenceProject.VRManager
{
    public class DockedPanelController : PanelController
    {
        [SerializeField]
        Transform m_DockPoint;

        public Transform DockPoint
        {
            get => m_DockPoint;
            set
            {
                m_DockPoint = value;
                transform.SetParent(value, false);
            }
        }
    }
}
