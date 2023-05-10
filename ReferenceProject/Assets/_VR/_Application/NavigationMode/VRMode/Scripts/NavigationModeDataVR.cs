using System;
using System.Collections;
using Unity.ReferenceProject.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

namespace Unity.ReferenceProject.VR
{
    public class NavigationModeDataVR : NavigationModeData
    {
        // Need this because NavigationModeData is a ScriptableObject and it's not support Coroutine
        public class Coworker : MonoBehaviour
        {
            public void Work(IEnumerator coroutine)
            {
                StartCoroutine(WorkCoroutine(coroutine));
            }

            IEnumerator WorkCoroutine(IEnumerator coroutine)
            {
                yield return StartCoroutine(coroutine);
                DestroyImmediate(gameObject);
            }
        }

        bool m_IsVRDeviceConnected;

#if UNITY_EDITOR
        void OnEnable()
        {
            m_IsVRDeviceConnected = false;

            // use platform dependent compilation so it only exists in editor, otherwise it'll break the build
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (UnityEngine.Application.isPlaying)
                {
                    StartCoroutine(CheckVRCapability());
                }
                else
                {
                    // Application is still changing to playmode, wait until the first scene is loaded
                    // otherwise a GameObject (Coworker) will be created in a weird state and will
                    // cause an error
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            StartCoroutine(CheckVRCapability());
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#else
        void Awake()
        {
            m_IsVRDeviceConnected = false;
            StartCoroutine(CheckVRCapability());
        }
#endif

        static void StartCoroutine(IEnumerator task)
        {
            var coworker = new GameObject("Coworker_" + task).AddComponent<Coworker>();
            coworker.Work(task);
        }

        IEnumerator CheckVRCapability()
        {
            // Wait a frame to be sure XRGeneralSettings.Instance.Manager is created
            yield return null;

            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                m_IsVRDeviceConnected = true;
            }
            else
            {
                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

                if (XRGeneralSettings.Instance.Manager.activeLoader == null)
                {
                    Debug.LogWarning("Initializing XR Failed. No XR device found.");
                }
                else
                {
                    XRGeneralSettings.Instance.Manager.DeinitializeLoader();
                    m_IsVRDeviceConnected = true;
                }
            }
        }

        public override bool CheckDeviceCapability()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return true;
#else
            return false;
#endif
        }

        public override bool CheckDeviceAvailability()
        {
            return m_IsVRDeviceConnected;
        }
    }
}
