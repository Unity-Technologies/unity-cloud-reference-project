using Unity.ReferenceProject.Navigation;
using Unity.XR.CoreUtils;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class NavigationVR : NavigationMode
    {
        XROrigin m_XROrigin;

        [Inject]
        public void Setup(XROrigin xrOrigin)
        {
            m_XROrigin = xrOrigin;
        }

        public override void Teleport(Vector3 position, Vector3 eulerAngles)
        {
            m_XROrigin.MoveCameraToWorldLocation(position);
            m_XROrigin.RotateAroundCameraUsingOriginUp(eulerAngles.y);
        }
    }
}
