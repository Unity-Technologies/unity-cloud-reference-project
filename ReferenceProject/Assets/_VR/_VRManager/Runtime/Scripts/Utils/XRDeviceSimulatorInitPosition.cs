using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Unity.ReferenceProject.VRManager
{
    [RequireComponent(typeof(XRDeviceSimulator))]
    public class XRDeviceSimulatorInitPosition : MonoBehaviour
    {
        [Header("Positions")]
        [SerializeField]
        Vector3 m_HmdPosition;

        [SerializeField]
        Vector3 m_LeftControllerPosition;

        [SerializeField]
        Vector3 m_RightControllerPosition;

        void Start()
        {
            var deviceSimulator = GetComponent<XRDeviceSimulator>();

            var hmdFieldInfo = typeof(XRDeviceSimulator).GetField("m_HMDState", BindingFlags.NonPublic | BindingFlags.Instance);
            var initValue = $"{{\"leftEyePosition\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0}},\"leftEyeRotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}},\"rightEyePosition\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0}},\"rightEyeRotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}},\"centerEyePosition\":{{\"x\":{m_HmdPosition.x},\"y\":{m_HmdPosition.y},\"z\":{m_HmdPosition.z}}},\"centerEyeRotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}},\"trackingState\":0,\"isTracked\":false,\"devicePosition\":{{\"x\":{m_HmdPosition.x},\"y\":{m_HmdPosition.y},\"z\":{m_HmdPosition.z}}},\"deviceRotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}}}}";
            hmdFieldInfo.SetValue(deviceSimulator, JsonUtility.FromJson<XRSimulatedHMDState>(initValue));

            var leftFieldInfo = typeof(XRDeviceSimulator).GetField("m_LeftControllerState", BindingFlags.NonPublic | BindingFlags.Instance);
            initValue = $"{{\"primary2DAxis\":{{\"x\":0.0,\"y\":0.0}},\"trigger\":0.0,\"grip\":0.0,\"secondary2DAxis\":{{\"x\":0.0,\"y\":0.0}},\"buttons\":0,\"batteryLevel\":0.0,\"trackingState\":0,\"isTracked\":false,\"devicePosition\":{{\"x\":{m_LeftControllerPosition.x},\"y\":{m_LeftControllerPosition.y},\"z\":{m_LeftControllerPosition.z}}},\"deviceRotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}}}}";
            leftFieldInfo.SetValue(deviceSimulator, JsonUtility.FromJson<XRSimulatedControllerState>(initValue));

            var rightFieldInfo = typeof(XRDeviceSimulator).GetField("m_RightControllerState", BindingFlags.NonPublic | BindingFlags.Instance);
            initValue = $"{{\"primary2DAxis\":{{\"x\":0.0,\"y\":0.0}},\"trigger\":0.0,\"grip\":0.0,\"secondary2DAxis\":{{\"x\":0.0,\"y\":0.0}},\"buttons\":0,\"batteryLevel\":0.0,\"trackingState\":0,\"isTracked\":false,\"devicePosition\":{{\"x\":{m_RightControllerPosition.x},\"y\":{m_RightControllerPosition.y},\"z\":{m_RightControllerPosition.z}}},\"deviceRotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}}}}";
            rightFieldInfo.SetValue(deviceSimulator, JsonUtility.FromJson<XRSimulatedControllerState>(initValue));
        }
    }
}
