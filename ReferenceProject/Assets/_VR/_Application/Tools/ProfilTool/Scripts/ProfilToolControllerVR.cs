using Unity.ReferenceProject.Identity;
using UnityEngine;

namespace Unity.ReferenceProject.VR
{
    [RequireComponent(typeof(ProfileToolUIController))]
    public class ProfilToolControllerVR : MonoBehaviour
    {
        void Awake()
        {
            var profileToolUIController = GetComponent<ProfileToolUIController>();
            var logoutState = FindObjectOfType<LogoutState>();
            if (logoutState != null)
            {
                profileToolUIController.LogoutState = logoutState;
            }
        }
    }
}