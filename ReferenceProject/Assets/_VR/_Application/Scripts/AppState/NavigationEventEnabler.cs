using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.ReferenceProject.VR
{
    public class NavigationEventEnabler : MonoBehaviour
    {
        [SerializeField]
        bool m_SendNavigationEvents = false;

        void Start()
        {
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem != null)
            {
                eventSystem.sendNavigationEvents = m_SendNavigationEvents;
            }
        }
    }
}
