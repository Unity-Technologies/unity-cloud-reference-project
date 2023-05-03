using System;
using UnityEngine;

namespace Unity.ReferenceProject.VR.RigUI
{
    public class FloatingPanelController : PanelController
    {
        [SerializeField]
        FloatingPanelManipulator m_PanelManipulator;

        public override Camera XRCamera
        {
            get => base.XRCamera;
            set
            {
                base.XRCamera = value;
                m_PanelManipulator.FaceTarget = value.transform;
            }
        }

        public override Vector2 PanelSize
        {
            set
            {
                base.PanelSize = value;
                m_PanelManipulator.UpdateSize();
            }
        }

        public override void SetVisible(bool isVisible)
        {
            base.SetVisible(isVisible);
            m_PanelManipulator.gameObject.SetActive(isVisible);
        }

        public void TurnToFace()
        {
            m_PanelManipulator.TurnToFace();
        }
    }
}
