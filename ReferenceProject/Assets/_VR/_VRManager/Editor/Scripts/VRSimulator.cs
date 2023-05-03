using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Unity.ReferenceProject.VRManager.Editor
{
    public class VRSimulator : UnityEditor.Editor
    {
        [MenuItem("ReferenceProject/VR/Add VR Simulator", false, 10)]
        public static void AddVRSimulator()
        {
            var prefab = AssetDatabase.LoadAssetAtPath("Assets/_VR/_VRManager/Runtime/Prefabs/XR Device Simulator.prefab", typeof(XRDeviceSimulator));
            var simulator = Instantiate(prefab) as XRDeviceSimulator;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            var cameras = FindObjectsOfType<Camera>();
            var xrCamera = cameras.FirstOrDefault(c => c.name == "XRCamera");

            if (simulator != null && xrCamera != null)
            {
                simulator.cameraTransform = xrCamera.transform;
            }
            else
            {
                if (simulator == null)
                {
                    Debug.LogError("XR Device Simulator.prefab not found.");
                }

                if (xrCamera == null)
                {
                    Debug.LogError("XRCamera not found in the scene. There is an instantiated VRManager prefab in it?");
                }
            }
        }
    }
}
