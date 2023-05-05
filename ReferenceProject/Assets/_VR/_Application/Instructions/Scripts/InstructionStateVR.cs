using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class InstructionStateVR : AppStateListener
    {
        [SerializeField]
        InstructionUIControllerVR m_InstructionUIControllerVRPrefab;

        GameObject m_InstructionsUIControllerVR;
        DiContainer m_DiContainer;

        [Inject]
        void Setup(DiContainer diContainer)
        {
            m_DiContainer = diContainer;
        }

        protected override void StateEntered()
        {
            m_InstructionsUIControllerVR = m_DiContainer.InstantiatePrefab(m_InstructionUIControllerVRPrefab);
        }

        protected override void StateExited()
        {
            if (m_InstructionsUIControllerVR != null)
            {
                Destroy(m_InstructionsUIControllerVR.gameObject);
            }
        }
    }
}
