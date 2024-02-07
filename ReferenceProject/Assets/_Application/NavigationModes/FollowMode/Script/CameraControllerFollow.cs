using Unity.ReferenceProject.Common;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Application
{
    public class CameraControllerFollow : MonoBehaviour
    {
        ICameraProvider m_CameraProvider;
        IFollowObject m_FollowObject;
        
        [Inject]
        void Setup(ICameraProvider cameraProvider, IFollowObject followObject)
        {
            m_CameraProvider = cameraProvider;
            m_FollowObject = followObject;
        }

        void Update()
        {
            if (m_FollowObject.IsFollowing() && m_CameraProvider.Camera != null)
            {
                m_CameraProvider.Camera.transform
                    .SetPositionAndRotation(m_FollowObject.GetFollowingObjectTransform().position, 
                        m_FollowObject.GetFollowingObjectTransform().rotation);
            }
        }
    }
}
