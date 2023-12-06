using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Presence;
using Unity.ReferenceProject.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Unity.ReferenceProject
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
