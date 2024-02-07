using System;
using System.Linq;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Presence;
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
        void ChangeNavigationMode(int modeID);
        void ChangeNavigationMode(string modeName);
        void TryTeleport(Vector3 position, Vector3 eulerAngles);
        void ChangeCameraView(CameraViewData cameraViewData);
        void SetDefaultPosition(Vector3 position, Vector3 rotation);
    }

    public class NavigationManager : MonoBehaviour, INavigationManager
    {
        [SerializeField]
        NavigationModeData[] m_NavigationModes;

        [SerializeField]
        NavigationModeData m_DefaultFollowNavigationMode;
        
        [SerializeField]
        CameraViewData[] m_CameraViewData;

        NavigationMode m_CurrentMode;
        NavigationModeData m_CurrentNavigationModeData;
        NavigationModeData m_BeforeFollowNavigationModeData;

        DiContainer m_DiContainer;
        ICameraProvider m_CameraProvider;
        IDataStreamBound m_DataStreamBound;
        IFollowManager m_FollowManager;
        Vector3 m_DefaultCameraPosition;
        Vector3 m_DefaultCameraRotation;

        [Inject]
        void Setup(DiContainer diContainer, ICameraProvider cameraProvider, IDataStreamBound dataStreamBound, 
            IFollowManager followManager)
        {
            m_DiContainer = diContainer;
            m_CameraProvider = cameraProvider;
            m_DataStreamBound = dataStreamBound;
            m_FollowManager = followManager;
        }

        void Awake()
        {
            m_FollowManager.EnterFollowMode += OnEnterFollowMode;
            m_FollowManager.ExitFollowMode += OnExitFollowMode;
        }

        void OnDestroy()
        {
            if(m_FollowManager != null)
            {
                m_FollowManager.EnterFollowMode -= OnEnterFollowMode;
                m_FollowManager.ExitFollowMode -= OnExitFollowMode;
            }
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
        
        public void ChangeNavigationMode(int modeID)
        {
            if (m_NavigationModes != null && modeID >= 0 && modeID < m_NavigationModes.Length)
            {
                ChangeNavigationMode(m_NavigationModes[modeID]);
            }
        }

        public void ChangeNavigationMode(string modeName)
        {
            if (m_NavigationModes != null)
            {
                if (modeName == m_DefaultFollowNavigationMode.name)
                {
                    ChangeNavigationMode(m_DefaultFollowNavigationMode);
                }
                else
                {
                    foreach (var mode in m_NavigationModes)
                    {
                        if (modeName == mode.name)
                        {
                            ChangeNavigationMode(mode);
                            break;
                        }
                    }
                }
            }
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
            {
                if(m_CurrentMode.GetType() == m_DefaultFollowNavigationMode.Prefab.GetType() && 
                   targetNavigationMode.GetType() != m_DefaultFollowNavigationMode.Prefab.GetType())
                {
                    m_FollowManager.StopFollowMode();
                }
                Destroy(m_CurrentMode.gameObject);
            }
            
            m_CurrentMode = m_DiContainer.InstantiatePrefabForComponent<NavigationMode>(targetNavigationMode.Prefab);

            m_CurrentNavigationModeData = targetNavigationMode;
            NavigationModeChanged?.Invoke();
        }
        
        public void ChangeCameraView(CameraViewData cameraViewData)
        {
            var bound = m_DataStreamBound.GetBound();
            if (cameraViewData is CameraViewDataDefault { UseDefaultView: true })
            { 
                TryTeleport(m_DefaultCameraPosition, m_DefaultCameraRotation);
            }
            else
            { 
                var distance = m_DataStreamBound.GetDistanceVisibleFromCenter(m_CameraProvider.Camera);
                var desiredRotation = Quaternion.Euler(cameraViewData.Rotation);
                var backwardMovement = -(desiredRotation * Vector3.forward) * distance;
                TryTeleport(bound.center + backwardMovement,  cameraViewData.Rotation);
            }
        }

        void OnEnterFollowMode(IParticipant participant, bool isPresentation)
        {
            m_BeforeFollowNavigationModeData = m_CurrentNavigationModeData;
            ChangeNavigationMode("Follow");
        }

        void OnExitFollowMode()
        {
            if (m_BeforeFollowNavigationModeData)
            {
                ChangeNavigationMode(m_BeforeFollowNavigationModeData.name);
            }
        }
    }
}
