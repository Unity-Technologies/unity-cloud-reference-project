using System;
using System.Linq;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Navigation
{
    public interface INavigationManager
    {
        NavigationModeData[] NavigationModes { get; }
        NavigationModeData CurrentNavigationModeData { get; }
        CameraViewData[] CameraViewModeData { get; }
        event Action NavigationModeChanged;
        void ChangeNavigationMode(int idMode);
        void TryTeleport(Vector3 position, Vector3 eulerAngles);
        void ChangeCameraView(CameraViewData cameraViewData);
        void SetDefaultPosition(Vector3 position, Vector3 rotation);
    }

    public class NavigationManager : MonoBehaviour, INavigationManager
    {
        [SerializeField]
        NavigationModeData[] m_NavigationModes;

        [SerializeField]
        CameraViewData[] m_CameraViewData;

        NavigationMode m_CurrentMode;
        NavigationModeData m_CurrentNavigationModeData;

        DiContainer m_DiContainer;
        Camera m_StreamingCamera;
        IDataStreamBound m_DataStreamBound;
        Vector3 m_DefaultCameraPosition;
        Vector3 m_DefaultCameraRotation;

        [Inject]
        void Setup(DiContainer diContainer, Camera streamingCamera, IDataStreamBound dataStreamBound)
        {
            m_DiContainer = diContainer;
            m_StreamingCamera = streamingCamera;
            m_DataStreamBound = dataStreamBound;
        }

        void Start() => ChangeNavigationMode(m_NavigationModes.First()); // Default navigation mode

        public NavigationModeData[] NavigationModes => m_NavigationModes;
        public NavigationModeData CurrentNavigationModeData => m_CurrentNavigationModeData;
        public event Action NavigationModeChanged;
        
        public CameraViewData[] CameraViewModeData => m_CameraViewData;

        public void SetDefaultPosition(Vector3 position, Vector3 rotation)
        {
            m_DefaultCameraPosition = position;
            m_DefaultCameraRotation = rotation;
        }
        
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

        public void ChangeCameraView(CameraViewData cameraViewData)
        {
            Bounds bound = m_DataStreamBound.GetBound();
            if(cameraViewData is CameraViewDataDefault  && ((CameraViewDataDefault)cameraViewData).UseDefaultView)
            { 
                TryTeleport(m_DefaultCameraPosition, m_DefaultCameraRotation);
            }
            else
            { 
                float distance = m_DataStreamBound.GetDistanceVisibleFromCenter(m_StreamingCamera);
                Quaternion desiredRotation = Quaternion.Euler(cameraViewData.Rotation);
                Vector3 backwardMovement = -(desiredRotation * Vector3.forward) * distance;
                TryTeleport(bound.center + backwardMovement,  cameraViewData.Rotation);
            }
        }
    }
}
