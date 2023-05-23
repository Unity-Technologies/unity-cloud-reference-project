using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.ReferenceProject.UIPanel.Tests
{
    public class MainUIPanelTests
    {
        readonly TestGameObjects m_TestGameObjects = new();

        [TearDown]
        public void TearDown()
        {
            m_TestGameObjects?.Cleanup();
        }

        [Test]
        public void NoMainUIPanelInScene_ShouldLogError()
        {
            LogAssert.Expect(LogType.Error, $"No instance of {nameof(MainUIPanel)} was found in the current scenes.");
            var mainUIPanel = MainUIPanel.Instance;
            Assert.IsTrue(mainUIPanel == null);
        }

        [Test]
        public void MainUIPanelStaticInstance_ShouldReturnCreatedInstance()
        {
            var mainUIPanel = m_TestGameObjects.NewMainUIPanel(null);
            Assert.AreSame(MainUIPanel.Instance, mainUIPanel);
        }

        [UnityTest]
        public IEnumerator AdditionalMainUIPanel_ShouldLogAndReturnSameInstance()
        {
            var mainUIPanel = m_TestGameObjects.NewMainUIPanel(null);
            var mainUIPanel2 = m_TestGameObjects.NewMainUIPanel(null);

            yield return null;

            LogAssert.Expect(LogType.Log, $"An instance of {nameof(MainUIPanel)} already exists.");

            Assert.AreSame(MainUIPanel.Instance, mainUIPanel);
            Assert.AreSame(MainUIPanel.Instance, mainUIPanel2);
        }

        [UnityTest]
        public IEnumerator DestroyingUIDocument_ShouldReturnNullInstance()
        {
            var mainUIPanel = m_TestGameObjects.NewMainUIPanel(null);
            Assert.AreSame(MainUIPanel.Instance, mainUIPanel);

            Object.Destroy(mainUIPanel.UIDocument);
            yield return null;

            LogAssert.ignoreFailingMessages = true;
            Assert.AreSame(null, MainUIPanel.Instance, "MainUIPanel.Instance must equal null, after mainUIPanel is destroyed");
        }

        [UnityTest]
        public IEnumerator RecreatingMainUIPanelAfterDestroyingUIDocument_ShouldReturnNewInstance()
        {
            var mainUIPanel = m_TestGameObjects.NewMainUIPanel(null);
            Assert.AreNotSame(null, MainUIPanel.Instance, "MainUIPanel.Instance must not be null after creating new MainUIPanel");

            Object.Destroy(mainUIPanel.UIDocument);
            yield return null;

            LogAssert.ignoreFailingMessages = true;
            Assert.AreSame(null, MainUIPanel.Instance, "MainUIPanel.Instance must be null after Destroying mainUIPanel");

            m_TestGameObjects.NewMainUIPanel(null);

            Assert.AreNotSame(null, MainUIPanel.Instance, "MainUIPanel.Instance must not be null, after calling NewMainUIPanel");
        }
    }
}
