using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace Unity.ReferenceProject.VRManager
{
    public class VRManager : MonoBehaviour
    {
        [SerializeField]
        Camera m_XRCamera;

        Camera m_ScreenModeCamera;
        float m_ScreenModeFieldOfView;

        void Awake()
        {
            Load();
        }

        void OnDestroy()
        {
            Unload();
        }

        void Load()
        {
            // Swap camera
            m_ScreenModeCamera = Camera.main;

            if (m_ScreenModeCamera != null)
            {
                m_ScreenModeFieldOfView = m_ScreenModeCamera.fieldOfView;
                m_ScreenModeCamera.gameObject.SetActive(false);
            }

            m_XRCamera.tag = "MainCamera";

            InitializeVRLoader();
        }

        void Unload()
        {
            m_XRCamera.tag = "Untagged";

            if (m_ScreenModeCamera != null)
            {
                // SteamVR might change the main FoV, so make sure to set it to the default
                m_ScreenModeCamera.fieldOfView = m_ScreenModeFieldOfView;
                m_ScreenModeCamera.gameObject.SetActive(true);
            }

            DeinitializeVRLoader();
        }

        void InitializeVRLoader()
        {
            if (XRGeneralSettings.Instance == null)
            {
                Debug.LogError("XR Plug-in Management Settings must be setup.");
                return;
            }

            var manager = XRGeneralSettings.Instance.Manager;
            XRLoader vrLoader = null;
            foreach (var loader in manager.activeLoaders)
            {
                if (loader is OpenXRLoader)
                {
                    vrLoader = loader;
                    break;
                }
            }

            if (vrLoader == null)
            {
                return;
            }

            if (manager.activeLoader != null)
            {
                manager.DeinitializeLoader();
            }

#if UNITY_EDITOR
            var loaders = manager.activeLoaders.ToList();
            loaders.Sort((a, b) =>
            {
                if (a is OpenXRLoader)
                    return -1;

                if (b is OpenXRLoader)
                    return 1;

                return 0;
            });

            if (manager.TrySetLoaders(loaders))
            {
                manager.InitializeLoaderSync();
                if (manager.activeLoader != null)
                {
                    manager.activeLoader.Start();
                }
            }
#else
            // start VR subsystems
            if (vrLoader != null)
            {
                vrLoader.Initialize();
                vrLoader.Start();
            }
#endif
        }

        void DeinitializeVRLoader()
        {
            var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
            foreach (var loader in loaders)
            {
                if (loader is OpenXRLoader)
                {
                    loader.Stop();
                    loader.Deinitialize();
                }
            }
        }
    }
}
