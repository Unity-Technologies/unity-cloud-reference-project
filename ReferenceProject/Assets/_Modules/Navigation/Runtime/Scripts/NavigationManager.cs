using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Navigation
{
    public interface INavigationManager
    {
        NavigationModeData[] NavigationModes { get; }
        NavigationModeData CurrentNavigationModeData { get; }
        event Action NavigationModeChanged;
        void ChangeNavigationMode(int idMode);
        void TryTeleport(Vector3 position, Vector3 eulerAngles);
    }

    public class NavigationManager : MonoBehaviour, INavigationManager
    {
        [SerializeField]
        NavigationModeData[] m_NavigationModes;

        NavigationMode m_CurrentMode;
        NavigationModeData m_CurrentNavigationModeData;

        DiContainer m_DiContainer;

        [Inject]
        void Setup(DiContainer diContainer)
        {
            m_DiContainer = diContainer;
        }

        void Start() => ChangeNavigationMode(m_NavigationModes.First()); // Default navigation mode

        public NavigationModeData[] NavigationModes => m_NavigationModes;
        public NavigationModeData CurrentNavigationModeData => m_CurrentNavigationModeData;
        public event Action NavigationModeChanged;

        public void ChangeNavigationMode(int idMode)
        {
            if (m_NavigationModes != null && idMode >= 0 && idMode < m_NavigationModes.Length)
                ChangeNavigationMode(m_NavigationModes[idMode]);
        }

        public void TryTeleport(Vector3 position, Vector3 eulerAngles)
        {
            if (m_CurrentMode != null)
            {
                m_CurrentMode.Teleport(position, eulerAngles);
            }
        }

        void ChangeNavigationMode(NavigationModeData targetNavigationMode)
        {
            if (targetNavigationMode == m_CurrentNavigationModeData)
                return;

            if (!targetNavigationMode.CheckDeviceAvailability())
            {
                Debug.LogWarning($"The device is not compatible with {targetNavigationMode.name}");
                return;
            }

            if (targetNavigationMode.Prefab == null)
            {
                Debug.LogError($"Null reference to prefab on {targetNavigationMode.name}");
                return;
            }

            if (m_CurrentMode != null)
                Destroy(m_CurrentMode.gameObject);

            m_CurrentMode = m_DiContainer.InstantiatePrefabForComponent<NavigationMode>(targetNavigationMode.Prefab);

            m_CurrentNavigationModeData = targetNavigationMode;
            NavigationModeChanged?.Invoke();
        }
    }
}
