using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.ReferenceProject.VRControls.Tests.Editor
{
    public class VRSimulatorTest : MonoBehaviour
    {
        [Test]
        public void VRSimulatorTestPresence()
        {
            var scenes = EditorBuildSettings.scenes;
            foreach(var scenePath in scenes.Select(s=> s.path))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var vrSimulator = FindObjectOfType<XRDeviceSimulator>();
                Assert.IsNull(vrSimulator, $"XR Device Simulator should not be present in the scene {scenePath}");
            }
        }
    }
}
